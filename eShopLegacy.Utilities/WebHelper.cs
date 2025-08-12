using System.Web;

namespace eShopLegacy.Utilities
{
    /// <summary>
    /// Utility class providing helper methods for web-related operations and HTTP context access.
    /// Simplifies access to common web request information and browser characteristics
    /// for logging, analytics, and user experience customization purposes.
    /// </summary>
    public class WebHelper
    {
        /// <summary>
        /// Gets the user agent string from the current HTTP request.
        /// Used for browser detection, compatibility checks, and request logging.
        /// Returns the complete user agent header as provided by the client browser.
        /// </summary>
        public static string UserAgent => HttpContext.Current.Request.UserAgent;
    }
}