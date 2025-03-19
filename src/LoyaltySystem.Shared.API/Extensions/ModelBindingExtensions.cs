using System;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LoyaltySystem.Shared.API.Extensions
{
    /// <summary>
    /// Extension methods for model binding to handle EntityId types consistently
    /// </summary>
    public static class ModelBindingExtensions
    {
        /// <summary>
        /// Gets a request parameter as an EntityId
        /// </summary>
        /// <param name="valueProvider">The value provider from model binding</param>
        /// <param name="name">The parameter name</param>
        /// <param name="result">The resulting EntityId if successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryGetEntityIdParameter<T>(
            this IValueProvider valueProvider,
            string name,
            out T result) where T : EntityId, new()
        {
            result = default;
            
            var valueResult = valueProvider.GetValue(name);
            if (valueResult == ValueProviderResult.None)
                return false;
                
            var stringValue = valueResult.FirstValue;
            if (string.IsNullOrEmpty(stringValue))
                return false;
                
            return EntityId.TryParse<T>(stringValue, out result);
        }
        
        /// <summary>
        /// Gets a request route, query, or form value as an EntityId
        /// </summary>
        /// <typeparam name="T">The specific EntityId type</typeparam>
        /// <param name="bindingContext">The model binding context</param>
        /// <param name="name">The parameter name</param>
        /// <param name="result">The resulting EntityId if successful</param>
        /// <returns>True if binding succeeded, false otherwise</returns>
        public static bool TryGetEntityId<T>(
            this ModelBindingContext bindingContext,
            string name,
            out T result) where T : EntityId, new()
        {
            result = default;
            
            // Try from route first
            if (bindingContext.ActionContext.RouteData.Values.TryGetValue(name, out var routeValue))
            {
                string stringValue = routeValue?.ToString();
                if (!string.IsNullOrEmpty(stringValue) && EntityId.TryParse<T>(stringValue, out result))
                    return true;
            }
            
            // Then try from value provider (query string, form values, etc.)
            return bindingContext.ValueProvider.TryGetEntityIdParameter(name, out result);
        }
    }
} 