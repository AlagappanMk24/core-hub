using Core_API.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Core_API.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated email address following RFC 5322 standards.
    /// Validates format only - no domain restrictions for business communication.
    /// </summary>
    public sealed record Email
    {
        /// <summary>
        /// RFC 5322 compliant email regex pattern.
        /// </summary>
        private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        /// <summary>
        /// Compiled regex for email validation.
        /// </summary>
        private static readonly Regex EmailRegex = new(EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The fully formatted email address (e.g., "user@example.com").
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// The domain part of the email (e.g., "example.com").
        /// </summary>
        public string Domain { get; init; }

        /// <summary>
        /// The local part of the email (e.g., "user").
        /// </summary>
        public string LocalPart { get; init; }

        /// <summary>
        /// Initializes a new Email instance with format validation only.
        /// No domain restrictions - any valid email format is accepted.
        /// </summary>
        /// <param name="value">The email address to validate.</param>
        /// <exception cref="InvalidEmailException">Thrown if the email is empty or has an invalid format.</exception>
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

            // Extract domain and local parts
            var atIndex = formatted.IndexOf('@');
            if (atIndex <= 0 || atIndex >= formatted.Length - 1)
                throw new InvalidEmailException($"Invalid email format: {value}");

            LocalPart = formatted[..atIndex];
            Domain = formatted[(atIndex + 1)..];

            // Additional validation: local part shouldn't be empty
            if (string.IsNullOrWhiteSpace(LocalPart))
                throw new InvalidEmailException("Email local part cannot be empty");

            // Additional validation: domain must have at least one dot
            if (!Domain.Contains('.'))
                throw new InvalidEmailException($"Email domain '{Domain}' must contain at least one dot");

            Value = formatted;
        }

        /// <summary>
        /// Attempts to create a valid email without throwing exceptions.
        /// </summary>
        /// <param name="value">The email address to validate.</param>
        /// <param name="result">The resulting Email if valid, or null if invalid.</param>
        /// <param name="errorMessage">Error message if validation fails.</param>
        /// <returns>True if the email is valid, false otherwise.</returns>
        public static bool TryCreate(string value, out Email? result, out string? errorMessage)
        {
            result = null;
            errorMessage = null;

            try
            {
                result = new Email(value);
                return true;
            }
            catch (InvalidEmailException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Returns the email address as a string.
        /// </summary>
        public override string ToString() => Value;
    }
}