using Core_API.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Core_API.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated email address with a restricted set of allowed domains.
    /// Ensures the email format is valid and the domain is supported.
    /// </summary>
    public sealed record Email
    {
        // Regex pattern for basic email format validation
        private const string EmailPattern = @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$";
        private static readonly Regex EmailRegex = new(EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Allowed email domains for validation
        private static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com", "yahoo.com", "outlook.com", "icloud.com",
            "company.com", "edu.in", "mail.ru", "qq.com", "163.com"
        };

        /// <summary>
        /// The fully formatted email address (e.g., "user@example.com").
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// The domain part of the email (e.g., "example.com").
        /// </summary>
        public string Domain { get; init; }

        /// <summary>
        /// Initializes a new Email instance with validation.
        /// </summary>
        /// <param name="value">The email address to validate.</param>
        /// <exception cref="InvalidEmailException">Thrown if the email is empty, has an invalid format, or uses an unsupported domain.</exception>
        public Email(string value)
        {
            // Check for empty or whitespace input
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidEmailException("Email cannot be empty");

            // Normalize email by trimming and converting to lowercase
            var formatted = value.Trim().ToLowerInvariant();

            // Validate email format using regex
            if (!EmailRegex.IsMatch(formatted))
                throw new InvalidEmailException($"Invalid email format: {value}");

            // Extract domain part after '@'
            var domainStart = formatted.IndexOf('@') + 1;
            if (domainStart <= 0 || domainStart >= formatted.Length)
                throw new InvalidEmailException($"Invalid email format: {value}");

            // Check if domain is in allowed list
            var domain = formatted[domainStart..];
            if (!AllowedDomains.Contains(domain))
                throw new InvalidEmailException($"Email domain '{domain}' is not supported");

            Value = formatted;
            Domain = domain;
        }

        /// <summary>
        /// Returns the email address as a string.
        /// </summary>
        public override string ToString() => Value;
    }
}
