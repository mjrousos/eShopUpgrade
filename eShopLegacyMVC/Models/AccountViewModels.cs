using System.ComponentModel.DataAnnotations;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// View model for user login form containing email, password, and remember me preference.
    /// Includes data validation attributes to ensure proper user input and security.
    /// </summary>
    /// <remarks>
    /// This view model is used by the Account/Login views to capture user authentication credentials.
    /// Key validation features:
    /// - Email address format validation to ensure valid email input
    /// - Required field validation for email and password
    /// - Display names for proper form labeling
    /// - Password field masking for security
    /// 
    /// The RememberMe option allows users to maintain authentication across browser sessions
    /// through persistent cookies, but should be used carefully for security-sensitive applications.
    /// </remarks>
    public class LoginViewModel
    {
        /// <summary>
        /// The user's email address used as the login identifier.
        /// Must be a valid email format and is required for authentication.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// The user's password for authentication.
        /// Displayed as a password field to mask input for security.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the user wants to remain logged in across browser sessions.
        /// When true, creates a persistent authentication cookie that survives browser closure.
        /// </summary>
        /// <remarks>
        /// Security consideration: Persistent cookies should be used cautiously in shared
        /// or public computer environments as they maintain authentication state even
        /// after the browser is closed.
        /// </remarks>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// View model for new user registration form with email, password, and confirmation fields.
    /// Implements comprehensive validation rules to ensure secure account creation.
    /// </summary>
    /// <remarks>
    /// This view model enforces several security and usability requirements:
    /// - Email address format validation to ensure deliverable email
    /// - Password strength requirements (minimum 6 characters, maximum 100)
    /// - Password confirmation to prevent typos during registration
    /// - Proper field display names for user-friendly forms
    /// 
    /// The validation attributes provide both client-side and server-side validation
    /// to ensure data integrity and security before account creation.
    /// </remarks>
    public class RegisterViewModel
    {
        /// <summary>
        /// The user's email address which will become their username.
        /// Must be a valid email format and will be used for login and communication.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// The user's chosen password for account security.
        /// Must be between 6 and 100 characters long to balance security and usability.
        /// </summary>
        /// <remarks>
        /// Password requirements:
        /// - Minimum 6 characters to provide basic security
        /// - Maximum 100 characters to prevent potential DoS attacks
        /// - Displayed as password field to mask input
        /// 
        /// Consider implementing additional password complexity requirements
        /// (uppercase, lowercase, numbers, special characters) for enhanced security.
        /// </remarks>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Password confirmation field to verify the user entered their intended password.
        /// Must exactly match the Password field to prevent registration errors.
        /// </summary>
        /// <remarks>
        /// This field helps prevent user frustration from password typos during registration.
        /// The [Compare] attribute ensures the confirmation matches the original password
        /// and provides a user-friendly error message if they don't match.
        /// </remarks>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}