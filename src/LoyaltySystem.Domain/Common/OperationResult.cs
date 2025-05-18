using LoyaltySystem.Domain.Enums;
using System.Collections.Generic;

namespace LoyaltySystem.Domain.Common
{
    public class OperationResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult{T}"/> class with the specified success status, data, errors, and error type.
        /// </summary>
        /// <param name="success">Indicates whether the operation was successful.</param>
        /// <param name="data">The data returned by the operation, or null if not applicable.</param>
        /// <param name="errors">A collection of error messages, or null if the operation succeeded.</param>
        /// <param name="errorType">The type of error, or null if the operation succeeded.</param>
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
        
        /// <summary>
/// Creates a successful operation result containing the specified data.
/// </summary>
/// <param name="data">The data returned by the successful operation.</param>
/// <returns>An <see cref="OperationResult{T}"/> indicating success and containing the provided data.</returns>
public static OperationResult<T> SuccessResult(T data) => new (success:true, data, errors:null, errorType:null);
        /// <summary>
            /// Creates a failed operation result with a single error message and an optional error type.
            /// </summary>
            /// <param name="error">The error message describing the failure.</param>
            /// <param name="errorType">The type of error. Defaults to Validation.</param>
            /// <returns>An <see cref="OperationResult{T}"/> representing a failed operation.</returns>
            public static OperationResult<T> FailureResult(string error, OperationErrorType errorType = OperationErrorType.Validation) => 
            new (success:false, data:default, new[] { error }, errorType);
        /// <summary>
            /// Creates a failed operation result with the specified error messages and error type.
            /// </summary>
            /// <param name="errors">A collection of error messages describing the failure.</param>
            /// <param name="errorType">The type of error that occurred. Defaults to <c>Validation</c>.</param>
            /// <returns>An <see cref="OperationResult{T}"/> representing a failed operation.</returns>
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