using System.Collections.Generic;

namespace LoyaltySystem.Domain.Common
{
    public class OperationResult<T>
    {
        private OperationResult(bool success, T? data, IEnumerable<string>? errors)
        {
            Success = success;
            Data = data;
            Errors = errors;
        }
        
        public bool Success { get; }
        public T? Data { get; }
        public IEnumerable<string>? Errors { get; }
        
        public static OperationResult<T> SuccessResult(T data)
        {
            return new OperationResult<T>(true, data, null);
        }
        
        public static OperationResult<T> FailureResult(string error)
        {
            return new OperationResult<T>(false, default, new[] { error });
        }
        
        public static OperationResult<T> FailureResult(IEnumerable<string> errors)
        {
            return new OperationResult<T>(false, default, errors);
        }
    }
    
    public class OperationResult
    {
        private OperationResult(bool success, IEnumerable<string>? errors)
        {
            Success = success;
            Errors = errors;
        }
        
        public bool Success { get; }
        public IEnumerable<string>? Errors { get; }
        
        public static OperationResult SuccessResult()
        {
            return new OperationResult(true, null);
        }
        
        public static OperationResult FailureResult(string error)
        {
            return new OperationResult(false, new[] { error });
        }
        
        public static OperationResult FailureResult(IEnumerable<string> errors)
        {
            return new OperationResult(false, errors);
        }
    }
} 