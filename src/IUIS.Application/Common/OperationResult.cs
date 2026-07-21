using System.Collections.Generic;

namespace IUIS.Application.Common
{
    public sealed class OperationResult
    {
        public OperationResult()
        {
            Errors = new List<string>();
        }

        public bool IsSuccess { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public IReadOnlyList<string> Errors { get; set; }

        public static OperationResult Success()
        {
            return new OperationResult { IsSuccess = true };
        }

        public static OperationResult Failure(string errorMessage)
        {
            return new OperationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Errors = new List<string> { errorMessage }
            };
        }

        public static OperationResult Failure(params string[] errors)
        {
            return new OperationResult
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }

    public sealed class OperationResult<T>
    {
        public OperationResult()
        {
            Errors = new List<string>();
        }

        public bool IsSuccess { get; set; }
        
        public T Data { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public IReadOnlyList<string> Errors { get; set; }

        public static OperationResult<T> Success(T data)
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static OperationResult<T> Failure(string errorMessage)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Errors = new List<string> { errorMessage }
            };
        }

        public static OperationResult<T> Failure(params string[] errors)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }
}
