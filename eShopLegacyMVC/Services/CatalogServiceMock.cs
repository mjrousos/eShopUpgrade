using System;
using System.Collections.Generic;
using System.Linq;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.ViewModel;

namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// In-memory mock implementation of catalog service for development and testing scenarios.
    /// Provides the same interface as the Entity Framework implementation but operates against
    /// preconfigured static data. Useful for demonstrations, unit testing, and development
    /// environments where database connectivity is not required or desired.
    /// </summary>
    public class CatalogServiceMock : ICatalogService
    {
        /// <summary>
        /// In-memory collection of catalog items for mock operations
        /// </summary>
        private List<CatalogItem> catalogItems;

        /// <summary>
        /// Initializes the mock service with preconfigured catalog data.
        /// Creates a copy of the static data to allow for modifications during runtime.
        /// </summary>
        public CatalogServiceMock()
        {
            catalogItems = new List<CatalogItem>(PreconfiguredData.GetPreconfiguredCatalogItems());
        }

        /// <summary>
        /// Returns a paginated view of catalog items from in-memory collection.
        /// Composes complete object graphs with related brand and type information
        /// to match the behavior of the Entity Framework implementation.
        /// </summary>
        /// <param name="pageSize">Number of items per page (default 10)</param>
        /// <param name="pageIndex">Zero-based page index (default 0)</param>
        /// <returns>Paginated view model with items and pagination metadata</returns>
        public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize = 10, int pageIndex = 0)
        {
            var items = ComposeCatalogItems(catalogItems);
            
            var itemsOnPage = items
                .OrderBy(c => c.Id)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToList();

            return new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, items.Count, itemsOnPage);
        }

        /// <summary>
        /// Finds a specific catalog item by ID from the in-memory collection.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item</param>
        /// <returns>Catalog item if found, null otherwise</returns>
        public CatalogItem FindCatalogItem(int id)
        {
            return catalogItems.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Returns all available catalog types from preconfigured data.
        /// </summary>
        /// <returns>Complete collection of catalog types</returns>
        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return PreconfiguredData.GetPreconfiguredCatalogTypes();
        }

        /// <summary>
        /// Returns all available catalog brands from preconfigured data.
        /// </summary>
        /// <returns>Complete collection of catalog brands</returns>
        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return PreconfiguredData.GetPreconfiguredCatalogBrands();
        }

        /// <summary>
        /// Creates a new catalog item in the in-memory collection.
        /// Generates a new ID by incrementing the maximum existing ID to simulate
        /// database auto-increment behavior.
        /// </summary>
        /// <param name="catalogItem">New catalog item to add (ID will be auto-generated)</param>
        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = catalogItems.Max(i => i.Id);
            catalogItem.Id = ++maxId;
            catalogItems.Add(catalogItem);
        }

        /// <summary>
        /// Updates an existing catalog item in the in-memory collection.
        /// Finds the original item by ID and replaces it with the modified version.
        /// </summary>
        /// <param name="modifiedItem">Catalog item with updated properties</param>
        public void UpdateCatalogItem(CatalogItem modifiedItem)
        {
            var originalItem = FindCatalogItem(modifiedItem.Id);
            if (originalItem != null)
            {
                catalogItems[catalogItems.IndexOf(originalItem)] = modifiedItem;
            }
        }

        /// <summary>
        /// Removes a catalog item from the in-memory collection.
        /// </summary>
        /// <param name="catalogItem">Catalog item to remove</param>
        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            catalogItems.Remove(catalogItem);
        }

        /// <summary>
        /// No-op dispose implementation since no unmanaged resources are used.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Composes complete catalog item objects with related brand and type information.
        /// Simulates the eager loading behavior of Entity Framework by manually linking
        /// related entities from preconfigured data collections.
        /// </summary>
        /// <param name="items">Collection of catalog items to enhance with related data</param>
        /// <returns>Catalog items with populated brand and type navigation properties</returns>
        private List<CatalogItem> ComposeCatalogItems(List<CatalogItem> items)
        {
            var catalogTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();
            var catalogBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();
            items.ForEach(i => i.CatalogBrand = catalogBrands.First(b => b.Id == i.CatalogBrandId));
            items.ForEach(i => i.CatalogType = catalogTypes.First(b => b.Id == i.CatalogTypeId));

            return items;
        }
    }
}