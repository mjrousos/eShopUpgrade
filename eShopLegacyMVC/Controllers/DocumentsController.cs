using eShopLegacyMVC.Services;
using System.Web;
using System.Web.Mvc;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Controller for managing business document uploads, downloads, and listing operations.
    /// Provides secure file management capabilities using the FileService with Windows authentication
    /// for accessing network file systems. Supports caching for improved download performance.
    /// </summary>
    public class DocumentsController : Controller
    {
        /// <summary>
        /// Displays a list of all available documents in the file system.
        /// Uses the FileService to securely access network file locations with appropriate authentication.
        /// </summary>
        /// <returns>View with collection of available file names</returns>
        public ActionResult Index()
        {
            var files = FileService.Create().ListFiles();
            return View(files);
        }

        /// <summary>
        /// Downloads a specific document file with aggressive caching for performance.
        /// Uses output caching to minimize file system access for frequently downloaded documents.
        /// Automatically detects MIME type based on file extension for proper browser handling.
        /// </summary>
        /// <param name="filename">Name of the file to download</param>
        /// <returns>File content result with appropriate MIME type and download name</returns>
        [OutputCache(VaryByParam = "filename", Duration = int.MaxValue)]
        public FileResult Download(string filename)
        {
            var fileService = FileService.Create();
            var file = fileService.DownloadFile(filename);
            FileContentResult fc = new FileContentResult(file, MimeMapping.GetMimeMapping(filename));
            fc.FileDownloadName = filename;
            return fc;
        }

        /// <summary>
        /// Displays the document upload form for adding new business documents.
        /// Requires authentication for document management operations.
        /// </summary>
        /// <returns>Upload form view</returns>
        public ActionResult Upload()
        {
            return View();
        }

        /// <summary>
        /// Processes document upload requests from the web form.
        /// Handles multiple file uploads and stores them securely using FileService.
        /// Redirects to document listing after successful upload.
        /// </summary>
        /// <returns>Redirect to Index action after upload completion</returns>
        [HttpPost]
        public ActionResult UploadDocument()
        {
            var fileService = FileService.Create();
            fileService.UploadFile(Request.Files);
            return RedirectToAction("Index");
        }
    }
}