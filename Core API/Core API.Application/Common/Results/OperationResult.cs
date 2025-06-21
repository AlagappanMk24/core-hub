using Microsoft.AspNetCore.Identity;

namespace Core_API.Application.Common.Results
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public string ErrorMessage { get; }
        public IEnumerable<IdentityError> Errors { get; }
        private OperationResult(bool isSuccess, T data, string errorMessage, IEnumerable<IdentityError> errors = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            Errors = errors ?? new List<IdentityError>();
        }
        public static OperationResult<T> SuccessResult(T data)
            => new OperationResult<T>(true, data, null);
        public static OperationResult<T> FailureResult(string errorMessage)
            => new OperationResult<T>(false, default, errorMessage);
        public static OperationResult<T> FailureResult(IEnumerable<IdentityError> errors, string errorMessage = null)
        => new OperationResult<T>(false, default, errorMessage, errors);
    }
}
