using Core_API.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Core_API.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated postal address.
    /// Industry standard: Accepts ALL countries, validates basic format requirements.
    /// No country restrictions - businesses operate globally.
    /// </summary>
    public sealed record Address
    {
        // Common country patterns for validation(extensible)
        // These are for basic validation only - not restrictive
        private static readonly Dictionary<string, CountryPattern> CountryPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            ["US"] = new CountryPattern { ZipCodePattern = @"^\d{5}(-\d{4})?$", StatePattern = @"^[A-Z]{2}$", StateRequired = true },
            ["CA"] = new CountryPattern { ZipCodePattern = @"^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$", StatePattern = @"^[A-Z]{2}$", StateRequired = true },
            ["IN"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = true },
            ["GB"] = new CountryPattern { ZipCodePattern = @"^[A-Za-z]{1,2}\d{1,2}[A-Za-z]? ?\d[A-Za-z]{2}$", StateRequired = false },
            ["AU"] = new CountryPattern { ZipCodePattern = @"^\d{4}$", StatePattern = @"^[A-Z]{2,3}$", StateRequired = true },
            ["DE"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = false },
            ["FR"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = false },
            ["JP"] = new CountryPattern { ZipCodePattern = @"^\d{3}-\d{4}$", StateRequired = true },
            ["SG"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = false },
            ["CN"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = true },
            ["BR"] = new CountryPattern { ZipCodePattern = @"^\d{5}-\d{3}$", StateRequired = true },
            ["MX"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["ZA"] = new CountryPattern { ZipCodePattern = @"^\d{4}$", StateRequired = false },
            ["NZ"] = new CountryPattern { ZipCodePattern = @"^\d{4}$", StateRequired = false },
            ["MY"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["TH"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["VN"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = true },
            ["PH"] = new CountryPattern { ZipCodePattern = @"^\d{4}$", StateRequired = true },
            ["ID"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["RU"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = true },
            ["TR"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["EG"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["NG"] = new CountryPattern { ZipCodePattern = @"^\d{6}$", StateRequired = true },
            ["PK"] = new CountryPattern { ZipCodePattern = @"^\d{5}$", StateRequired = true },
            ["BD"] = new CountryPattern { ZipCodePattern = @"^\d{4}$", StateRequired = true },
        };
        private class CountryPattern
        {
            public string? ZipCodePattern { get; set; }
            public string? StatePattern { get; set; }
            public bool StateRequired { get; set; } = false;
        }

        /// <summary>
        /// The primary address line (e.g., street number and name).
        /// </summary>
        public string AddressLine1 { get; init; }

        /// <summary>
        /// Optional secondary address line (e.g., apartment, suite, unit).
        /// </summary>
        public string? AddressLine2 { get; init; }

        /// <summary>
        /// The city or locality.
        /// </summary>
        public string City { get; init; }

        /// <summary>
        /// The state, province, or region.
        /// </summary>
        public string? State { get; init; }

        /// <summary>
        /// The postal or ZIP code.
        /// </summary>
        public string ZipCode { get; init; }

        /// <summary>
        /// The two-letter country code (e.g., "US", "IN").
        /// </summary>
        public string CountryCode { get; init; }

        /// <summary>
        /// The full country name (e.g., "United States").
        /// </summary>
        public string CountryName { get; init; }

        /// <summary>
        /// Initializes a new Address instance with validation.
        /// </summary>
        /// <param name="addressLine1">Primary address line.</param>
        /// <param name="city">City or locality.</param>
        /// <param name="zipCode">Postal or ZIP code.</param>
        /// <param name="countryCode">Two-letter country code.</param>
        /// <param name="addressLine2">Optional secondary address line.</param>
        /// <param name="state">Optional state or region.</param>
        /// <exception cref="InvalidAddressException">Thrown when address validation fails.</exception>
        public Address(
            string addressLine1,
            string city,
            string zipCode,
            string countryCode,
            string? addressLine2 = null,
            string? state = null)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(addressLine1))
                throw new InvalidAddressException("Address line 1 is required");

            if (string.IsNullOrWhiteSpace(city))
                throw new InvalidAddressException("City is required");

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new InvalidAddressException("Zip/Postal code is required");

            if (string.IsNullOrWhiteSpace(countryCode))
                throw new InvalidAddressException("Country code is required");

            // Normalize inputs
            countryCode = countryCode.ToUpperInvariant();

            // Get country name (use friendly name if available, otherwise use code)
            var countryName = GetCountryName(countryCode) ?? countryCode;

            // Validate zip/postal code format if pattern exists for this country
            if (CountryPatterns.TryGetValue(countryCode, out var pattern) &&
                !string.IsNullOrWhiteSpace(pattern.ZipCodePattern))
            {
                var zipRegex = new Regex(pattern.ZipCodePattern, RegexOptions.Compiled);
                if (!zipRegex.IsMatch(zipCode))
                    throw new InvalidAddressException($"Invalid zip/postal code format for {countryName}. Expected format: {pattern.ZipCodePattern}");
            }
            else
            {
                // For unknown countries, only basic validation
                if (!Regex.IsMatch(zipCode, @"^[A-Za-z0-9\s\-]{3,10}$"))
                    throw new InvalidAddressException($"Invalid zip/postal code format for {countryName}");
            }

            // Validate state if required for this country
            if (pattern != null && pattern.StateRequired && string.IsNullOrWhiteSpace(state))
                throw new InvalidAddressException($"State/Province is required for {countryName}");

            // Validate state format if pattern exists
            if (pattern != null && !string.IsNullOrWhiteSpace(state) && !string.IsNullOrWhiteSpace(pattern.StatePattern))
            {
                var stateRegex = new Regex(pattern.StatePattern, RegexOptions.Compiled);
                if (!stateRegex.IsMatch(state.Trim()))
                    throw new InvalidAddressException($"Invalid state format for {countryName}");
            }

            // Set properties
            AddressLine1 = addressLine1.Trim();
            AddressLine2 = addressLine2?.Trim();
            City = city.Trim();
            State = state?.Trim();
            ZipCode = zipCode.Trim();
            CountryCode = countryCode;
            CountryName = countryName;
        }

        /// <summary>
        /// Gets the full country name from country code.
        /// Industry standard: Uses comprehensive country list.
        /// </summary>
        private static string GetCountryName(string countryCode)
        {
            return countryCode.ToUpperInvariant() switch
            {
                "US" => "United States",
                "CA" => "Canada",
                "IN" => "India",
                "GB" or "UK" => "United Kingdom",
                "AU" => "Australia",
                "DE" => "Germany",
                "FR" => "France",
                "JP" => "Japan",
                "SG" => "Singapore",
                "CN" => "China",
                "BR" => "Brazil",
                "MX" => "Mexico",
                "ZA" => "South Africa",
                "NZ" => "New Zealand",
                "MY" => "Malaysia",
                "TH" => "Thailand",
                "VN" => "Vietnam",
                "PH" => "Philippines",
                "ID" => "Indonesia",
                "RU" => "Russia",
                "TR" => "Turkey",
                "EG" => "Egypt",
                "NG" => "Nigeria",
                "PK" => "Pakistan",
                "BD" => "Bangladesh",
                "KR" => "South Korea",
                "IT" => "Italy",
                "ES" => "Spain",
                "NL" => "Netherlands",
                "SE" => "Sweden",
                "NO" => "Norway",
                "DK" => "Denmark",
                "FI" => "Finland",
                "BE" => "Belgium",
                "CH" => "Switzerland",
                "AT" => "Austria",
                "IE" => "Ireland",
                "PL" => "Poland",
                "PT" => "Portugal",
                "GR" => "Greece",
                "CZ" => "Czech Republic",
                "HU" => "Hungary",
                "RO" => "Romania",
                "UA" => "Ukraine",
                "SA" => "Saudi Arabia",
                "AE" => "United Arab Emirates",
                "IL" => "Israel",
                _ => countryCode // Return code if name not found
            };
        }

        /// <summary>
        /// Attempts to create a valid address without throwing exceptions.
        /// </summary>
        /// <param name="addressLine1">Primary address line.</param>
        /// <param name="city">City or locality.</param>
        /// <param name="zipCode">Postal or ZIP code.</param>
        /// <param name="countryCode">Two-letter country code.</param>
        /// <param name="addressLine2">Optional secondary address line.</param>
        /// <param name="state">Optional state or region.</param>
        /// <param name="result">The resulting Address if valid, or null if invalid.</param>
        /// <param name="errorMessage">Error message if validation fails.</param>
        /// <returns>True if the address is valid, false otherwise.</returns>
        /// Attempts to create a valid address without throwing exceptions.
        public static bool TryCreate(
           string addressLine1,
           string city,
           string zipCode,
           string countryCode,
           out Address? result,
           out string? errorMessage,
           string? addressLine2 = null,
           string? state = null)
        {
            result = null;
            errorMessage = null;

            try
            {
                result = new Address(addressLine1, city, zipCode, countryCode, addressLine2, state);
                return true;
            }
            catch (InvalidAddressException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Returns the formatted address as a multi-line string.
        /// </summary>
        public override string ToString()
        {
            var lines = new List<string>();
            lines.Add(AddressLine1);
            if (!string.IsNullOrWhiteSpace(AddressLine2))
                lines.Add(AddressLine2);
            lines.Add(City);
            if (!string.IsNullOrWhiteSpace(State))
                lines.Add(State);
            lines.Add(ZipCode);
            lines.Add(CountryName);

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Returns the address as a single-line string.
        /// </summary>
        public string ToSingleLineString()
        {
            var parts = new[] { AddressLine1, AddressLine2, City, State, ZipCode, CountryName }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }

        /// <summary>
        /// Returns the address as HTML formatted string.
        /// </summary>
        public string ToHtmlString()
        {
            var lines = new List<string>();
            lines.Add(AddressLine1);
            if (!string.IsNullOrWhiteSpace(AddressLine2))
                lines.Add(AddressLine2);
            lines.Add(City);
            if (!string.IsNullOrWhiteSpace(State))
                lines.Add(State);
            lines.Add(ZipCode);
            lines.Add(CountryName);

            return string.Join("<br/>", lines);
        }

        /// <summary>
        /// Gets all supported country codes (extensible).
        /// </summary>
        public static IReadOnlyList<string> GetSupportedCountryCodes()
        {
            // Return a comprehensive list of common countries
            return new List<string>
            {
                "US", "CA", "GB", "AU", "DE", "FR", "JP", "SG", "IN", "CN",
                "BR", "MX", "ZA", "NZ", "MY", "TH", "VN", "PH", "ID", "RU",
                "TR", "EG", "NG", "PK", "BD", "KR", "IT", "ES", "NL", "SE",
                "NO", "DK", "FI", "BE", "CH", "AT", "IE", "PL", "PT", "GR",
                "CZ", "HU", "RO", "UA", "SA", "AE", "IL"
            }.AsReadOnly();
        }

        /// <summary>
        /// Checks if a zip code format is valid for a country.
        /// </summary>
        public static bool IsValidZipCode(string zipCode, string countryCode)
        {
            if (!CountryPatterns.TryGetValue(countryCode.ToUpperInvariant(), out var pattern))
                return true; // No pattern defined, accept any format

            if (string.IsNullOrWhiteSpace(pattern.ZipCodePattern))
                return true;

            var zipRegex = new Regex(pattern.ZipCodePattern, RegexOptions.Compiled);
            return zipRegex.IsMatch(zipCode);
        }
    }
}