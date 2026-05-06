namespace Core_API.Domain.Exceptions
{
    /// <summary>
    /// Base domain exception for all domain-level errors.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception thrown when email validation fails.
    /// </summary>

    public sealed class InvalidEmailException : DomainException
    {
        public string AttemptedValue { get; }
        public string? Reason { get; }

        public InvalidEmailException(string attemptedValue)
            : base($"Invalid email format: {attemptedValue}")
        {
            AttemptedValue = attemptedValue;
            Reason = "Invalid format";
        }

        public InvalidEmailException(string attemptedValue, string reason)
             : base($"Invalid email: {reason}. Value: {attemptedValue}")
        {
            AttemptedValue = attemptedValue;
            Reason = reason;
        }

        public InvalidEmailException(string attemptedValue, string reason, Exception inner)
            : base($"Invalid email: {reason}. Value: {attemptedValue}", inner)
        {
            AttemptedValue = attemptedValue;
            Reason = reason;
        }
    }

    /// <summary>
    /// Exception thrown when phone number validation fails.
    /// </summary>
    public sealed class InvalidPhoneException : DomainException
    {
        public string AttemptedValue { get; }
        public string? CountryCode { get; }
        public string? Reason { get; }
        public InvalidPhoneException(string attemptedValue)
               : base(GenerateMessage(attemptedValue, null, "Invalid format"))
        {
            AttemptedValue = attemptedValue;
            CountryCode = null;
            Reason = "Invalid format";
        }

        public InvalidPhoneException(string attemptedValue, string reason)
            : base(GenerateMessage(attemptedValue, null, reason))
        {
            AttemptedValue = attemptedValue;
            CountryCode = null;
            Reason = reason;
        }

        public InvalidPhoneException(string attemptedValue, string? countryCode, string reason)
            : base(GenerateMessage(attemptedValue, countryCode, reason))
        {
            AttemptedValue = attemptedValue;
            CountryCode = countryCode;
            Reason = reason;
        }

        public InvalidPhoneException(string attemptedValue, string? countryCode, string reason, Exception inner)
            : base(GenerateMessage(attemptedValue, countryCode, reason), inner)
        {
            AttemptedValue = attemptedValue;
            CountryCode = countryCode;
            Reason = reason;
        }

        private static string GenerateMessage(string value, string? countryCode, string reason)
        {
            return countryCode switch
            {
                null => $"Invalid phone number: {reason}. Value: {value}",
                _ => $"Invalid {countryCode} phone number: {reason}. Value: {value}"
            };
        }
    }

    /// <summary>
    /// Exception thrown when address validation fails.
    /// </summary>
    public sealed class InvalidAddressException : DomainException
    {
        public string? CountryCode { get; }
        public string? Field { get; }

        public InvalidAddressException(string message)
          : base(message)
        { }
        public InvalidAddressException(string message, string countryCode)
            : base($"Invalid address for {countryCode}: {message}")
        {
            CountryCode = countryCode;
        }
        public InvalidAddressException(string message, string countryCode, string field)
            : base($"Invalid address for {countryCode} - {field}: {message}")
        {
            CountryCode = countryCode;
            Field = field;
        }
        public InvalidAddressException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}