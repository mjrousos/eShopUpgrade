using Autofac;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Modules
{
    /// <summary>
    /// Autofac dependency injection module for configuring application services.
    /// Supports both production (Entity Framework) and development/testing (mock) scenarios
    /// based on the UseMockData configuration setting. Manages service lifetimes and
    /// ensures proper dependency resolution throughout the application.
    /// </summary>
    public class ApplicationModule : Module
    {
        /// <summary>
        /// Flag indicating whether to use mock data services instead of database services
        /// </summary>
        private bool useMockData;

        /// <summary>
        /// Initializes the application module with the mock data configuration.
        /// </summary>
        /// <param name="useMockData">True to use mock services, false for Entity Framework services</param>
        public ApplicationModule(bool useMockData)
        {
            this.useMockData = useMockData;
        }
        
        /// <summary>
        /// Configures service registrations in the Autofac container.
        /// Registers appropriate implementations based on the mock data flag and sets
        /// proper lifetime scopes for different service types.
        /// </summary>
        /// <param name="builder">Autofac container builder</param>
        protected override void Load(ContainerBuilder builder)
        {
            if (this.useMockData)
            {
                // Register mock service as singleton since it operates on in-memory data
                // that should persist across requests during development/testing
                builder.RegisterType<CatalogServiceMock>()
                    .As<ICatalogService>()
                    .SingleInstance();
            }
            else
            {
                // Register Entity Framework service with per-request lifetime
                // to ensure proper DbContext management and avoid threading issues
                builder.RegisterType<CatalogService>()
                    .As<ICatalogService>()
                    .InstancePerLifetimeScope();
            }

            // Register DbContext with per-request lifetime for proper disposal
            // and to avoid shared state issues in web applications
            builder.RegisterType<CatalogDBContext>()
                .InstancePerLifetimeScope();

            // Register database initializer with per-request lifetime
            // for proper Entity Framework initialization coordination
            builder.RegisterType<CatalogDBInitializer>()
                .InstancePerLifetimeScope();

            // Register Hi-Lo sequence generator as singleton since it maintains
            // sequence state across the application lifetime
            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();
        }
    }
}