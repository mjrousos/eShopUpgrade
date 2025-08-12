using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Represents a product brand or manufacturer in the catalog system.
    /// Used for categorizing products by their brand identity (e.g., ".NET Foundation", "Microsoft").
    /// Provides a lookup table for brand-based filtering and product organization.
    /// </summary>
    public class CatalogBrand
    {
        /// <summary>
        /// Unique identifier for the catalog brand
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Brand name or manufacturer name displayed to users
        /// </summary>
        public string Brand { get; set; }
    }
}