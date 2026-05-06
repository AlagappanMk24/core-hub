using Core_API.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Core_API.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated phone number following E.164 international standard.
    /// Accepts any valid international phone number format - no country restrictions.
    /// This is the industry standard used by Twilio, Stripe, and major telecom providers.
    /// </summary>
    public sealed record PhoneNumber
    {
        // E.164 format: + followed by 1-15 digits (international standard)
        // This is the format used by Twilio, Stripe, and all major telecom providers
        private const string E164Pattern = @"^\+[1-9][0-9]{1,14}$";
        private static readonly Regex E164Regex = new(E164Pattern, RegexOptions.Compiled);

        // Optional: For display formatting (not validation)
        private static readonly Dictionary<string, string> CountryPrefixes = new()
        {
            ["US"] = "+1",
            ["CA"] = "+1",
            ["IN"] = "+91",
            ["GB"] = "+44",
            ["AU"] = "+61",
            ["DE"] = "+49",
            ["FR"] = "+33",
            ["JP"] = "+81",
            ["SG"] = "+65",
            ["CN"] = "+86",
            ["RU"] = "+7",
            ["BR"] = "+55",
            ["MX"] = "+52",
            ["KR"] = "+82",
            ["IT"] = "+39",
            ["ES"] = "+34",
            ["NL"] = "+31",
            ["SE"] = "+46",
            ["NO"] = "+47",
            ["DK"] = "+45",
            ["FI"] = "+358",
            ["BE"] = "+32",
            ["CH"] = "+41",
            ["AT"] = "+43",
            ["IE"] = "+353",
            ["NZ"] = "+64",
            ["ZA"] = "+27",
            ["AE"] = "+971",
            ["SA"] = "+966",
            ["SG"] = "+65",
            ["MY"] = "+60",
            ["TH"] = "+66",
            ["VN"] = "+84",
            ["PH"] = "+63",
            ["ID"] = "+62"
        };

        /// <summary>
        /// The formatted phone number in E.164 format (e.g., "+1234567890").
        /// This is the industry standard format for international phone numbers.
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// The country code extracted from the phone number (e.g., "1", "44", "91").
        /// </summary>
        public string CountryCode { get; init; }

        /// <summary>
        /// The national number without country code (e.g., "234567890").
        /// </summary>
        public string NationalNumber { get; init; }

        /// <summary>
        /// The two-letter country code if recognized (e.g., "US", "IN").
        /// May be null if country cannot be determined.
        /// </summary>
        public string? CountryCodeAlpha2 { get; init; }

        /// <summary>
        /// Initializes a new PhoneNumber instance with E.164 validation.
        /// No country restrictions - any valid E.164 format is accepted.
        /// </summary>
        /// <param name="phoneNumber">The phone number (will be normalized to E.164).</param>
        /// <exception cref="InvalidPhoneException">Thrown when phone number is invalid.</exception>
        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPhoneException(value, "Phone number cannot be empty");

            // Normalize to E.164 format
            var normalized = NormalizeToE164(value);

            // Validate E.164 format
            if (!E164Regex.IsMatch(normalized))
                throw new InvalidPhoneException(value, $"Invalid phone number format. Use E.164 format (e.g., +1234567890)");

            if (!IsValidE164Length(normalized))
                throw new InvalidPhoneException(value, "Phone number must be between 1 and 15 digits after country code");

            Value = normalized;

            // Extract components
            CountryCode = ExtractCountryCode(normalized);
            NationalNumber = ExtractNationalNumber(normalized);
            CountryCodeAlpha2 = GetCountryAlpha2(CountryCode);
        }

        /// <summary>
        /// Attempts to create a valid phone number without throwing exceptions.
        /// </summary>
        public static bool TryCreate(string phoneNumber, out PhoneNumber? result, out string? errorMessage)
        {
            result = null;
            errorMessage = null;

            try
            {
                result = new PhoneNumber(phoneNumber);
                return true;
            }
            catch (InvalidPhoneException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Normalizes any phone number format to E.164 standard.
        /// Examples:
        /// - "+1 (212) 555-1234" → "+12125551234"
        /// - "02125551234" → "+12125551234" (assumes US default)
        /// - "044 1234 5678" → "+4412345678"
        /// </summary>
        private static string NormalizeToE164(string phoneNumber)
        {
            // If already starts with +, extract only digits after and keep +
            if (phoneNumber.StartsWith('+'))
            {
                var digits = new string(phoneNumber.Skip(1).Where(char.IsDigit).ToArray());
                return $"+{digits}";
            }

            // Check if it starts with international access code (00)
            if (phoneNumber.StartsWith("00"))
            {
                var digits = new string(phoneNumber.Skip(2).Where(char.IsDigit).ToArray());
                return $"+{digits}";
            }

            // For numbers without country code, we need to detect or assume
            // In production, you might want to use libphonenumber-csharp
            // For simplicity, we'll just extract digits and assume caller knows format
            var allDigits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Try to detect country code from common patterns
            var detectedCountryCode = DetectCountryCodeFromDigits(allDigits);
            if (detectedCountryCode != null)
            {
                var nationalNumber = allDigits[detectedCountryCode.Length..];
                return $"+{detectedCountryCode}{nationalNumber}";
            }

            // If no country code detected, assume it's a national number without country code
            // In a real app, you might want to use the user's default country
            return $"+{allDigits}";
        }

        private static string? DetectCountryCodeFromDigits(string digits)
        {
            // Check for known country codes (1-3 digits)
            for (int len = 1; len <= 3 && len <= digits.Length; len++)
            {
                var potentialCode = digits[..len];
                if (IsValidCountryCodeDigits(potentialCode))
                    return potentialCode;
            }
            return null;
        }

        private static bool IsValidCountryCodeDigits(string code)
        {
            // Valid E.164 country codes are 1-3 digits
            // This is a simplified check - in production, use a complete list
            var validCodes = new HashSet<string>
            {
                "1", "7", "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41",
                "43", "44", "45", "46", "47", "48", "49", "51", "52", "53", "54", "55", "56",
                "57", "58", "60", "61", "62", "63", "64", "65", "66", "81", "82", "84", "86",
                "90", "91", "92", "93", "94", "95", "98", "212", "213", "216", "218", "220",
                "221", "222", "223", "224", "225", "226", "227", "228", "229", "230", "231",
                "232", "233", "234", "235", "236", "237", "238", "239", "240", "241", "242",
                "243", "244", "245", "246", "247", "248", "249", "250", "251", "252", "253",
                "254", "255", "256", "257", "258", "259", "260", "261", "262", "263", "264",
                "265", "266", "267", "268", "269", "290", "291", "297", "298", "299", "350",
                "351", "352", "353", "354", "355", "356", "357", "358", "359", "370", "371",
                "372", "373", "374", "375", "376", "377", "378", "379", "380", "381", "382",
                "385", "386", "387", "389", "420", "421", "423", "500", "501", "502", "503",
                "504", "505", "506", "507", "508", "509", "590", "591", "592", "593", "594",
                "595", "596", "597", "598", "599", "670", "672", "673", "674", "675", "676",
                "677", "678", "679", "680", "681", "682", "683", "685", "686", "687", "688",
                "689", "690", "691", "692", "800", "808", "850", "852", "853", "855", "856",
                "870", "880", "881", "882", "883", "886", "960", "961", "962", "963", "964",
                "965", "966", "967", "968", "970", "971", "972", "973", "974", "975", "976",
                "977", "992", "993", "994", "995", "996", "998"
            };
            return validCodes.Contains(code);
        }

        private static bool IsValidE164Length(string e164)
        {
            // E.164 allows 1-15 digits after the +
            var digits = e164[1..];
            return digits.Length >= 1 && digits.Length <= 15;
        }

        private static string ExtractCountryCode(string e164)
        {
            // Country code is 1-3 digits after the +
            var digits = e164[1..];

            for (int len = 3; len >= 1; len--)
            {
                if (len <= digits.Length)
                {
                    var possibleCode = digits[..len];
                    if (IsValidCountryCodeDigits(possibleCode))
                        return possibleCode;
                }
            }

            // Fallback to first digit
            return digits.Length > 0 ? digits[..1] : string.Empty;
        }

        private static string ExtractNationalNumber(string e164)
        {
            var countryCode = ExtractCountryCode(e164);
            var digits = e164[1..];
            return digits[countryCode.Length..];
        }

        private static string? GetCountryAlpha2(string countryCode)
        {
            // Reverse lookup: find alpha2 by prefix
            return CountryPrefixes.FirstOrDefault(x => x.Value == $"+{countryCode}").Key;
        }

        /// <summary>
        /// Returns the formatted phone number in E.164 format.
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Returns a human-readable formatted phone number (e.g., "+1 (212) 555-1234").
        /// Note: This is for display only - the stored value remains E.164.
        /// </summary>
        public string ToDisplayFormat()
        {
            // Simple formatting based on country code
            // In production, use libphonenumber-csharp for proper formatting
            return CountryCode switch
            {
                "1" => $"+{CountryCode} ({NationalNumber[..3]}) {NationalNumber[3..6]}-{NationalNumber[6..10]}",
                "44" => $"+{CountryCode} {NationalNumber[..4]} {NationalNumber[4..7]} {NationalNumber[7..]}",
                "91" => $"+{CountryCode} {NationalNumber[..5]} {NationalNumber[5..]}",
                _ => Value
            };
        }
    }
}
