#region Imports

using Core_API.Infrastructure.RealTime.Hubs;
using Core_API.Web.Extensions;

#endregion

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

/// <summary>
/// Registers all required services with the dependency injection container.
/// </summary>
/// <remarks>
/// Services are organized by concern using extension methods for better maintainability.
/// </remarks>
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityAndAuthentication(builder.Configuration);
builder.Services.AddAuthorizationAndHandlers();
builder.Services.AddExternalLogins(builder.Configuration);
builder.Services.AddApplicationAndInfrastructure();
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddWebLayer(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks();
builder.Services.AddBackgroundServices();

#endregion

var app = builder.Build();

#region HTTP Pipeline Configuration

/// <summary>
/// Configures the HTTP request pipeline with middleware components.
/// </summary>
/// <remarks>
/// Middleware order is critical and follows the recommended sequence:
/// 1. Development tools (Swagger)
/// 2. Security (HTTPS, CORS)
/// 3. Static files
/// 4. Rate limiting
/// 5. Exception handling
/// 6. Authentication/Authorization
/// 7. Endpoint mapping
/// </remarks>

// Configure Swagger only in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enforce HTTPS redirection for security
app.UseHttpsRedirection();

// Apply Cross-Origin Resource Sharing policy
app.UseCors("AllowMyOrigin");

// Enable serving of static files from wwwroot
app.UseStaticFiles();

// Apply rate limiting to protect against abuse
app.UseRateLimiter();

// Global exception handling middleware
app.UseGlobalExceptionHandling();

// Enable authentication middleware
app.UseAuthentication();

// Enable authorization middleware
app.UseAuthorization();

// Map API controller endpoints
app.MapControllers();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

#endregion

#region Post-Build Operations

/// <summary>
/// Executes post-build operations including database seeding and logging configuration.
/// </summary>
/// <remarks>
/// These operations run after the pipeline is configured but before the application starts.
/// </remarks>

// Run database seeding asynchronously
await app.SeedDatabaseAsync();

// Configure custom file-based logging
app.ConfigureCustomFileLogging();

#endregion

// Start the application
app.Run();