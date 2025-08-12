using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(eShopLegacyMVC.Startup))]
namespace eShopLegacyMVC
{
    /// <summary>
    /// OWIN startup configuration for the eShop Legacy MVC application.
    /// Configures the OWIN middleware pipeline including authentication services.
    /// This class follows the partial class pattern to separate authentication configuration
    /// from other potential OWIN middleware configurations.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Main OWIN configuration entry point. Sets up the middleware pipeline
        /// with authentication as the primary cross-cutting concern for the application.
        /// </summary>
        /// <param name="app">OWIN application builder for configuring middleware pipeline</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
