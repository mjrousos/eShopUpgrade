using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Represents a product category or type in the catalog system.
    /// Used for categorizing products by their functional category (e.g., "Mug", "T-Shirt", "USB Memory Stick").
    /// Provides a lookup table for type-based filtering and product organization.
    /// </summary>
    public class CatalogType
    {
        /// <summary>
        /// Unique identifier for the catalog type
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Type name or category name displayed to users for product classification
        /// </summary>
        public string Type { get; set; }
    }
}