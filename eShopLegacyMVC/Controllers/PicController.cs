using eShopLegacyMVC.Services;
using log4net;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Specialized controller for serving product images with proper MIME type handling.
    /// Provides a dedicated endpoint for catalog item pictures with optimized caching
    /// and content delivery. Uses custom routing for SEO-friendly image URLs.
    /// </summary>
    public class PicController : Controller
    {
        /// <summary>
        /// Logger instance for tracking image serving operations
        /// </summary>
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Route name constant used for generating picture URLs throughout the application
        /// </summary>
        public const string GetPicRouteName = "GetPicRouteTemplate";

        /// <summary>
        /// Catalog service for retrieving item metadata and picture file names
        /// </summary>
        private ICatalogService service;

        /// <summary>
        /// Initializes the picture controller with catalog service dependency.
        /// </summary>
        /// <param name="service">Catalog service for item lookups</param>
        public PicController(ICatalogService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Serves product images for catalog items with proper MIME type headers.
        /// Custom route: GET /items/{catalogItemId}/pic
        /// Reads image files from the ~/Pics directory and returns them with appropriate
        /// content type headers for browser display and caching.
        /// </summary>
        /// <param name="catalogItemId">Unique identifier of the catalog item</param>
        /// <returns>File result with image data and MIME type, or NotFound if item/image doesn't exist</returns>
        [HttpGet]
        [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        public ActionResult Index(int catalogItemId)
        {
            _log.Info($"Now loading... /items/Index?{catalogItemId}/pic");

            if (catalogItemId <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = service.FindCatalogItem(catalogItemId);

            if (item != null)
            {
                var webRoot = Server.MapPath("~/Pics");
                var path = Path.Combine(webRoot, item.PictureFileName);

                string imageFileExtension = Path.GetExtension(item.PictureFileName);
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = System.IO.File.ReadAllBytes(path);

                return File(buffer, mimetype);
            }

            return HttpNotFound();
        }

        /// <summary>
        /// Determines the appropriate MIME type for image files based on file extension.
        /// Supports common web image formats and falls back to generic binary type
        /// for unrecognized extensions. Used to set proper Content-Type headers
        /// for browser compatibility and caching.
        /// </summary>
        /// <param name="extension">File extension including the leading dot (e.g., ".png")</param>
        /// <returns>MIME type string appropriate for the image format</returns>
        private string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            string mimetype;

            switch (extension)
            {
                case ".png":
                    mimetype = "image/png";
                    break;
                case ".gif":
                    mimetype = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimetype = "image/jpeg";
                    break;
                case ".bmp":
                    mimetype = "image/bmp";
                    break;
                case ".tiff":
                    mimetype = "image/tiff";
                    break;
                case ".wmf":
                    mimetype = "image/wmf";
                    break;
                case ".jp2":
                    mimetype = "image/jp2";
                    break;
                case ".svg":
                    mimetype = "image/svg+xml";
                    break;
                default:
                    // Fallback for unrecognized image formats
                    mimetype = "application/octet-stream";
                    break;
            }

            return mimetype;
        }
    }
}
