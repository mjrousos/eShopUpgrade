using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Entity Framework database context for catalog management operations.
    /// Provides access to catalog items, brands, and types with configured relationships
    /// and database mappings. Uses Code-First approach with fluent API configuration
    /// for precise control over database schema and entity relationships.
    /// </summary>
    public class CatalogDBContext : DbContext
    {
        /// <summary>
        /// Initializes the catalog database context with connection string from configuration.
        /// Uses named connection string "CatalogDBContext" from Web.config.
        /// </summary>
        public CatalogDBContext() : base("name=CatalogDBContext")
        {
        }

        /// <summary>
        /// Database set for catalog items (products) with inventory and categorization data
        /// </summary>
        public DbSet<CatalogItem> CatalogItems { get; set; }

        /// <summary>
        /// Database set for catalog brands used for product categorization
        /// </summary>
        public DbSet<CatalogBrand> CatalogBrands { get; set; }

        /// <summary>
        /// Database set for catalog types used for product categorization
        /// </summary>
        public DbSet<CatalogType> CatalogTypes { get; set; }

        /// <summary>
        /// Configures entity relationships and database mappings using Entity Framework fluent API.
        /// Defines table mappings, primary keys, foreign key relationships, and column constraints
        /// for precise database schema control and optimal query performance.
        /// </summary>
        /// <param name="builder">Entity Framework model builder for configuration</param>
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            ConfigureCatalogType(builder.Entity<CatalogType>());
            ConfigureCatalogBrand(builder.Entity<CatalogBrand>());
            ConfigureCatalogItem(builder.Entity<CatalogItem>());

            base.OnModelCreating(builder);
        }

        /// <summary>
        /// Configures the CatalogType entity mapping and database constraints.
        /// Maps to "CatalogType" table with required string constraint for type names.
        /// </summary>
        /// <param name="builder">Entity type configuration builder for CatalogType</param>
        void ConfigureCatalogType(EntityTypeConfiguration<CatalogType> builder)
        {
            builder.ToTable("CatalogType");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
               .IsRequired();

            builder.Property(cb => cb.Type)
                .IsRequired()
                .HasMaxLength(100);
        }

        /// <summary>
        /// Configures the CatalogBrand entity mapping and database constraints.
        /// Maps to "CatalogBrand" table with required string constraint for brand names.
        /// </summary>
        /// <param name="builder">Entity type configuration builder for CatalogBrand</param>
        void ConfigureCatalogBrand(EntityTypeConfiguration<CatalogBrand> builder)
        {
            builder.ToTable("CatalogBrand");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
               .IsRequired();

            builder.Property(cb => cb.Brand)
                .IsRequired()
                .HasMaxLength(100);
        }

        /// <summary>
        /// Configures the CatalogItem entity mapping with relationships and database constraints.
        /// Maps to "Catalog" table with Hi-Lo sequence ID generation, foreign key relationships
        /// to brands and types, and exclusion of calculated properties from database persistence.
        /// </summary>
        /// <param name="builder">Entity type configuration builder for CatalogItem</param>
        void ConfigureCatalogItem(EntityTypeConfiguration<CatalogItem> builder)
        {
            builder.ToTable("Catalog");

            builder.HasKey(ci => ci.Id);

            // Configure Hi-Lo sequence pattern - IDs are not database-generated
            builder.Property(ci => ci.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .IsRequired();

            builder.Property(ci => ci.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ci => ci.Price)
                .IsRequired();

            builder.Property(ci => ci.PictureFileName)
                .IsRequired();

            // Exclude calculated/runtime property from database persistence
            builder.Ignore(ci => ci.PictureUri);

            // Configure foreign key relationship to CatalogBrand
            builder.HasRequired<CatalogBrand>(ci => ci.CatalogBrand)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogBrandId);

            // Configure foreign key relationship to CatalogType
            builder.HasRequired<CatalogType>(ci => ci.CatalogType)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogTypeId);
        }
    }
}
