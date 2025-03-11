using System.Collections.Generic;

namespace LoyaltySystem.Application.Common
{
    /// <summary>
    /// Represents a paged result set with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The items in the current page.
        /// </summary>
        public IEnumerable<T> Items { get; }
        
        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public int TotalCount { get; }
        
        /// <summary>
        /// The current page number (1-based).
        /// </summary>
        public int Page { get; }
        
        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; }
        
        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; }
        
        /// <summary>
        /// Whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        
        /// <summary>
        /// Whether there is a next page.
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Creates a new paged result.
        /// </summary>
        public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (totalCount + pageSize - 1) / pageSize : 0;
        }
    }
} 