using Core_API.Infrastructure.Configuration.Settings;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Core_API.Web.Helpers
{
    /// <summary>
    /// Provides cryptographic key generation and management functionality for JWT tokens.
    /// </summary>
    /// <remarks>
    /// This utility class handles the secure generation of JWT secret keys and ensures
    /// they persist in the application configuration. The default key length is 32 bytes
    /// (256 bits) which provides adequate security for HMAC-SHA256 signing.
    /// </remarks>
    public static class SecretKeyGenerator
    {
        private const int DefaultKeyLength = 32;
        private const string AppSettingsFileName = "appsettings.json";

        /// <summary>
        /// Generates a cryptographically secure random secret key for JWT signing.
        /// </summary>
        /// <param name="length">The length of the key in bytes. Default is 32 bytes (256 bits).</param>
        /// <returns>A Base64-encoded string representation of the generated key.</returns>
        /// <remarks>
        /// The method uses <see cref="RandomNumberGenerator"/> for cryptographic randomness,
        /// which is suitable for security-sensitive operations like key generation.
        /// </remarks>
        /// <example>
        /// <code>
        /// var secretKey = SecretKeyGenerator.GenerateSecretKey();
        /// var customKey = SecretKeyGenerator.GenerateSecretKey(64);
        /// </code>
        /// </example>
        public static string GenerateSecretKey(int length = DefaultKeyLength)
        {
            var byteArray = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(byteArray);
            return Convert.ToBase64String(byteArray);
        }

        /// <summary>
        /// Ensures a valid JWT secret key exists in the configuration, generating one if missing.
        /// </summary>
        /// <param name="configuration">The application configuration interface.</param>
        /// <param name="jwtSettings">The current JWT settings object to validate.</param>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        /// <item><description>Checks if a secret key already exists in the provided settings</description></item>
        /// <item><description>Generates a new cryptographically secure key if none exists</description></item>
        /// <item><description>Persists the new key to appsettings.json for future runs</description></item>
        /// <item><description>Updates the in-memory configuration with the new key</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="IOException">Thrown when appsettings.json cannot be read or written.</exception>
        /// <exception cref="JsonException">Thrown when appsettings.json contains invalid JSON.</exception>
        public static void EnsureSecretKeyExists(IConfiguration configuration, JwtSettings jwtSettings)
        {
            // Return early if a valid secret key already exists
            if (!string.IsNullOrEmpty(jwtSettings?.SecretKey))
                return;

            // Generate a new secret key
            var secretKey = GenerateSecretKey();

            #region Update Configuration File

            /// <summary>
            /// Persists the generated secret key to appsettings.json.
            /// </summary>
            /// <remarks>
            /// This operation reads the existing configuration, updates the JwtSettings section,
            /// and writes it back to disk with proper formatting.
            /// </remarks>
            var json = File.ReadAllText(AppSettingsFileName);
            dynamic jsonObj = JsonConvert.DeserializeObject(json) ?? throw new InvalidOperationException("Failed to parse appsettings.json");

            jsonObj["JwtSettings"]["SecretKey"] = secretKey;
            var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(AppSettingsFileName, output);

            #endregion

            #region Update In-Memory Configuration

            /// <summary>
            /// Updates the in-memory configuration with the new secret key.
            /// </summary>
            /// <remarks>
            /// This ensures the current application instance can immediately use the new key
            /// without requiring a restart.
            /// </remarks>
            configuration.GetSection("JwtSettings")["SecretKey"] = secretKey;

            #endregion
        }
    }
}
