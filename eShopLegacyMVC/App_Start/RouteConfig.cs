using System.Web.Mvc;
using System.Web.Routing;

namespace eShopLegacyMVC
{
    /// <summary>
    /// Configures URL routing for the eShop Legacy MVC application.
    /// Defines how incoming URLs are mapped to controllers and actions,
    /// establishing the application's URL structure and navigation patterns.
    /// </summary>
    /// <remarks>
    /// This configuration establishes the following routing behavior:
    /// 1. Attribute-based routes take precedence over convention-based routes
    /// 2. ASP.NET Web Forms handlers (.axd files) are ignored for performance
    /// 3. Default route pattern follows MVC convention: {controller}/{action}/{id}
    /// 4. Application defaults to Catalog controller and Index action
    /// 
    /// The routing configuration directly impacts SEO, user experience, and
    /// the overall navigation structure of the eShop catalog application.
    /// </remarks>
    public class RouteConfig
    {
        /// <summary>
        /// Registers URL routing rules for the MVC application.
        /// Configures how incoming HTTP requests are mapped to controller actions
        /// and establishes the default navigation behavior for the catalog system.
        /// </summary>
        /// <param name="routes">The route collection to configure with application routes</param>
        /// <remarks>
        /// Route registration order is important - routes are evaluated in the order they're added:
        /// 
        /// 1. MapMvcAttributeRoutes(): Enables attribute-based routing on controllers/actions
        ///    - Allows custom routes defined with [Route] attributes
        ///    - Provides fine-grained control over specific controller actions
        ///    - Takes precedence over convention-based routes
        /// 
        /// 2. IgnoreRoute(): Prevents MVC from handling ASP.NET Web Forms handlers
        ///    - Improves performance by bypassing MVC pipeline for .axd resources
        ///    - Includes handlers like WebResource.axd, ScriptResource.axd
        /// 
        /// 3. Default route: Implements standard MVC convention-based routing
        ///    - Pattern: domain.com/Controller/Action/Id
        ///    - Defaults to Catalog/Index for the home page (domain.com/)
        ///    - Id parameter is optional for actions that don't require it
        /// 
        /// Business impact: The default route to Catalog/Index makes the product catalog
        /// the primary landing page, emphasizing the shopping experience over authentication.
        /// </remarks>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapMvcAttributeRoutes();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Catalog", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
