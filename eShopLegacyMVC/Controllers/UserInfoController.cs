using System.Web.Mvc;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Controller for displaying authenticated user information and claims.
    /// Provides secure access to user profile data, authentication status,
    /// and integrated external service information for logged-in users only.
    /// Demonstrates ASP.NET Identity integration and claims-based authentication.
    /// </summary>
    public class UserInfoController : Controller
    {
        /// <summary>
        /// Displays comprehensive user information page for authenticated users.
        /// Shows user identity, claims, authentication context, and integration
        /// with external services like user lookup and weather data.
        /// Requires authentication - unauthorized users will be redirected to login.
        /// </summary>
        /// <returns>User information view with current user context data</returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}