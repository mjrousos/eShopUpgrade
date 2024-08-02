using System.Web;

namespace eShopLegacy.Utilities
{
    public class WebHelper
    {
        // TODO : This is using System.Web.Adapters. Replace with ASP.NET Core native APIs once callers are all updated.
        public static string UserAgent => HttpContext.Current.Request.Headers["User-Agent"].ToString();
    }
}