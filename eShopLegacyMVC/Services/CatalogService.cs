using eShopLegacyMVC.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using eShopLegacyMVC.ViewModel;

namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// Entity Framework implementation of catalog service for production database operations.
    /// Handles all catalog CRUD operations against SQL Server database using Entity Framework 6.
    /// Implements Hi-Lo sequence generation for catalog item IDs to ensure unique identifiers
    /// across multiple application instances.
    /// </summary>
    public class CatalogService : ICatalogService
    {
        /// <summary>
        /// Entity Framework database context for catalog operations
        /// </summary>
        private CatalogDBContext db;
        
        /// <summary>
        /// Hi-Lo sequence generator for creating unique catalog item identifiers
        /// </summary>
        private CatalogItemHiLoGenerator indexGenerator;

        /// <summary>
        /// Initializes a new instance of the catalog service with database context and ID generator.
        /// </summary>
        /// <param name="db">Entity Framework database context</param>
        /// <param name="indexGenerator">Hi-Lo sequence generator for unique ID creation</param>
        public CatalogService(CatalogDBContext db, CatalogItemHiLoGenerator indexGenerator)
        {
            this.db = db;
            this.indexGenerator = indexGenerator;
        }

        /// <summary>
        /// Retrieves a paginated view of catalog items with related brand and type information.
        /// Uses Entity Framework Include to eager load relationships and avoid N+1 query problems.
        /// Orders by ID to ensure consistent pagination across requests.
        /// </summary>
        /// <param name="pageSize">Maximum number of items to return per page</param>
        /// <param name="pageIndex">Zero-based page index</param>
        /// <returns>Paginated view model with items and metadata for UI rendering</returns>
        public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex)
        {
            var totalItems = db.CatalogItems.LongCount();

            var itemsOnPage = db.CatalogItems
                .Include(c => c.CatalogBrand)
                .Include(c => c.CatalogType)
                .OrderBy(c => c.Id)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToList();

            return new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);
        }

        /// <summary>
        /// Finds a specific catalog item by ID with related brand and type information loaded.
        /// Uses eager loading to minimize database round trips when displaying item details.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item</param>
        /// <returns>Catalog item with related data, or null if not found</returns>
        public CatalogItem FindCatalogItem(int id)
        {
            return db.CatalogItems.Include(c => c.CatalogBrand).Include(c => c.CatalogType).FirstOrDefault(ci => ci.Id == id);
        }
        
        /// <summary>
        /// Retrieves all catalog types for dropdown population and filtering.
        /// Returns the complete list for category-based product organization.
        /// </summary>
        /// <returns>All available catalog types in the system</returns>
        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return db.CatalogTypes;
        }

        /// <summary>
        /// Retrieves all catalog brands for dropdown population and filtering.
        /// Returns the complete list for brand-based product organization.
        /// </summary>
        /// <returns>All available catalog brands in the system</returns>
        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return db.CatalogBrands;
        }

        /// <summary>
        /// Creates a new catalog item in the database using Hi-Lo sequence generation for the ID.
        /// Ensures unique identifiers across multiple application instances and saves to database.
        /// The Hi-Lo pattern provides better performance than database-generated identity columns
        /// in high-throughput scenarios.
        /// </summary>
        /// <param name="catalogItem">New catalog item to create (ID will be auto-generated)</param>
        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            catalogItem.Id = indexGenerator.GetNextSequenceValue(db);
            db.CatalogItems.Add(catalogItem);
            db.SaveChanges();
        }

        /// <summary>
        /// Updates an existing catalog item with modified information.
        /// Marks the entity as modified in Entity Framework change tracker and persists changes.
        /// </summary>
        /// <param name="catalogItem">Catalog item with updated properties</param>
        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            db.Entry(catalogItem).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Removes a catalog item from the database.
        /// Handles Entity Framework change tracking and commits the deletion.
        /// </summary>
        /// <param name="catalogItem">Catalog item to remove from the system</param>
        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            db.CatalogItems.Remove(catalogItem);
            db.SaveChanges();
        }

        /// <summary>
        /// Disposes the Entity Framework database context to release resources.
        /// Called automatically by the dependency injection container at end of request scope.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }
    }
}