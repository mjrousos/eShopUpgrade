using eShopLegacy.Utilities;
using eShopLegacyMVC.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace eShopLegacyMVC.Controllers.WebApi
{
    /// <summary>
    /// Web API controller for binary file serialization and data export operations.
    /// Provides specialized endpoints for retrieving catalog data in binary format
    /// for high-performance data exchange with external systems and batch processing.
    /// </summary>
    public class FilesController : ApiController
    {
        /// <summary>
        /// Catalog service for data access operations
        /// </summary>
        private ICatalogService _service;

        /// <summary>
        /// Initializes the files API controller with catalog service dependency.
        /// </summary>
        /// <param name="service">Catalog service implementation for data access</param>
        public FilesController(ICatalogService service)
        {
            _service = service;
        }

        /// <summary>
        /// Exports catalog brand data in binary serialized format for efficient data transfer.
        /// GET api/files
        /// Returns serialized brand data using custom binary serialization for optimal
        /// performance in bulk data exchange scenarios.
        /// </summary>
        /// <returns>HTTP response with binary serialized brand data</returns>
        public HttpResponseMessage Get()
        {
            var brands = _service.GetCatalogBrands()
                .Select(b => new BrandDTO
                {
                    Id = b.Id,
                    Brand = b.Brand
                }).ToList();
            var serializer = new Serializing();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(serializer.SerializeBinary(brands))
            };

            return response;
        }

        /// <summary>
        /// Data transfer object for brand information in serialization operations.
        /// Lightweight representation of catalog brand data optimized for binary serialization
        /// and external system integration.
        /// </summary>
        [Serializable]
        public class BrandDTO
        {
            /// <summary>
            /// Unique identifier of the catalog brand
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// Brand name for external system consumption
            /// </summary>
            public string Brand { get; set; }
        }
    }
}