using System;
using System.Collections.Generic;

namespace eShopLegacyMVC.ViewModel
{
    /// <summary>
    /// Generic view model for paginated data display in the catalog system.
    /// Provides pagination metadata along with the actual data items for efficient
    /// browsing of large datasets. Used primarily for catalog item listing with
    /// page navigation controls.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities being paginated (typically CatalogItem)</typeparam>
    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {
        /// <summary>
        /// Current page index (zero-based) being displayed
        /// </summary>
        public int ActualPage { get; private set; }

        /// <summary>
        /// Number of items displayed per page
        /// </summary>
        public int ItemsPerPage { get; private set; }

        /// <summary>
        /// Total number of items available across all pages
        /// </summary>
        public long TotalItems { get; private set; }

        /// <summary>
        /// Total number of pages required to display all items
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Collection of entities for the current page
        /// </summary>
        public IEnumerable<TEntity> Data { get; private set; }

        /// <summary>
        /// Initializes a new paginated view model with pagination metadata and data.
        /// Automatically calculates total pages based on item count and page size.
        /// </summary>
        /// <param name="pageIndex">Zero-based current page index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="count">Total number of items available</param>
        /// <param name="data">Items for the current page</param>
        public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
        {
            ActualPage = pageIndex;
            ItemsPerPage = pageSize;
            TotalItems = count;
            TotalPages = (int)Math.Ceiling(((decimal)count / pageSize));
            Data = data;
        }
    }
}
