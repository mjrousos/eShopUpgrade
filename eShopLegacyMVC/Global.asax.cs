using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using log4net;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace eShopLegacyMVC
{
    /// <summary>
    /// Main application entry point for the eShop Legacy MVC catalog management system.
    /// Configures dependency injection, routing, database initialization, and logging infrastructure.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Autofac dependency injection container for managing service lifetimes and registrations
        /// </summary>
        IContainer container;

        /// <summary>
        /// Application startup configuration - initializes all core systems including DI container,
        /// Web API configuration, MVC areas, filters, routing, bundling, and database setup
        /// </summary>
        protected void Application_Start()
        {
            container = RegisterContainer();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigDataBase();
        }

        /// <summary>
        /// Session start handler - captures machine name and session start time for tracking purposes.
        /// This information is used in the application footer to demonstrate session persistence
        /// and help with debugging in multi-server environments.
        /// </summary>
        protected void Session_Start(Object sender, EventArgs e)
        {
            HttpContext.Current.Session["MachineName"] = Environment.MachineName;
            HttpContext.Current.Session["SessionStartTime"] = DateTime.Now;
        }

        /// <summary>
        /// Request lifecycle handler - establishes logging context for each HTTP request.
        /// Sets up activity ID correlation and request information for comprehensive log tracking
        /// across the entire request pipeline.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //set the property to our new object
            LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();

            LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo();

            _log.Debug("WebApplication_BeginRequest");
        }

        /// <summary>
        /// Configures the Autofac dependency injection container for both MVC and Web API.
        /// Registers controllers and configures service implementations based on the UseMockData setting.
        /// This allows the application to run with either real database services or mock services for testing.
        /// Reference: http://docs.autofac.org/en/latest/integration/mvc.html
        /// </summary>
        /// <returns>Configured Autofac container with all service registrations</returns>
        protected IContainer RegisterContainer()
        {
            var builder = new ContainerBuilder();

            var thisAssembly = Assembly.GetExecutingAssembly();
            builder.RegisterControllers(thisAssembly);
            builder.RegisterApiControllers(thisAssembly);

            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"]);
            builder.RegisterModule(new ApplicationModule(mockData));

            var container = builder.Build();

            // set mvc resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // set webapi resolver
            var resolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

            return container;
        }

        /// <summary>
        /// Configures Entity Framework database initialization strategy.
        /// When using real data (not mock), sets up the custom CatalogDBInitializer to handle
        /// database creation, seeding, and schema updates for the product catalog.
        /// </summary>
        private void ConfigDataBase()
        {
            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"]);

            if (!mockData)
            {
                Database.SetInitializer<CatalogDBContext>(container.Resolve<CatalogDBInitializer>());
            }
        }

    }

    /// <summary>
    /// Helper class for generating and maintaining activity IDs for log correlation.
    /// Ensures each request thread has a unique identifier for tracking operations
    /// across different components and services within a single request lifecycle.
    /// </summary>
    public class ActivityIdHelper
    {
        /// <summary>
        /// Returns the current activity ID for this thread, creating a new one if none exists.
        /// This enables distributed tracing and log correlation across service boundaries.
        /// </summary>
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }
    }

    /// <summary>
    /// Helper class for capturing HTTP request context information for logging purposes.
    /// Provides detailed request information including URL and user agent for troubleshooting
    /// and monitoring user behavior patterns.
    /// </summary>
    public class WebRequestInfo
    {
        /// <summary>
        /// Returns formatted request information including the raw URL and user agent.
        /// Used by the logging framework to capture request context for each log entry.
        /// </summary>
        public override string ToString()
        {
            return HttpContext.Current?.Request?.RawUrl + ", " + HttpContext.Current?.Request?.UserAgent;
        }
    }
}
