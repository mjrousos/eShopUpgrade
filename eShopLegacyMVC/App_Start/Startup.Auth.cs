using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using eShopLegacyMVC.Models;

namespace eShopLegacyMVC
{
    /// <summary>
    /// Authentication configuration portion of the OWIN startup class.
    /// Configures ASP.NET Identity with cookie-based authentication for the catalog management system.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configures ASP.NET Identity authentication using cookie-based authentication.
        /// Sets up per-request instances of DbContext and Identity managers, and establishes
        /// cookie authentication with security stamp validation for enhanced security.
        /// Reference: https://go.microsoft.com/fwlink/?LinkId=301864
        /// </summary>
        /// <param name="app">OWIN application builder for configuring authentication middleware</param>
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
        }
    }
}