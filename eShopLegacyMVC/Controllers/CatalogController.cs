using System.Collections.Generic;
using System.Configuration;
using System.Messaging;
using System.Net;
using System.Web.Mvc;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using log4net;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Main MVC controller for catalog management operations in the eShop system.
    /// Handles CRUD operations for catalog items, provides paginated browsing,
    /// and manages integration with message queues for item creation notifications.
    /// Serves both authenticated administrators and anonymous users browsing the catalog.
    /// </summary>
    public class CatalogController : Controller
    {
        /// <summary>
        /// Logger instance for tracking catalog operations and debugging
        /// </summary>
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Catalog service abstraction for data operations (supports both EF and mock implementations)
        /// </summary>
        private ICatalogService service;

        /// <summary>
        /// Initializes the catalog controller with the provided catalog service.
        /// Service implementation is determined by dependency injection configuration.
        /// </summary>
        /// <param name="service">Catalog service implementation (EF or mock)</param>
        public CatalogController(ICatalogService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Displays paginated catalog items with configurable page size and navigation.
        /// Default route: GET /Catalog or /Catalog/Index?pageSize=3&pageIndex=10
        /// Updates picture URIs for proper image display through the PicController.
        /// </summary>
        /// <param name="pageSize">Number of items per page (default 10)</param>
        /// <param name="pageIndex">Zero-based page index (default 0)</param>
        /// <returns>View with paginated catalog items</returns>
        public ActionResult Index(int pageSize = 10, int pageIndex = 0)
        {
            _log.Info($"Now loading... /Catalog/Index?pageSize={pageSize}&pageIndex={pageIndex}");
            var paginatedItems = service.GetCatalogItemsPaginated(pageSize, pageIndex);
            ChangeUriPlaceholder(paginatedItems.Data);
            return View(paginatedItems);
        }

        /// <summary>
        /// Displays detailed information for a specific catalog item.
        /// Includes product details, pricing, stock information, and product image.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item</param>
        /// <returns>Detail view for the catalog item, or NotFound if item doesn't exist</returns>
        public ActionResult Details(int? id)
        {
            _log.Info($"Now loading... /Catalog/Details?id={id}");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return HttpNotFound();
            }
            AddUriPlaceHolder(catalogItem);

            return View(catalogItem);
        }

        /// <summary>
        /// Displays the catalog item creation form with populated dropdown lists.
        /// Requires authentication - typically used by catalog administrators.
        /// </summary>
        /// <returns>Create view with empty catalog item and populated brand/type dropdowns</returns>
        public ActionResult Create()
        {
            _log.Info($"Now loading... /Catalog/Create");
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type");
            return View(new CatalogItem());
        }

        /// <summary>
        /// Processes catalog item creation with validation and business rule enforcement.
        /// Sends new item notification to message queue for downstream processing.
        /// Protects against overposting attacks using explicit property binding.
        /// Reference: https://go.microsoft.com/fwlink/?LinkId=317598
        /// </summary>
        /// <param name="catalogItem">New catalog item data from form submission</param>
        /// <returns>Redirect to Index on success, or Create view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Create?catalogItemName={catalogItem.Name}");
            if (ModelState.IsValid)
            {
                service.CreateCatalogItem(catalogItem);
                QueueItemCreatedMessage(catalogItem);
                return RedirectToAction("Index");
            }

            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        /// <summary>
        /// Sends a message to the configured message queue when a new catalog item is created.
        /// Uses MSMQ with XML message formatting for integration with downstream systems.
        /// The queue path is configured in appSettings as "NewItemQueuePath".
        /// </summary>
        /// <param name="catalogItem">Newly created catalog item to broadcast</param>
        private void QueueItemCreatedMessage(CatalogItem catalogItem)
        {
            using (var queue = new MessageQueue(ConfigurationManager.AppSettings["NewItemQueuePath"]))
            {
                var message = new Message
                {
                    Formatter = new XmlMessageFormatter(new[] { typeof(CatalogItem) }),
                    Body = catalogItem,
                    Label = "New catalog item"
                };

                queue.Send(message);
            }
        }

        /// <summary>
        /// Displays the catalog item edit form with current values and populated dropdown lists.
        /// Loads existing item data and prepares form for modification.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item to edit</param>
        /// <returns>Edit view with catalog item data, or NotFound if item doesn't exist</returns>
        public ActionResult Edit(int? id)
        {
            _log.Info($"Now loading... /Catalog/Edit?id={id}");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return HttpNotFound();
            }
            AddUriPlaceHolder(catalogItem);
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        /// <summary>
        /// Processes catalog item updates with validation and business rule enforcement.
        /// Excludes navigation properties and calculated fields from binding to prevent tampering.
        /// Reference: https://go.microsoft.com/fwlink/?LinkId=317598
        /// </summary>
        /// <param name="catalogItem">Updated catalog item data from form submission</param>
        /// <returns>Redirect to Index on success, or Edit view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "PictureUri,CatalogType,CatalogBrand")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Edit?id={catalogItem.Id}");
            if (ModelState.IsValid)
            {
                service.UpdateCatalogItem(catalogItem);
                return RedirectToAction("Index");
            }
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        /// <summary>
        /// Displays catalog item deletion confirmation page with item details.
        /// Shows item information before permanent removal from the system.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item to delete</param>
        /// <returns>Delete confirmation view, or NotFound if item doesn't exist</returns>
        public ActionResult Delete(int? id)
        {
            _log.Info($"Now loading... /Catalog/Delete?id={id}");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return HttpNotFound();
            }
            AddUriPlaceHolder(catalogItem);

            return View(catalogItem);
        }

        /// <summary>
        /// Processes catalog item deletion after user confirmation.
        /// Permanently removes the item from the system and redirects to catalog listing.
        /// </summary>
        /// <param name="id">Unique identifier of the catalog item to delete</param>
        /// <returns>Redirect to Index after successful deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _log.Info($"Now processing... /Catalog/DeleteConfirmed?id={id}");
            CatalogItem catalogItem = service.FindCatalogItem(id);
            service.RemoveCatalogItem(catalogItem);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Disposes of the catalog service and its underlying resources.
        /// Called automatically by the MVC framework when the controller is disposed.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected override void Dispose(bool disposing)
        {
            _log.Debug($"Now disposing");
            if (disposing)
            {
                service.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates picture URIs for a collection of catalog items to enable proper image display.
        /// Converts file names to routable URLs through the PicController.
        /// </summary>
        /// <param name="items">Collection of catalog items to update</param>
        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var catalogItem in items)
            {
                AddUriPlaceHolder(catalogItem);
            }
        }

        /// <summary>
        /// Generates the proper URI for a catalog item's picture using the PicController route.
        /// Enables images to be served through the dedicated picture controller with proper caching.
        /// </summary>
        /// <param name="item">Catalog item to update with picture URI</param>
        private void AddUriPlaceHolder(CatalogItem item)
        {
            item.PictureUri = this.Url.RouteUrl(PicController.GetPicRouteName, new { catalogItemId = item.Id }, this.Request.Url.Scheme);            
        }
    }
}
