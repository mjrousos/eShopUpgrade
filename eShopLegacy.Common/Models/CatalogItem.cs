using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Represents a product item in the catalog management system.
    /// Contains product information, pricing, inventory levels, and categorization.
    /// Implements business rules for stock management and warehouse constraints.
    /// </summary>
    public class CatalogItem
    {
        /// <summary>
        /// Default placeholder image filename used when no product image is specified
        /// </summary>
        public const string DefaultPictureName = "dummy.png";

        /// <summary>
        /// Initializes a new catalog item with default picture filename
        /// </summary>
        public CatalogItem()
        {
            PictureFileName = DefaultPictureName;
        }
        
        /// <summary>
        /// Unique identifier for the catalog item, generated using Hi-Lo sequence pattern
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name - required field for catalog display and search
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Detailed product description for customer information
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Product price with validation for currency format and reasonable range.
        /// Uses decimal(18,2) precision for financial calculations.
        /// </summary>
        [RegularExpression(@"^\d+(\.\d{0,2})*$", ErrorMessage = "The field Price must be a positive number with maximum two decimals.")]
        [Range(0, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        /// <summary>
        /// Filename of the product image stored in the file system
        /// </summary>
        [Display(Name = "Picture name")]
        public string PictureFileName { get; set; }

        /// <summary>
        /// URI path for accessing the product image through the web application
        /// </summary>
        public string PictureUri { get; set; }

        /// <summary>
        /// Foreign key reference to the catalog type (category) for product classification
        /// </summary>
        [Display(Name = "Type")]
        public int CatalogTypeId { get; set; }

        /// <summary>
        /// Navigation property to the catalog type entity for category information
        /// </summary>
        [Display(Name = "Type")]
        public CatalogType CatalogType { get; set; }

        /// <summary>
        /// Foreign key reference to the catalog brand for product brand classification
        /// </summary>
        [Display(Name = "Brand")]
        public int CatalogBrandId { get; set; }

        /// <summary>
        /// Navigation property to the catalog brand entity for brand information
        /// </summary>
        [Display(Name = "Brand")]
        public CatalogBrand CatalogBrand { get; set; }

        /// <summary>
        /// Current quantity of product available in stock for sale.
        /// Constrained by warehouse capacity limits (0 to 10 million units).
        /// </summary>
        [Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        [Display(Name = "Stock")]
        public int AvailableStock { get; set; }

        /// <summary>
        /// Stock level threshold at which the system should trigger a reorder.
        /// When AvailableStock falls below this value, OnReorder flag should be set to true.
        /// </summary>
        [Range(0, 10000000, ErrorMessage = "The field Restock must be between 0 and 10 million.")]
        [Display(Name = "Restock")]
        public int RestockThreshold { get; set; }

        /// <summary>
        /// Maximum number of units that can be stored in inventory due to physical
        /// or logistical constraints in warehouses. Prevents over-ordering beyond capacity.
        /// </summary>
        [Range(0, 10000000, ErrorMessage = "The field Max stock must be between 0 and 10 million.")]
        [Display(Name = "Max stock")]
        public int MaxStockThreshold { get; set; }

        /// <summary>
        /// Indicates whether this item has been flagged for reordering.
        /// Set to true when AvailableStock falls below RestockThreshold.
        /// Used to trigger purchasing workflows and inventory management processes.
        /// </summary>
        public bool OnReorder { get; set; }
    }
}