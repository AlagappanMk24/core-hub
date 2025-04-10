using Core_API.Domain.Models.Entities;
using Core_API.Infrastructure.Data.Context;
using Core_API.Web.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Core_API.Web.Utilities;
using Newtonsoft.Json;
using Core_API.Application.DI;
using Core_API.Infrastructure.DI;
using Core_API.Infrastructure.Data.Seeders;
using Microsoft.AspNetCore.Authentication.Google;
using Core_API.Web.Logger;
using AspNet.Security.OAuth.GitHub;
using Core_API.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Seed Database

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await SeedDatabase.SeedDatabaseAsync(services);
}

// Configure middleware pipeline
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    ConfigureDatabase(services, configuration);

    ConfigureIdentity(services);

    ConfigureJwtSettings(services, configuration);

    ConfigureEmailSettings(services, configuration);

    GenerateSecretKey(configuration);

    ConfigureAuthentication(services, configuration);

    ConfigureCORS(services);

    ConfigureSwagger(services);

    ConfigureExternalLogins(builder.Services, builder.Configuration);

    services.AddControllers(options =>
    {
        // Custom filter for model validation
        options.Filters.Add<ValidateModelFilter>();
    });

    services.AddControllersWithViews();

    services.AddHttpClient();

    services.AddHttpContextAccessor();

    RegisterServices(services);
}

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("CoreAPIDbConnection");

    // Add DbContext with SQL Server
    services.AddDbContext<CoreAPIDbContext>(options =>
        options.UseSqlServer(connectionString));
}
void ConfigureIdentity(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreAPIDbContext>()
            .AddDefaultTokenProviders();
}

void ConfigureJwtSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
}

/// <summary>
/// Configures Email settings from app configuration.
/// </summary>
void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
}
void GenerateSecretKey(IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

    // Check if SecretKey is null or empty and generate a new one if necessary
    if (string.IsNullOrEmpty(jwtSettings.SecretKey))
    {
        var secretKey = SecretKeyGenerator.GenerateSecretKey();
        jwtSettings ??= new JwtSettings();
        jwtSettings.SecretKey = secretKey;

        // Update appsettings.json with the new secret key
        var appSettingsFile = "appsettings.json";
        var json = File.ReadAllText(appSettingsFile);
        dynamic jsonObj = JsonConvert.DeserializeObject(json);

        jsonObj["JwtSettings"]["SecretKey"] = secretKey;
        string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        File.WriteAllText(appSettingsFile, output);
    }
}

void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

    services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(opt =>
    {
        opt.SaveToken = true;
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtSettings.ValidAudience,
            ValidIssuer = jwtSettings.ValidIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
}

/// <summary>
/// Configures external login providers, such as Facebook, Google, Microsoft.
/// </summary>
void ConfigureExternalLogins(IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication()
    //.AddFacebook(fbOptions =>
    //{
    //    fbOptions.AppId = configuration.GetSection("FacebookKeys:AppId").Value;
    //    fbOptions.AppSecret = configuration.GetSection("FacebookKeys:AppSecret").Value;
    //})
    .AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
    {
        googleOptions.ClientId = configuration.GetSection("GoogleKeys:ClientId").Value;
        googleOptions.ClientSecret = configuration.GetSection("GoogleKeys:ClientSecret").Value;
    })
    .AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, githubOptions =>
     {
         githubOptions.ClientId = configuration.GetSection("GitHubKeys:ClientId").Value;
         githubOptions.ClientSecret = configuration.GetSection("GitHubKeys:ClientSecret").Value;
         githubOptions.CallbackPath = new PathString("/signin-github"); // Must match GitHub settings
     })
    .AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = configuration.GetSection("MicrosoftKeys:ClientId").Value;
        microsoftOptions.ClientSecret = configuration.GetSection("MicrosoftKeys:ClientSecret").Value;
    });
}
void ConfigureCORS(IServiceCollection services)
{
    // Add CORS policy to allow requests from the specified origin
    services.AddCors(options =>
    {
        options.AddPolicy("AllowMyOrigin",
            builder => builder.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials());
    });
}
void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization Example : 'Bearer eyeleuieeesfjfjdue",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
             {
                 new OpenApiSecurityScheme
                 {
                      Reference= new OpenApiReference
                      {
                          Type=ReferenceType.SecurityScheme,
                          Id="Bearer",

                      },
                      Name="Bearer",
                      In=ParameterLocation.Header
                 },
                 new List<string>()
             }

        });
    });
}
void RegisterServices(IServiceCollection services)
{
    services.AddApplicationDependencies()
            .AddInfrastructureDependencies();
}
void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowMyOrigin");
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Configure Logging
    ConfigureCustomLogging(app);
}


/// <summary>
/// Configures custom file logging using a dynamic log file path.
/// </summary>
void ConfigureCustomLogging(WebApplication app)
{
    string formattedDate = DateTime.Now.ToString("MM-dd-yyyy");
    string baseLogPath = builder.Configuration.GetValue<string>("Logging:LogFilePath");
    string logFilePath = Path.Combine(baseLogPath, $"log-{formattedDate}.txt");

    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    loggerFactory.AddProvider(new CustomFileLoggerProvider(logFilePath, httpContextAccessor));
}