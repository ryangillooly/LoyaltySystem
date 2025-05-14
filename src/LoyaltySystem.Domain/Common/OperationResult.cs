using LoyaltySystem.Domain.Enums;
using System.Collections.Generic;

namespace LoyaltySystem.Domain.Common
{
    public class OperationResult<T>
    {
        private OperationResult(bool success, T? data, IEnumerable<string>? errors, OperationErrorType? errorType)
        {
            Success = success;
            Data = data;
            Errors = errors;
            ErrorType = errorType;
        }
        
        public bool Success { get; }
        public T? Data { get; }
        public IEnumerable<string>? Errors { get; }
        public OperationErrorType? ErrorType { get; set; } = OperationErrorType.None;
        
        public static OperationResult<T> SuccessResult(T data) => new (success:true, data, errors:null, errorType:null);
        public static OperationResult<T> FailureResult(string error, OperationErrorType errorType = OperationErrorType.Validation) => 
            new (success:false, data:default, new[] { error }, errorType);
        public static OperationResult<T> FailureResult(IEnumerable<string> errors, OperationErrorType errorType = OperationErrorType.Validation) => 
            new (success:false, data:default, errors, errorType);
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