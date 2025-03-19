using System.Collections.Generic;

namespace LoyaltySystem.Shared.API.Models
{
    /// <summary>
    /// Standardized API response without data
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// List of error messages if Success is false
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }
        
        /// <summary>
        /// Creates an error response
        /// </summary>
        public static ApiResponse ErrorResponse(string message)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }
        
        /// <summary>
        /// Creates an error response with multiple error messages
        /// </summary>
        public static ApiResponse ErrorResponse(IEnumerable<string> errors)
        {
            var errorsList = errors as List<string> ?? new List<string>(errors);
            return new ApiResponse
            {
                Success = false,
                Message = string.Join("; ", errorsList),
                Errors = errorsList
            };
        }
    }
    
    /// <summary>
    /// Standardized API response with data
    /// </summary>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// The data returned by the API
        /// </summary>
        public T Data { get; set; }
        
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        
        /// <summary>
        /// Creates an error response
        /// </summary>
        public new static ApiResponse<T> ErrorResponse(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }
        
        /// <summary>
        /// Creates an error response with multiple error messages
        /// </summary>
        public new static ApiResponse<T> ErrorResponse(IEnumerable<string> errors)
        {
            var errorsList = errors as List<string> ?? new List<string>(errors);
            return new ApiResponse<T>
            {
                Success = false,
                Message = string.Join("; ", errorsList),
                Errors = errorsList
            };
        }
    }
} 