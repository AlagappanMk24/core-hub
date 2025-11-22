namespace Core_API.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
    }
    public sealed class InvalidEmailException : DomainException
    {
        public string AttemptedValue { get; }

        public InvalidEmailException(string attemptedValue)
            : base($"Invalid email format: {attemptedValue}")
        {
            AttemptedValue = attemptedValue;
        }

        public InvalidEmailException(string attemptedValue, string message)
            : base(message)
        {
            AttemptedValue = attemptedValue;
        }
    }
    public sealed class InvalidPhoneException(string attemptedValue, string? countryCode = null) : DomainException(GenerateMessage(attemptedValue, countryCode))
    {
        public string AttemptedValue { get; } = attemptedValue;
        public string? CountryCode { get; } = countryCode;
        private static string GenerateMessage(string value, string? countryCode)
        {
            return countryCode switch
            {
                null => $"Invalid phone number format: {value}",
                _ => $"Invalid {countryCode} phone number format: {value}"
            };
        }
    }
}
