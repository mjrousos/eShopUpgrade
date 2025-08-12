using eShopLegacy.Models;
using System.Web.Mvc;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Controller for demonstrating ASP.NET session state management functionality.
    /// Provides examples of storing and retrieving session data for educational and
    /// testing purposes. Shows how session persistence works across HTTP requests
    /// and form submissions in the application.
    /// </summary>
    public class AspNetSessionController : Controller
    {
        /// <summary>
        /// Displays the session demonstration page with current session data.
        /// Retrieves any existing session demo data and displays it in the view.
        /// GET: /AspNetSession
        /// </summary>
        /// <returns>View with current session demo model or null if no session data exists</returns>
        public ActionResult Index()
        {
            var model = HttpContext.Session["DemoItem"];
            return View(model);
        }

        /// <summary>
        /// Processes session demo form submission and stores data in session state.
        /// Updates the session with new demo data and redisplays the form with
        /// the persisted information to demonstrate session functionality.
        /// POST: /AspNetSession
        /// </summary>
        /// <param name="demoModel">Session demo model with user input data</param>
        /// <returns>View with updated session demo model</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            HttpContext.Session["DemoItem"] = demoModel;
            return View(demoModel);
        }
    }
}
