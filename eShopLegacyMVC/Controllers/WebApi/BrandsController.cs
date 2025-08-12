using eShopLegacy.Utilities;
using eShopLegacyMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;

namespace eShopLegacyMVC.Controllers.WebApi
{
    /// <summary>
    /// Web API controller for catalog brand operations, providing RESTful endpoints
    /// for external system integration and AJAX operations. Supports listing all brands,
    /// retrieving specific brands by ID, and demonstration delete operations.
    /// </summary>
    public class BrandsController : ApiController
    {
        /// <summary>
        /// Catalog service for data access operations
        /// </summary>
        private ICatalogService _service;

        /// <summary>
        /// Initializes the brands API controller with catalog service dependency.
        /// </summary>
        /// <param name="service">Catalog service implementation (EF or mock)</param>
        public BrandsController(ICatalogService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all catalog brands for external system integration.
        /// GET api/brands
        /// </summary>
        /// <returns>Collection of all catalog brands in the system</returns>
        public IEnumerable<Models.CatalogBrand> Get()
        {
            var brands = _service.GetCatalogBrands();
            return brands;
        }

        /// <summary>
        /// Retrieves a specific catalog brand by its unique identifier.
        /// GET api/brands/{id}
        /// </summary>
        /// <param name="id">Unique identifier of the catalog brand</param>
        /// <returns>HTTP 200 with brand data if found, HTTP 404 if not found</returns>
        public IHttpActionResult Get(int id)
        {
            var brands = _service.GetCatalogBrands();
            var brand = brands.FirstOrDefault(x => x.Id == id);
            if (brand == null) return NotFound();

            return Ok(brand);
        }

        /// <summary>
        /// Demonstrates brand deletion API endpoint for testing purposes.
        /// DELETE api/brands/{id}
        /// Note: This is a demonstration endpoint only - no actual deletion occurs
        /// to maintain data integrity in the demo environment.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog brand to delete</param>
        /// <returns>HTTP 200 if brand exists (demo), HTTP 404 if brand not found</returns>
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var brandToDelete = _service.GetCatalogBrands().FirstOrDefault(x => x.Id == id);
            if (brandToDelete == null)
            {
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            // demo only - don't actually delete
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}