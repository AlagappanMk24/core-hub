using AutoMapper;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Application.Contracts.Persistence.Payments;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Integration.Payments
{
    public class PaymentService(
        IUnitOfWork unitOfWork,
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        IMapper mapper,
        ILogger<PaymentService> logger) 
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<PaymentService> _logger = logger;

        //public async Task<OperationResult<PaymentResponseDto>> GetPaymentByIdAsync(int id)
        //{
        //    try
        //    {
        //        var payment = await _paymentRepository.GetPaymentWithDetailsAsync(id);
        //        if (payment == null)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Payment not found");
        //        }

        //        var response = _mapper.Map<PaymentResponseDto>(payment);
        //        return OperationResult<PaymentResponseDto>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting payment {PaymentId}", id);
        //        return OperationResult<PaymentResponseDto>.FailureResult("Failed to retrieve payment");
        //    }
        //}

        //public async Task<OperationResult<PaginatedResult<PaymentResponseDto>>> GetPagedPaymentsAsync(
        //    PaymentFilterDto filter, OperationContext context)
        //{
        //    try
        //    {
        //        var query = _unitOfWork.Payments.Query()
        //            .Where(p => !p.IsDeleted);

        //        // Apply company filter
        //        if (context.CompanyId.HasValue)
        //        {
        //            query = query.Where(p => p.CompanyId == context.CompanyId.Value);
        //        }

        //        // Apply customer filter
        //        if (context.CustomerId.HasValue)
        //        {
        //            query = query.Where(p => p.CustomerId == context.CustomerId.Value);
        //        }

        //        // Apply filters
        //        if (filter.InvoiceId.HasValue)
        //        {
        //            query = query.Where(p => p.InvoiceId == filter.InvoiceId.Value);
        //        }

        //        if (filter.CustomerId.HasValue)
        //        {
        //            query = query.Where(p => p.CustomerId == filter.CustomerId.Value);
        //        }

        //        if (!string.IsNullOrEmpty(filter.PaymentStatus))
        //        {
        //            if (Enum.TryParse<PaymentStatus>(filter.PaymentStatus, true, out var status))
        //            {
        //                query = query.Where(p => p.PaymentStatus == status);
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(filter.PaymentMethod))
        //        {
        //            query = query.Where(p => p.PaymentMethod == filter.PaymentMethod);
        //        }

        //        if (filter.FromDate.HasValue)
        //        {
        //            query = query.Where(p => p.PaymentDate >= filter.FromDate.Value);
        //        }

        //        if (filter.ToDate.HasValue)
        //        {
        //            query = query.Where(p => p.PaymentDate <= filter.ToDate.Value);
        //        }

        //        if (filter.MinAmount.HasValue)
        //        {
        //            query = query.Where(p => p.Amount >= filter.MinAmount.Value);
        //        }

        //        if (filter.MaxAmount.HasValue)
        //        {
        //            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);
        //        }

        //        if (!string.IsNullOrEmpty(filter.Search))
        //        {
        //            query = query.Where(p =>
        //                p.PaymentNumber.Contains(filter.Search) ||
        //                p.Invoice.InvoiceNumber.Contains(filter.Search) ||
        //                p.Customer.Name.Contains(filter.Search));
        //        }

        //        var totalCount = await query.CountAsync();
        //        var items = await query
        //            .Include(p => p.Invoice)
        //            .Include(p => p.Customer)
        //            .OrderByDescending(p => p.PaymentDate)
        //            .Skip((filter.PageNumber - 1) * filter.PageSize)
        //            .Take(filter.PageSize)
        //            .ToListAsync();

        //        var responseItems = _mapper.Map<List<PaymentResponseDto>>(items);

        //        var result = new PaginatedResult<PaymentResponseDto>
        //        {
        //            Items = responseItems,
        //            TotalCount = totalCount,
        //            PageNumber = filter.PageNumber,
        //            PageSize = filter.PageSize,
        //            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        //        };

        //        return OperationResult<PaginatedResult<PaymentResponseDto>>.SuccessResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting paged payments");
        //        return OperationResult<PaginatedResult<PaymentResponseDto>>.FailureResult("Failed to retrieve payments");
        //    }
        //}

        //public async Task<OperationResult<PaymentResponseDto>> CreatePaymentAsync(
        //    CreatePaymentDto dto, OperationContext context)
        //{
        //    try
        //    {
        //        // Validate invoice exists
        //        var invoice = await _invoiceRepository.GetByIdAsync(dto.InvoiceId);
        //        if (invoice == null)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Invoice not found");
        //        }

        //        // Validate amount doesn't exceed invoice balance
        //        var totalPaid = await _paymentRepository.GetTotalPaymentsForInvoiceAsync(dto.InvoiceId);
        //        var remainingBalance = invoice.TotalAmount - totalPaid;

        //        if (dto.Amount > remainingBalance + 0.01m) // Small tolerance for rounding
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult(
        //                $"Payment amount exceeds remaining balance. Remaining: ${remainingBalance}");
        //        }

        //        var payment = _mapper.Map<Payment>(dto);
        //        payment.PaymentNumber = await _paymentRepository.GenerateNextPaymentNumberAsync(context.CompanyId.Value);
        //        payment.CustomerId = invoice.CustomerId;
        //        payment.CompanyId = context.CompanyId.Value;
        //        payment.PaymentStatus = PaymentStatus.Paid;
        //        payment.PaymentType = dto.Amount >= invoice.TotalAmount - totalPaid - 0.01m ?
        //            PaymentType.Full : PaymentType.Partial;
        //        payment.AppliedAmount = dto.Amount;
        //        payment.UnappliedAmount = 0;
        //        payment.ProcessedBy = context.UserId;
        //        payment.CreatedBy = context.UserId;
        //        payment.CreatedDate = DateTime.UtcNow;

        //        await _paymentRepository.AddAsync(payment);
        //        await _unitOfWork.SaveChangesAsync();

        //        // Update invoice payment status
        //        await UpdateInvoicePaymentStatus(invoice.Id);

        //        var response = _mapper.Map<PaymentResponseDto>(payment);
        //        return OperationResult<PaymentResponseDto>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating payment for invoice {InvoiceId}", dto.InvoiceId);
        //        return OperationResult<PaymentResponseDto>.FailureResult("Failed to create payment");
        //    }
        //}

        //public async Task<OperationResult<PaymentResponseDto>> ProcessPaymentAsync(
        //    ProcessPaymentDto dto, OperationContext context)
        //{
        //    try
        //    {
        //        // Validate invoice exists
        //        var invoice = await _invoiceRepository.GetByIdAsync(dto.InvoiceId);
        //        if (invoice == null)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Invoice not found");
        //        }

        //        // Here you would integrate with payment gateway (Stripe, PayPal, etc.)
        //        // For now, we'll just record the payment

        //        var payment = new Payment
        //        {
        //            InvoiceId = dto.InvoiceId,
        //            CustomerId = invoice.CustomerId,
        //            CompanyId = context.CompanyId.Value,
        //            Amount = dto.Amount,
        //            PaymentDate = DateTime.UtcNow,
        //            PaymentMethod = dto.PaymentMethod,
        //            PaymentStatus = PaymentStatus.Paid,
        //            PaymentType = PaymentType.Full,
        //            AppliedAmount = dto.Amount,
        //            ProcessedBy = context.UserId,
        //            CreatedBy = context.UserId,
        //            CreatedDate = DateTime.UtcNow
        //        };

        //        payment.PaymentNumber = await _paymentRepository.GenerateNextPaymentNumberAsync(context.CompanyId.Value);

        //        await _paymentRepository.AddAsync(payment);
        //        await _unitOfWork.SaveChangesAsync();

        //        // Update invoice payment status
        //        await UpdateInvoicePaymentStatus(invoice.Id);

        //        var response = _mapper.Map<PaymentResponseDto>(payment);
        //        return OperationResult<PaymentResponseDto>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", dto.InvoiceId);
        //        return OperationResult<PaymentResponseDto>.FailureResult("Failed to process payment");
        //    }
        //}

        //public async Task<OperationResult<PaymentResponseDto>> RefundPaymentAsync(
        //    RefundPaymentDto dto, OperationContext context)
        //{
        //    try
        //    {
        //        var originalPayment = await _paymentRepository.GetByIdAsync(dto.PaymentId);
        //        if (originalPayment == null)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Payment not found");
        //        }

        //        if (originalPayment.PaymentStatus != PaymentStatus.Paid)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Only paid payments can be refunded");
        //        }

        //        if (dto.Amount > originalPayment.Amount)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Refund amount exceeds payment amount");
        //        }

        //        var refund = new Payment
        //        {
        //            InvoiceId = originalPayment.InvoiceId,
        //            CustomerId = originalPayment.CustomerId,
        //            CompanyId = originalPayment.CompanyId,
        //            Amount = dto.Amount,
        //            PaymentDate = DateTime.UtcNow,
        //            PaymentMethod = originalPayment.PaymentMethod,
        //            PaymentStatus = PaymentStatus.Refunded,
        //            PaymentType = originalPayment.PaymentType,
        //            IsRefund = true,
        //            OriginalPaymentId = originalPayment.Id,
        //            InternalNotes = dto.Reason,
        //            ProcessedBy = context.UserId,
        //            CreatedBy = context.UserId,
        //            CreatedDate = DateTime.UtcNow
        //        };

        //        refund.PaymentNumber = await _paymentRepository.GenerateNextPaymentNumberAsync(context.CompanyId.Value);

        //        await _paymentRepository.AddAsync(refund);

        //        // Update original payment status
        //        originalPayment.PaymentStatus = dto.Amount >= originalPayment.Amount ?
        //            PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        //        _paymentRepository.Update(originalPayment);

        //        await _unitOfWork.SaveChangesAsync();

        //        // Update invoice payment status
        //        await UpdateInvoicePaymentStatus(originalPayment.InvoiceId);

        //        var response = _mapper.Map<PaymentResponseDto>(refund);
        //        return OperationResult<PaymentResponseDto>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error refunding payment {PaymentId}", dto.PaymentId);
        //        return OperationResult<PaymentResponseDto>.FailureResult("Failed to process refund");
        //    }
        //}

        //public async Task<OperationResult<PaymentResponseDto>> UpdatePaymentAsync(
        //    UpdatePaymentDto dto, OperationContext context)
        //{
        //    try
        //    {
        //        var payment = await _paymentRepository.GetByIdAsync(dto.Id);
        //        if (payment == null)
        //        {
        //            return OperationResult<PaymentResponseDto>.FailureResult("Payment not found");
        //        }

        //        _mapper.Map(dto, payment);
        //        payment.UpdatedBy = context.UserId;
        //        payment.UpdatedDate = DateTime.UtcNow;

        //        _paymentRepository.Update(payment);
        //        await _unitOfWork.SaveChangesAsync();

        //        // Update invoice payment status if amount changed
        //        if (dto.Amount.HasValue)
        //        {
        //            await UpdateInvoicePaymentStatus(payment.InvoiceId);
        //        }

        //        var response = _mapper.Map<PaymentResponseDto>(payment);
        //        return OperationResult<PaymentResponseDto>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating payment {PaymentId}", dto.Id);
        //        return OperationResult<PaymentResponseDto>.FailureResult("Failed to update payment");
        //    }
        //}

        //public async Task<OperationResult<bool>> DeletePaymentAsync(int id, OperationContext context)
        //{
        //    try
        //    {
        //        var payment = await _paymentRepository.GetByIdAsync(id);
        //        if (payment == null)
        //        {
        //            return OperationResult<bool>.FailureResult("Payment not found");
        //        }

        //        payment.IsDeleted = true;
        //        payment.UpdatedBy = context.UserId;
        //        payment.UpdatedDate = DateTime.UtcNow;

        //        _paymentRepository.Update(payment);
        //        await _unitOfWork.SaveChangesAsync();

        //        // Update invoice payment status
        //        await UpdateInvoicePaymentStatus(payment.InvoiceId);

        //        return OperationResult<bool>.SuccessResult(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
        //        return OperationResult<bool>.FailureResult("Failed to delete payment");
        //    }
        //}

        //public async Task<OperationResult<PaymentStatsDto>> GetPaymentStatsAsync(OperationContext context)
        //{
        //    try
        //    {
        //        var query = _unitOfWork.Payments.Query()
        //            .Where(p => !p.IsDeleted && p.PaymentStatus == PaymentStatus.Completed);

        //        if (context.CompanyId.HasValue)
        //        {
        //            query = query.Where(p => p.CompanyId == context.CompanyId.Value);
        //        }

        //        if (context.CustomerId.HasValue)
        //        {
        //            query = query.Where(p => p.CustomerId == context.CustomerId.Value);
        //        }

        //        var payments = await query.ToListAsync();

        //        var totalPayments = payments.Sum(p => p.Amount);
        //        var totalPaymentCount = payments.Count;
        //        var refunds = payments.Where(p => p.IsRefund).Sum(p => p.Amount);
        //        var refundCount = payments.Count(p => p.IsRefund);

        //        // Payment method breakdown
        //        var methodBreakdown = payments
        //            .GroupBy(p => p.PaymentMethod)
        //            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        //        // Status breakdown
        //        var statusBreakdown = await _paymentRepository.GetStatusBreakdownAsync(context.CompanyId);

        //        // Monthly trend (last 12 months)
        //        var monthlyTrend = new List<MonthlyPaymentTrendDto>();
        //        var startDate = DateTime.UtcNow.AddMonths(-11);
        //        for (var date = startDate; date <= DateTime.UtcNow; date = date.AddMonths(1))
        //        {
        //            var monthPayments = payments.Where(p =>
        //                p.PaymentDate.Year == date.Year && p.PaymentDate.Month == date.Month);

        //            monthlyTrend.Add(new MonthlyPaymentTrendDto
        //            {
        //                Month = date.ToString("MMM"),
        //                Year = date.Year,
        //                Amount = monthPayments.Sum(p => p.Amount),
        //                Count = monthPayments.Count()
        //            });
        //        }

        //        var stats = new PaymentStatsDto
        //        {
        //            TotalPayments = totalPayments,
        //            TotalPaymentCount = totalPaymentCount,
        //            TotalRefunds = refunds,
        //            TotalRefundCount = refundCount,
        //            NetCollected = totalPayments - refunds,
        //            PaymentMethodBreakdown = methodBreakdown,
        //            StatusBreakdown = statusBreakdown,
        //            MonthlyTrend = monthlyTrend
        //        };

        //        return OperationResult<PaymentStatsDto>.SuccessResult(stats);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting payment stats");
        //        return OperationResult<PaymentStatsDto>.FailureResult("Failed to retrieve payment statistics");
        //    }
        //}

        //public async Task<OperationResult<decimal>> GetCustomerOutstandingBalanceAsync(
        //    int customerId, OperationContext context)
        //{
        //    try
        //    {
        //        var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId);
        //        var totalInvoiced = invoices.Where(i => !i.IsDeleted).Sum(i => i.TotalAmount);
        //        var totalPaid = await _paymentRepository.GetTotalPaymentsForCustomerAsync(customerId);
        //        var outstandingBalance = totalInvoiced - totalPaid;

        //        return OperationResult<decimal>.SuccessResult(outstandingBalance);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting outstanding balance for customer {CustomerId}", customerId);
        //        return OperationResult<decimal>.FailureResult("Failed to retrieve outstanding balance");
        //    }
        //}

        //#region Private Methods

        //private async Task UpdateInvoicePaymentStatus(int invoiceId)
        //{
        //    var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        //    if (invoice == null) return;

        //    var totalPaid = await _paymentRepository.GetTotalPaymentsForInvoiceAsync(invoiceId);
        //    var refunds = await _unitOfWork.Payments.Query()
        //        .Where(p => p.InvoiceId == invoiceId && p.IsRefund && !p.IsDeleted)
        //        .SumAsync(p => p.Amount);

        //    var netPaid = totalPaid - refunds;

        //    if (netPaid >= invoice.TotalAmount - 0.01m)
        //    {
        //        invoice.PaymentStatus = PaymentStatus.Paid;
        //    }
        //    else if (netPaid > 0)
        //    {
        //        invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
        //    }
        //    else
        //    {
        //        invoice.PaymentStatus = PaymentStatus.Pending;
        //    }

        //    invoice.AmountPaid = netPaid;
        //    invoice.AmountDue = invoice.TotalAmount - netPaid;

        //    _invoiceRepository.Update(invoice);
        //    await _unitOfWork.SaveChangesAsync();
        //}

        //#endregion
    }
}