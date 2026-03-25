using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Email;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that automatically generates recurring invoices based on their schedules.
    /// Runs continuously in the background and processes due invoices at regular intervals.
    /// </summary>
    public class RecurringInvoiceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringInvoiceBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour
        private const int MaxRetryCount = 3; // Maximum retry attempts for failed generations

        public RecurringInvoiceBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RecurringInvoiceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Main execution loop of the background service
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Invoice Background Service started at: {time}", DateTime.UtcNow);

            // Add initial delay to allow application to fully start
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime startTime = DateTime.UtcNow;
                try
                {
                    _logger.LogDebug("Starting recurring invoice check cycle at {time}", startTime);

                    // Process due invoices
                    await ProcessDueRecurringInvoices(stoppingToken);

                    // Process failed retries
                    await ProcessFailedGenerations(stoppingToken);

                    // Log completion
                    TimeSpan duration = DateTime.UtcNow - startTime;
                    _logger.LogDebug("Completed recurring invoice check cycle in {duration}ms at {time}",
                        duration.TotalMilliseconds, DateTime.UtcNow);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Recurring invoice background service was cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in recurring invoice background service at {time}",
                        DateTime.UtcNow);
                }

                // Wait for next check interval
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Recurring invoice background service was cancelled during delay.");
                    break;
                }
            }

            _logger.LogInformation("Recurring Invoice Background Service stopped at: {time}", DateTime.UtcNow);
        }

        /// <summary>
        /// Processes all recurring invoices that are due for generation
        /// </summary>
        private async Task ProcessDueRecurringInvoices(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringInvoiceService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RecurringInvoiceBackgroundService>>();

            try
            {
                var today = DateTime.UtcNow.Date;

                // Get all due recurring invoices
                var dueInvoices = await unitOfWork.RecurringInvoices
                    .Query()
                    .Where(r => !r.IsDeleted &&
                                r.Status == RecurringInvoiceStatus.Active &&
                                r.NextInvoiceDate.Date <= today &&
                                (!r.EndDate.HasValue || r.EndDate.Value.Date >= today) &&
                                (!r.MaxOccurrences.HasValue || r.OccurrencesGenerated < r.MaxOccurrences.Value))
                    .Include(r => r.Customer)
                    .Include(r => r.Company)
                    .ToListAsync(stoppingToken);

                if (!dueInvoices.Any())
                {
                    logger.LogDebug("No due recurring invoices found for {Today}.", today.ToString("yyyy-MM-dd"));
                    return;
                }

                logger.LogInformation("Found {Count} due recurring invoices to process for {Today}.",
                    dueInvoices.Count, today.ToString("yyyy-MM-dd"));

                int successCount = 0;
                int failureCount = 0;

                foreach (var recurringInvoice in dueInvoices)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        bool success = await ProcessSingleRecurringInvoice(recurringInvoice, unitOfWork, recurringService, logger, stoppingToken);

                        if (success)
                            successCount++;
                        else
                            failureCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        logger.LogError(ex, "Unhandled exception processing recurring invoice {RecurringInvoiceId} - {Name}",
                            recurringInvoice.Id, recurringInvoice.Name);

                        // Mark as failed for retry
                        await MarkGenerationFailed(unitOfWork, recurringInvoice.Id, ex.Message, logger, stoppingToken);
                    }
                }

                logger.LogInformation("Recurring invoice processing completed: {Success} succeeded, {Failed} failed out of {Total}",
                    successCount, failureCount, dueInvoices.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing due recurring invoices.");
            }
        }

        /// <summary>
        /// Processes a single recurring invoice for generation
        /// </summary>
        private async Task<bool> ProcessSingleRecurringInvoice(
            RecurringInvoice recurringInvoice,
            IUnitOfWork unitOfWork,
            IRecurringInvoiceService recurringService,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Processing recurring invoice {RecurringInvoiceId} - {Name} for customer {CustomerId}",
                    recurringInvoice.Id, recurringInvoice.Name, recurringInvoice.CustomerId);

                // Create operation context for background process (system user)
                var operationContext = new OperationContext(
                    userId: "system",
                    companyId: recurringInvoice.CompanyId,
                    customerId: recurringInvoice.CustomerId
                );

                // Calculate generation date (with advance days if configured)
                DateTime invoiceDate = DateTime.UtcNow;
                if (recurringInvoice.GenerateInAdvanceDays > 0)
                {
                    invoiceDate = DateTime.UtcNow.AddDays(recurringInvoice.GenerateInAdvanceDays);
                }

                // Prepare generation DTO
                var generateDto = new GenerateManualDto
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    InvoiceDate = invoiceDate,
                    OverrideNextDate = true,
                    SendImmediately = recurringInvoice.AutoSend,
                    GenerationNotes = $"Auto-generated by system on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                };

                // Generate the invoice using the recurring service
                var result = await recurringService.GenerateInvoiceManuallyAsync(generateDto, operationContext);

                if (result.IsSuccess)
                {
                    logger.LogInformation(
                        "Successfully generated invoice {InvoiceNumber} (ID: {InvoiceId}) from recurring template {RecurringInvoiceId}",
                        result.Data.InvoiceNumber, result.Data.Id, recurringInvoice.Id);

                    // Send email notification if auto-email is enabled
                    if (recurringInvoice.AutoEmail)
                    {
                        await SendInvoiceEmailNotification(result.Data, recurringInvoice, unitOfWork, logger, cancellationToken);
                    }

                    // Log successful generation
                    await LogGenerationAudit(unitOfWork, recurringInvoice.Id, result.Data.Id,
                        GenerationStatus.Success, null, logger, cancellationToken);

                    return true;
                }
                else
                {
                    logger.LogError(
                        "Failed to generate invoice from recurring template {RecurringInvoiceId}: {ErrorMessage}",
                        recurringInvoice.Id, result.ErrorMessage);

                    // Mark as failed for retry
                    await MarkGenerationFailed(unitOfWork, recurringInvoice.Id, result.ErrorMessage, logger, cancellationToken);

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while generating invoice from recurring template {RecurringInvoiceId}",
                    recurringInvoice.Id);

                await MarkGenerationFailed(unitOfWork, recurringInvoice.Id, ex.Message, logger, cancellationToken);
                return false;
            }
        }

        /// <summary>
        /// Processes failed generations for retry
        /// </summary>
        private async Task ProcessFailedGenerations(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringInvoiceService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RecurringInvoiceBackgroundService>>();

            try
            {
                // Get instances with failed status that are eligible for retry
                var retryTime = DateTime.UtcNow.AddMinutes(-15); // Retry after 15 minutes

                var failedInstances = await unitOfWork.RecurringInvoiceInstances
                    .Query()
                    .Where(i => !i.IsDeleted &&
                                i.GenerationStatus == GenerationStatus.Failed &&
                                i.RetryCount < MaxRetryCount &&
                                i.CreatedDate <= retryTime)
                    .Include(i => i.RecurringInvoice)
                    .ToListAsync(stoppingToken);

                if (!failedInstances.Any())
                {
                    return;
                }

                logger.LogInformation("Found {Count} failed recurring invoice generations to retry", failedInstances.Count);

                foreach (var instance in failedInstances)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Increment retry count
                        instance.RetryCount++;

                        // Create operation context
                        var operationContext = new OperationContext(
                            userId: "system",
                            companyId: instance.RecurringInvoice.CompanyId,
                            customerId: instance.RecurringInvoice.CustomerId
                        );

                        // Prepare generation DTO
                        var generateDto = new GenerateManualDto
                        {
                            RecurringInvoiceId = instance.RecurringInvoiceId,
                            InvoiceDate = DateTime.UtcNow,
                            OverrideNextDate = true,
                            SendImmediately = instance.RecurringInvoice.AutoSend,
                            GenerationNotes = $"Retry attempt #{instance.RetryCount} on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        };

                        // Retry generation
                        var result = await recurringService.GenerateInvoiceManuallyAsync(generateDto, operationContext);

                        if (result.IsSuccess)
                        {
                            // Update instance status to success (soft delete the failed record)
                            instance.IsDeleted = true;
                            instance.UpdatedBy = "system";
                            instance.UpdatedDate = DateTime.UtcNow;

                            unitOfWork.RecurringInvoiceInstances.Update(instance);
                            await unitOfWork.SaveChangesAsync(stoppingToken);

                            logger.LogInformation(
                                "Successfully retried generation for recurring invoice {RecurringInvoiceId} on attempt #{RetryCount}",
                                instance.RecurringInvoiceId, instance.RetryCount);
                        }
                        else if (instance.RetryCount >= MaxRetryCount)
                        {
                            // Max retries reached, mark as permanently failed
                            await MarkGenerationPermanentlyFailed(unitOfWork, instance, result.ErrorMessage, logger, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error retrying failed generation for instance {InstanceId}", instance.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing failed generations for retry");
            }
        }

        /// <summary>
        /// Marks a generation as failed for retry logic
        /// </summary>
        private async Task MarkGenerationFailed(
            IUnitOfWork unitOfWork,
            int recurringInvoiceId,
            string errorMessage,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // Create a failed instance record for retry tracking
                var instance = new RecurringInvoiceInstance
                {
                    RecurringInvoiceId = recurringInvoiceId,
                    InvoiceId = 0, // No invoice generated
                    GeneratedDate = DateTime.UtcNow,
                    ScheduledGenerationDate = DateTime.UtcNow,
                    SequenceNumber = 0,
                    GeneratedInvoiceNumber = "FAILED",
                    Amount = 0,
                    Notes = $"Generation failed: {errorMessage}",
                    GenerationStatus = GenerationStatus.Failed,
                    ErrorMessage = errorMessage,
                    RetryCount = 0,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };

                await unitOfWork.RecurringInvoiceInstances.AddAsync(instance);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogWarning("Marked recurring invoice {RecurringInvoiceId} as failed for retry. Error: {ErrorMessage}",
                    recurringInvoiceId, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error marking generation as failed for recurring invoice {RecurringInvoiceId}",
                    recurringInvoiceId);
            }
        }

        /// <summary>
        /// Marks a generation as permanently failed after max retries
        /// </summary>
        private async Task MarkGenerationPermanentlyFailed(
            IUnitOfWork unitOfWork,
            RecurringInvoiceInstance instance,
            string errorMessage,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                instance.Notes = $"Permanently failed after {MaxRetryCount} attempts. Last error: {errorMessage}";
                instance.GenerationStatus = GenerationStatus.Failed; // Keep as failed, no more retries
                instance.UpdatedBy = "system";
                instance.UpdatedDate = DateTime.UtcNow;

                unitOfWork.RecurringInvoiceInstances.Update(instance);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Also create an audit log for the recurring invoice
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = instance.RecurringInvoiceId,
                    Action = "GenerationFailed",
                    Description = $"Invoice generation permanently failed after {MaxRetryCount} attempts",
                    Changes = JsonSerializer.Serialize(new
                    {
                        Error = errorMessage,
                        LastAttempt = DateTime.UtcNow,
                        RetryCount = instance.RetryCount
                    }),
                    IpAddress = "system",
                    UserAgent = "background-service",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };

                await unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogWarning("Recurring invoice {RecurringInvoiceId} permanently failed after {RetryCount} attempts",
                    instance.RecurringInvoiceId, instance.RetryCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error marking generation as permanently failed for recurring invoice {RecurringInvoiceId}",
                    instance.RecurringInvoiceId);
            }
        }

        /// <summary>
        /// Sends email notification for generated invoice
        /// </summary>
        private async Task SendInvoiceEmailNotification(
            InvoiceResponseDto invoice,
            RecurringInvoice recurringInvoice,
            IUnitOfWork unitOfWork,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get customer using GetAsync instead of GetByIdAsync
                var customer = await unitOfWork.Customers.GetAsync(
                    c => c.Id == recurringInvoice.CustomerId && !c.IsDeleted);

                if (customer == null || string.IsNullOrEmpty(customer.Email))
                {
                    logger.LogWarning("Cannot send email for invoice {InvoiceId}: Customer email not found",
                        invoice.Id);
                    return;
                }

                // Create email data
                var emailData = new EmailDataDto
                {
                    To = new List<string> { customer.Email },
                    Subject = $"Your recurring invoice {invoice.InvoiceNumber} is ready",
                    Message = GenerateEmailMessage(invoice, recurringInvoice),
                    AttachPdf = true
                };

                // Note: You'll need to implement email sending logic here
                // await emailService.SendInvoiceEmailAsync(invoice.Id, emailData);

                logger.LogInformation("Email notification ready for generated invoice {InvoiceNumber} to {Email}",
                    invoice.InvoiceNumber, customer.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error preparing email notification for invoice {InvoiceId}", invoice.Id);
            }
        }

        /// <summary>
        /// Logs generation audit trail
        /// </summary>
        private async Task LogGenerationAudit(
            IUnitOfWork unitOfWork,
            int recurringInvoiceId,
            int invoiceId,
            GenerationStatus status,
            string? errorMessage,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoiceId,
                    Action = status == GenerationStatus.Success ? "Generated" : "GenerationFailed",
                    Description = status == GenerationStatus.Success
                        ? $"Invoice {invoiceId} generated successfully"
                        : $"Generation failed: {errorMessage}",
                    Changes = JsonSerializer.Serialize(new
                    {
                        InvoiceId = invoiceId,
                        Status = status.ToString(),
                        Timestamp = DateTime.UtcNow
                    }),
                    IpAddress = "system",
                    UserAgent = "background-service",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };

                await unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error logging generation audit for recurring invoice {RecurringInvoiceId}",
                    recurringInvoiceId);
            }
        }

        /// <summary>
        /// Generates HTML email message for invoice
        /// </summary>
        private string GenerateEmailMessage(InvoiceResponseDto invoice, RecurringInvoice recurring)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .invoice-details {{ background-color: #f5f5f5; padding: 15px; border-radius: 5px; }}
                        .total {{ font-size: 18px; font-weight: bold; color: #2c3e50; }}
                    </style>
                </head>
                <body>
                    <h2>Your recurring invoice is ready</h2>
                    <p>Dear {invoice.Customer?.Name},</p>
                    <p>Your recurring invoice has been generated based on your {recurring.Frequency.ToString().ToLower()} schedule.</p>
                    
                    <div class='invoice-details'>
                        <h3>Invoice Details</h3>
                        <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                        <p><strong>Issue Date:</strong> {invoice.IssueDate:dd MMM yyyy}</p>
                        <p><strong>Due Date:</strong> {invoice.DueDate:dd MMM yyyy}</p>
                        <p class='total'><strong>Total Amount:</strong> {invoice.TotalAmount:C} {invoice.Currency}</p>
                    </div>
                    
                    <p>Please find the PDF invoice attached to this email.</p>
                    <p>If you have any questions, please don't hesitate to contact us.</p>
                    
                    <p>Thank you for your business!</p>
                </body>
                </html>";
        }

        /// <summary>
        /// Override of StopAsync for graceful shutdown
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recurring Invoice Background Service is stopping at: {time}", DateTime.UtcNow);
            await base.StopAsync(cancellationToken);
        }
    }
}