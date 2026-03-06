using System;

namespace CPR.Application.DTOs.Taxonomy
{
    /// <summary>Generic wrapper for paginated API responses.</summary>
    /// <typeparam name="T">The type of items in the page.</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>The items in the current page.</summary>
        public T[] Data { get; set; } = Array.Empty<T>();

        /// <summary>Current page number (1-based).</summary>
        public int Page { get; set; }

        /// <summary>Number of items per page.</summary>
        public int PerPage { get; set; }

        /// <summary>Total number of items across all pages.</summary>
        public int TotalItems { get; set; }

        /// <summary>Total number of pages.</summary>
        public int TotalPages { get; set; }
    }
}
