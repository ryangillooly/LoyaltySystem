using System.Collections.Generic;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Represents the result of an application operation.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool Success { get; private set; }
        
        /// <summary>
        /// The result data, if successful.
        /// </summary>
        public T Data { get; private set; }
        
        /// <summary>
        /// Error messages, if not successful.
        /// </summary>
        public List<string> Errors { get; private set; }
        
        /// <summary>
        /// Creates a successful result with data.
        /// </summary>
        public static OperationResult<T> SuccessResult(T data)
        {
            return new OperationResult<T> { Success = true, Data = data };
        }
        
        /// <summary>
        /// Creates a failure result with error messages.
        /// </summary>
        public static OperationResult<T> FailureResult(params string[] errors)
        {
            return new OperationResult<T> 
            { 
                Success = false, 
                Errors = new List<string>(errors) 
            };
        }
        
        /// <summary>
        /// Creates a failure result with a list of error messages.
        /// </summary>
        public static OperationResult<T> FailureResult(List<string> errors)
        {
            return new OperationResult<T> 
            { 
                Success = false, 
                Errors = errors ?? new List<string>() 
            };
        }
    }
} 