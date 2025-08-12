using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Extended user entity for ASP.NET Identity with additional eShop-specific properties.
    /// Inherits from IdentityUser to provide standard authentication capabilities while adding
    /// business-specific features like zip code lookup from external user service.
    /// </summary>
    /// <remarks>
    /// This class extends the base IdentityUser with:
    /// - Zip code property that lazy-loads from external UserLookup service
    /// - Claims identity generation for authentication cookies
    /// - Support for custom user claims in the identity system
    /// 
    /// Security considerations:
    /// - External service calls are made synchronously (could impact performance)
    /// - No caching mechanism for zip code data (repeated calls for same user)
    /// - Service endpoint is hardcoded (consider moving to configuration)
    /// - No error handling for external service failures
    /// </remarks>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Generates a claims identity for the user with authentication cookie support.
        /// Creates the identity token used for authentication and authorization throughout the application.
        /// </summary>
        /// <param name="manager">The user manager for creating the identity</param>
        /// <returns>A ClaimsIdentity containing user information and custom claims</returns>
        /// <remarks>
        /// This method creates the ClaimsIdentity that will be stored in the authentication cookie.
        /// The authenticationType must match CookieAuthenticationOptions.AuthenticationType
        /// configured in Startup.cs. Custom claims can be added here for role-based access control
        /// or user-specific permissions.
        /// </remarks>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        /// <summary>
        /// Cached zip code value to avoid repeated external service calls.
        /// Initialized as null and populated on first access through ZipCode property.
        /// </summary>
        private int? _zipCode = null; 

        /// <summary>
        /// Gets the user's zip code by calling an external UserLookup service.
        /// The value is cached after the first call to improve performance.
        /// </summary>
        /// <returns>The user's zip code as an integer, or null if not available</returns>
        /// <remarks>
        /// This property makes a synchronous HTTP call to an external service at:
        /// http://10.0.0.42/UserLookup.svc/zipCode?id={userId}
        /// 
        /// Performance and reliability considerations:
        /// - Synchronous call may block the UI thread and impact responsiveness
        /// - No timeout handling or retry logic for service failures
        /// - Service endpoint is hardcoded and not configurable
        /// - No error handling if service returns invalid data or is unavailable
        /// - Consider implementing async pattern and proper error handling for production use
        /// 
        /// The zip code is cached in _zipCode field to avoid repeated service calls,
        /// but cache is lost when the object is garbage collected.
        /// </remarks>
        public int? ZipCode
        {
            get
            {
                if (_zipCode is null)
                {
                    var uri = string.Format("http://10.0.0.42/UserLookup.svc/zipCode?id={0}", Id);
                    var req = HttpWebRequest.Create(uri) as HttpWebRequest;
                    req.Method = "GET";
                    req.ServicePoint.Expect100Continue = false;

                    var response = req.GetResponse();
                    var responseStream = response.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        var zipCode = reader.ReadToEnd();
                        _zipCode = int.Parse(zipCode);
                    }
                }
                return _zipCode;
            }
        }
    }

    /// <summary>
    /// Entity Framework database context for ASP.NET Identity user management.
    /// Extends IdentityDbContext to provide database access for user authentication,
    /// roles, claims, and other identity-related data.
    /// </summary>
    /// <remarks>
    /// This context manages the Identity framework tables:
    /// - AspNetUsers: User accounts and authentication data
    /// - AspNetRoles: Application roles for authorization
    /// - AspNetUserRoles: User-to-role mappings
    /// - AspNetUserClaims: Custom claims for users
    /// - AspNetUserLogins: External login provider mappings
    /// 
    /// Configuration:
    /// - Uses "IdentityDBContext" connection string from web.config
    /// - throwIfV1Schema: false allows migration from older Identity versions
    /// - Separate from CatalogDBContext to maintain separation of concerns
    /// 
    /// The context is designed to be used with dependency injection through
    /// the Create() factory method for OWIN integration.
    /// </remarks>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of ApplicationDbContext with the Identity connection string.
        /// Configures the context to use SQL Server with Entity Framework migrations support.
        /// </summary>
        /// <remarks>
        /// The context uses:
        /// - "IdentityDBContext" connection string from configuration
        /// - throwIfV1Schema: false to support upgrading from ASP.NET Identity 1.0 schemas
        /// - ApplicationUser as the custom user entity type
        /// </remarks>
        public ApplicationDbContext()
            : base("IdentityDBContext", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Factory method for creating ApplicationDbContext instances.
        /// Used by OWIN middleware and dependency injection containers to create context instances.
        /// </summary>
        /// <returns>A new ApplicationDbContext instance configured for Identity operations</returns>
        /// <remarks>
        /// This static factory method is the recommended way to create context instances
        /// in OWIN-based applications. It ensures consistent configuration and supports
        /// the OWIN per-request lifetime pattern used by ASP.NET Identity.
        /// 
        /// The context created by this method will automatically be disposed
        /// at the end of each HTTP request by the OWIN pipeline.
        /// </remarks>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}