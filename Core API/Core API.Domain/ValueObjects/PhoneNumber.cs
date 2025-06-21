using Core_API.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core_API.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated phone number with country-specific formatting and rules.
    /// Supports international formats and validates against country-specific patterns.
    /// </summary>
    public sealed record PhoneNumber
    {
        // Country-specific phone number configurations
        private static readonly Dictionary<string, CountryPhoneConfig> CountryConfigs = new()
        {
            ["US"] = new("+1", @"^\+1[2-9]\d{2}[2-9](?!11)\d{6}$", 10, "US phone number must be in format: +12125551212"),
            ["CA"] = new("+1", @"^\+1[2-9]\d{2}[2-9](?!11)\d{6}$", 10, "Canadian phone number must be in format: +12125551212"),
            ["IN"] = new("+91", @"^\+91[6-9]\d{9}$", 10, "Indian phone number must be in format: +919876543210"),
            ["AU"] = new("+61", @"^\+614\d{8}$", 9, "Australian phone number must be in format: +61412345678"),
            ["UK"] = new("+44", @"^\+447\d{9}$", 10, "UK phone number must be in format: +447912345678"),
            ["RU"] = new("+7", @"^\+7[3489]\d{9}$", 10, "Russian phone number must be in format: +79123456789"),
            ["CN"] = new("+86", @"^\+86(1[3-9]\d{9}|[2-9]\d{1,2}\d{7,8})$", 0, "Chinese phone number must be in format: +8613812345678 or +862112345678")
        };

        /// <summary>
        /// Configuration for country-specific phone number validation.
        /// </summary>
        private record CountryPhoneConfig(string Prefix, string Pattern, int NationalLength, string ErrorMessage)
        {
            public Regex Regex { get; } = new(Pattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// The fully formatted phone number with country code (e.g., "+12125551212").
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// The two-letter country code (e.g., "US", "IN").
        /// </summary>
        public string CountryCode { get; init; }

        /// <summary>
        /// The phone number without country prefix (e.g., "2125551212").
        /// </summary>
        public string NationalNumber { get; init; }

        /// <summary>
        /// Initializes a new PhoneNumber instance with validation.
        /// </summary>
        /// <param name="phoneNumber">The raw phone number input.</param>
        /// <param name="countryCode">Optional country code (defaults to "US" if not provided).</param>
        /// <exception cref="InvalidPhoneException">Thrown if the phone number is invalid or unsupported.</exception>
        public PhoneNumber(string phoneNumber, string? countryCode = null)
        {
            if (!TryCreate(phoneNumber, countryCode ?? "US", out var result, out var errorMessage))
                throw new InvalidPhoneException(errorMessage);

            Value = result.Value;
            CountryCode = result.CountryCode;
            NationalNumber = result.NationalNumber;
        }

        /// <summary>
        /// Attempts to create a valid phone number without throwing exceptions.
        /// </summary>
        /// <param name="phoneNumber">The raw phone number input.</param>
        /// <param name="countryCode">The country code (e.g., "US", "IN").</param>
        /// <param name="result">The resulting PhoneNumber if valid, or null if invalid.</param>
        /// <param name="errorMessage">Error message if validation fails.</param>
        /// <returns>True if the phone number is valid, false otherwise.</returns>
        public static bool TryCreate(string phoneNumber, string countryCode, out PhoneNumber? result, out string? errorMessage)
        {
            result = null;
            errorMessage = null;

            // Validate non-empty input
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                errorMessage = "Phone number cannot be empty";
                return false;
            }

            // Detect country code from prefix if phone number starts with "+"
            countryCode = (phoneNumber.StartsWith("+") ? DetectCountry(phoneNumber) : countryCode).ToUpperInvariant();

            // Retrieve country configuration
            if (!CountryConfigs.TryGetValue(countryCode, out var config))
            {
                errorMessage = "Unsupported country code";
                return false;
            }

            // Format and validate the phone number
            var formattedNumber = FormatNumber(phoneNumber, countryCode, config);
            if (!config.Regex.IsMatch(formattedNumber))
            {
                errorMessage = config.ErrorMessage;
                return false;
            }

            // Extract national number and create result
            var nationalNumber = formattedNumber[config.Prefix.Length..];
            result = new PhoneNumber
            {
                Value = formattedNumber,
                CountryCode = countryCode,
                NationalNumber = nationalNumber
            };
            return true;
        }

        /// <summary>
        /// Detects the country code based on the phone number's international prefix.
        /// </summary>
        /// <param name="phoneNumber">Phone number with international prefix.</param>
        /// <returns>Detected country code or "US" as default.</returns>
        private static string DetectCountry(string phoneNumber)
        {
            foreach (var (code, config) in CountryConfigs)
            {
                if (phoneNumber.StartsWith(config.Prefix))
                    return code;
            }
            return "US";
        }

        /// <summary>
        /// Formats the phone number according to country-specific rules.
        /// </summary>
        /// <param name="phoneNumber">Raw phone number input.</param>
        /// <param name="countryCode">Two-letter country code.</param>
        /// <param name="config">Country-specific configuration.</param>
        /// <returns>Formatted phone number with country prefix.</returns>
        private static string FormatNumber(string phoneNumber, string countryCode, CountryPhoneConfig config)
        {
            // Return as-is if already correctly prefixed
            if (phoneNumber.StartsWith(config.Prefix))
                return phoneNumber;

            // Handle variable-length numbers (e.g., China)
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (config.NationalLength == 0) // Special case for variable-length (e.g., China)
                return digits.StartsWith(config.Prefix[1..]) ? $"+{digits}" : $"{config.Prefix}{digits}";

            // Format based on expected length
            var expectedLength = config.NationalLength;
            var formatted = digits.Length switch
            {
                var len when len == expectedLength => $"{config.Prefix}{digits}",
                var len when len > expectedLength && digits.StartsWith(config.Prefix[1..]) => $"+{digits}",
                _ => $"{config.Prefix}{digits[^expectedLength..]}"
            };
            return formatted;
        }

        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private PhoneNumber() { }

        /// <summary>
        /// Returns the formatted phone number as a string.
        /// </summary>
        public override string ToString() => Value;
    }
}
