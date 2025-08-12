using eShopLegacyMVC.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eShopLegacyMVC.Controllers
{
    /// <summary>
    /// Handles user authentication, registration, and account management for the eShop legacy catalog system.
    /// Implements ASP.NET Identity 2.0 with OWIN authentication middleware for secure user management.
    /// All actions except Login and Register require authentication by default.
    /// </summary>
    /// <remarks>
    /// This controller integrates with ApplicationUserManager and ApplicationSignInManager to provide:
    /// - User login with email/password authentication
    /// - New user registration with automatic sign-in
    /// - Secure logout with cookie cleanup
    /// - Protection against CSRF attacks through anti-forgery tokens
    /// - Local URL redirection to prevent open redirect vulnerabilities
    /// 
    /// Security considerations:
    /// - Account lockout is disabled (shouldLockout: false) to prevent DoS attacks
    /// - Email confirmation is commented out but can be enabled for production
    /// - Anti-forgery tokens protect state-changing operations
    /// - Local URL validation prevents malicious redirects
    /// </remarks>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// ASP.NET Identity sign-in manager for handling user authentication operations.
        /// Provides methods for password sign-in, external login, and sign-out functionality.
        /// </summary>
        private ApplicationSignInManager _signInManager;
        
        /// <summary>
        /// ASP.NET Identity user manager for handling user account operations.
        /// Provides methods for user creation, password validation, and account management.
        /// </summary>
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Initializes a new instance of the AccountController with dependency injection support.
        /// Managers will be resolved from OWIN context when accessed.
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountController with explicit manager dependencies.
        /// Used for dependency injection and unit testing scenarios.
        /// </summary>
        /// <param name="userManager">The user manager for account operations</param>
        /// <param name="signInManager">The sign-in manager for authentication operations</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Gets the sign-in manager from dependency injection or OWIN context.
        /// Handles user authentication, sign-in validation, and session management.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        /// <summary>
        /// Gets the user manager from dependency injection or OWIN context.
        /// Handles user creation, password management, and account validation.
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        /// <summary>
        /// Displays the login form for user authentication.
        /// Allows anonymous access and preserves the return URL for post-login redirection.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after successful login (must be local for security)</param>
        /// <returns>The login view with the return URL preserved in ViewBag</returns>
        /// <remarks>
        /// This action is marked with [AllowAnonymous] to enable unauthenticated users to access the login form.
        /// The return URL is validated later in RedirectToLocal() to prevent open redirect attacks.
        /// </remarks>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Processes user login attempts with email and password authentication.
        /// Validates credentials and establishes authenticated session on success.
        /// </summary>
        /// <param name="model">Login view model containing email, password, and remember me preference</param>
        /// <param name="returnUrl">The URL to redirect to after successful authentication</param>
        /// <returns>Redirect to return URL on success, or login view with errors on failure</returns>
        /// <remarks>
        /// Security features:
        /// - Anti-forgery token validation prevents CSRF attacks
        /// - Account lockout is disabled (shouldLockout: false) to prevent denial of service
        /// - Invalid credentials return generic error message to prevent user enumeration
        /// - Return URL validation prevents open redirect vulnerabilities
        /// 
        /// For production environments, consider enabling account lockout and implementing
        /// rate limiting to protect against brute force attacks.
        /// </remarks>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        /// <summary>
        /// Displays the user registration form for new account creation.
        /// Allows anonymous access to enable new users to register.
        /// </summary>
        /// <returns>The registration view</returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes new user registration with automatic sign-in on success.
        /// Creates new user account and immediately establishes authenticated session.
        /// </summary>
        /// <param name="model">Registration view model containing email, password, and confirmation</param>
        /// <returns>Redirect to catalog on success, or registration view with errors on failure</returns>
        /// <remarks>
        /// Registration process:
        /// 1. Validates model state (email format, password strength, confirmation match)
        /// 2. Creates new ApplicationUser with email as username
        /// 3. Saves user to database through UserManager
        /// 4. Automatically signs in the new user (isPersistent: false for session-only)
        /// 5. Redirects to catalog homepage
        /// 
        /// Email confirmation is commented out but available for production use.
        /// Consider enabling email verification for enhanced security and account validation.
        /// </remarks>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Catalog");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Logs out the current user by clearing authentication cookies and session data.
        /// Requires anti-forgery token to prevent cross-site request forgery attacks.
        /// </summary>
        /// <returns>Redirect to catalog homepage after successful logout</returns>
        /// <remarks>
        /// This action:
        /// 1. Clears the authentication cookie (DefaultAuthenticationTypes.ApplicationCookie)
        /// 2. Terminates the user's authenticated session
        /// 3. Redirects to the public catalog page
        /// 
        /// The [ValidateAntiForgeryToken] attribute protects against CSRF attacks where
        /// malicious sites could attempt to log out users without their consent.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Catalog");
        }

        /// <summary>
        /// Properly disposes of managed resources when the controller is destroyed.
        /// Ensures UserManager and SignInManager are properly disposed to prevent memory leaks.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        /// <summary>
        /// Anti-CSRF token key used for cross-site request forgery protection in external login scenarios.
        /// Ensures that external login callbacks cannot be hijacked by malicious sites.
        /// </summary>
        private const string XsrfKey = "XsrfId";

        /// <summary>
        /// Gets the OWIN authentication manager for handling authentication operations.
        /// Provides access to sign-in, sign-out, and external authentication providers.
        /// </summary>
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// Adds Identity framework validation errors to the ModelState for display in views.
        /// Converts IdentityResult errors into MVC-compatible validation messages.
        /// </summary>
        /// <param name="result">The IdentityResult containing validation errors</param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// Safely redirects to a local URL or falls back to the catalog homepage.
        /// Prevents open redirect vulnerabilities by validating that the URL is local to this application.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to if it's local and safe</param>
        /// <returns>Redirect to return URL if local, otherwise redirect to catalog homepage</returns>
        /// <remarks>
        /// This method is critical for security as it prevents attackers from crafting URLs
        /// that redirect users to malicious external sites after authentication.
        /// Only URLs that belong to this application domain are considered safe.
        /// </remarks>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Catalog");
        }

        /// <summary>
        /// Custom ActionResult for initiating external authentication challenges.
        /// Used for OAuth/OpenID providers like Google, Facebook, or Microsoft accounts.
        /// </summary>
        /// <remarks>
        /// This result type handles the OWIN authentication challenge flow for external providers:
        /// 1. Sets up authentication properties with redirect URL and CSRF protection
        /// 2. Initiates the external authentication challenge
        /// 3. Includes XSRF protection to prevent cross-site request forgery
        /// 
        /// Although not currently used in the basic email/password authentication,
        /// this infrastructure supports future external login provider integration.
        /// </remarks>
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            /// <summary>
            /// Initializes a new challenge result for external authentication.
            /// </summary>
            /// <param name="provider">The name of the external authentication provider</param>
            /// <param name="redirectUri">The URI to redirect to after authentication</param>
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            /// <summary>
            /// Initializes a new challenge result with user ID for XSRF protection.
            /// </summary>
            /// <param name="provider">The name of the external authentication provider</param>
            /// <param name="redirectUri">The URI to redirect to after authentication</param>
            /// <param name="userId">The user ID for XSRF protection</param>
            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            /// <summary>
            /// The external authentication provider name (e.g., "Google", "Facebook").
            /// </summary>
            public string LoginProvider { get; set; }
            
            /// <summary>
            /// The URI to redirect to after successful external authentication.
            /// </summary>
            public string RedirectUri { get; set; }
            
            /// <summary>
            /// The user ID for XSRF token validation.
            /// </summary>
            public string UserId { get; set; }

            /// <summary>
            /// Executes the authentication challenge by setting up OWIN authentication properties
            /// and initiating the external provider authentication flow.
            /// </summary>
            /// <param name="context">The controller context for the current request</param>
            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}