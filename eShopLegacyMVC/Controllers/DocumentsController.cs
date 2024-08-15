using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace eShopLegacyMVC.Controllers
{
    public class DocumentsController : Controller
    {
        private const string DefaultContentType = "application/octet-stream";

        private readonly FileService fileService;
        private readonly FileExtensionContentTypeProvider contentTypeProvider;

        public DocumentsController(FileService fileService)
        {
            this.fileService = fileService;
            this.contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        // GET: Files
        public ActionResult Index()
        {
            var files = fileService.ListFiles();
            return View(files);
        }

        [ResponseCache(VaryByQueryKeys = new string[] { "filename" }, Duration = int.MaxValue)]
        public FileResult Download(string filename)
        {
            var file = fileService.DownloadFile(filename);
            var fc = new FileContentResult(file, contentTypeProvider.TryGetContentType(filename, out var contentType) ? contentType : DefaultContentType)
            {
                FileDownloadName = filename
            };
            return fc;
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadDocument()
        {
            fileService.UploadFile(Request.Form.Files);
            return RedirectToAction("Index");
        }
    }
}