using Microsoft.OpenApi.Models;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Swagger/OpenAPI documentation.
    /// </summary>
    /// <remarks>
    /// Swagger documentation enables API discovery, testing, and client code generation.
    /// This configuration adds JWT bearer authentication support to the Swagger UI.
    /// </remarks>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configures Swagger/OpenAPI documentation with JWT bearer authentication support.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// The configuration includes:
        /// <list type="bullet">
        /// <item><description>Security definition for Bearer token authentication</description></item>
        /// <item><description>Security requirement requiring authentication for all endpoints</description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                #region Security Definition

                /// <summary>
                /// Defines the Bearer token authentication scheme for Swagger.
                /// </summary>
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                              \r\n\r\n Enter 'Bearer' [space] and then your token.
                              \r\n\r\n Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                #endregion

                #region Security Requirement

                /// <summary>
                /// Applies the security requirement globally to all endpoints.
                /// </summary>
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
                         new List<string>() // Empty list means no specific scopes required
                     }
                });
                #endregion
            });
            return services;
        }
    }
}