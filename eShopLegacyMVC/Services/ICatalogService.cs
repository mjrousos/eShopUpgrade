using System.Collections.Generic;
using eShopLegacyMVC.Models;
using System;
using eShopLegacyMVC.ViewModel;

namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// Service contract for managing product catalog operations in the eShop system.
    /// Provides abstraction layer for catalog CRUD operations, brand/type management,
    /// and paginated catalog browsing. Supports both Entity Framework and mock implementations
    /// for flexible deployment scenarios (development, testing, production).
    /// </summary>
    public interface ICatalogService : IDisposable
    {
        /// <summary>
        /// Retrieves a specific catalog item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the catalog item</param>
        /// <returns>The catalog item if found, null otherwise</returns>
        CatalogItem FindCatalogItem(int id);

        /// <summary>
        /// Retrieves all available catalog brands for product categorization.
        /// Used for filtering products by brand and populating brand dropdown lists.
        /// </summary>
        /// <returns>Collection of all catalog brands in the system</returns>
        IEnumerable<CatalogBrand> GetCatalogBrands();

        /// <summary>
        /// Retrieves a paginated view of catalog items for efficient browsing of large catalogs.
        /// Supports pagination to improve performance and user experience when dealing with
        /// extensive product inventories.
        /// </summary>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="pageIndex">Zero-based page index</param>
        /// <returns>Paginated view model containing items and pagination metadata</returns>
        PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex);

        /// <summary>
        /// Retrieves all available catalog types for product categorization.
        /// Used for filtering products by type and populating type dropdown lists.
        /// </summary>
        /// <returns>Collection of all catalog types in the system</returns>
        IEnumerable<CatalogType> GetCatalogTypes();

        /// <summary>
        /// Creates a new catalog item in the system.
        /// Handles business rule validation and sends creation notifications to message queue.
        /// </summary>
        /// <param name="catalogItem">The catalog item to create</param>
        void CreateCatalogItem(CatalogItem catalogItem);

        /// <summary>
        /// Updates an existing catalog item with new information.
        /// Maintains inventory thresholds and business rule compliance.
        /// </summary>
        /// <param name="catalogItem">The catalog item with updated information</param>
        void UpdateCatalogItem(CatalogItem catalogItem);

        /// <summary>
        /// Removes a catalog item from the system.
        /// Handles cascading effects and maintains data integrity.
        /// </summary>
        /// <param name="catalogItem">The catalog item to remove</param>
        void RemoveCatalogItem(CatalogItem catalogItem);
    }
}