using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;

namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// Service for managing file operations on network file systems with Windows authentication.
    /// Provides secure file upload, download, and listing capabilities using Windows impersonation
    /// to access network resources with service account credentials. Supports both current user
    /// authentication and configured service account authentication for enterprise scenarios.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Windows API constant for default logon provider
        /// </summary>
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        
        /// <summary>
        /// Windows API constant for new credentials logon type (network authentication)
        /// </summary>
        private const int LOGON32_LOGON_NEWCREDENTIALS = 9;

        /// <summary>
        /// Configuration settings for file service operations including paths and credentials
        /// </summary>
        private readonly FileServiceConfiguration configuration;

        /// <summary>
        /// Windows API function for obtaining authentication tokens with credentials
        /// </summary>
        /// <param name="lpszUsername">Username for authentication</param>
        /// <param name="lpszDomain">Domain for authentication</param>
        /// <param name="lpszPassword">Password for authentication</param>
        /// <param name="dwLogonType">Type of logon operation</param>
        /// <param name="dwLogonProvider">Logon provider to use</param>
        /// <param name="phToken">Output parameter for the authentication token</param>
        /// <returns>True if authentication succeeds, false otherwise</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        /// <summary>
        /// Initializes the file service with configuration settings.
        /// </summary>
        /// <param name="configuration">File service configuration including base path and credentials</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
        public FileService(FileServiceConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Factory method to create a FileService instance with configuration from app settings.
        /// Reads file service configuration from appSettings including base path and service account credentials.
        /// </summary>
        /// <returns>Configured FileService instance</returns>
        public static FileService Create() =>
            new FileService(new FileServiceConfiguration
            {
                BasePath = ConfigurationManager.AppSettings["Files:BasePath"],
                ServiceAccountUsername = ConfigurationManager.AppSettings["Files:ServiceAccountUsername"],
                ServiceAccountDomain = ConfigurationManager.AppSettings["Files:ServiceAccountDomain"],
                ServiceAccountPassword = ConfigurationManager.AppSettings["Files:ServiceAccountPassword"]
            });

        /// <summary>
        /// Lists all files in the configured base directory using Windows authentication.
        /// Uses either the current user's credentials or configured service account credentials
        /// to access network file systems securely.
        /// </summary>
        /// <returns>Collection of file names in the base directory</returns>
        public IEnumerable<string> ListFiles()
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
            {
                return Directory.GetFiles(configuration.BasePath).Select(Path.GetFileName);
            }
        }

        /// <summary>
        /// Downloads a file from the configured base directory using Windows authentication.
        /// Reads the entire file into memory and returns as byte array for web delivery.
        /// </summary>
        /// <param name="filename">Name of the file to download</param>
        /// <returns>File contents as byte array</returns>
        public byte[] DownloadFile(string filename)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
            {
                var path = Path.Combine(configuration.BasePath, filename);
                return File.ReadAllBytes(path);
            }
        }

        /// <summary>
        /// Uploads multiple files to the configured base directory using Windows authentication.
        /// Processes all files in the HTTP request and saves them to the network file system.
        /// TODO: Switch to async operations when upgrading to .NET 8 for better performance.
        /// </summary>
        /// <param name="files">Collection of HTTP files from web request</param>
        public void UploadFile(HttpFileCollectionBase files)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var impersonationContext = WindowsIdentity.Impersonate(authToken))
            {

                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var filename = Path.GetFileName(file.FileName);
                    var path = Path.Combine(configuration.BasePath, filename);

                    using (var fs = File.Create(path))
                    {
                        // TODO - Switch to CopyToAsync when upgrading to .NET 8
                        file.InputStream.CopyTo(fs);
                    }
                }
            }
        }

        /// <summary>
        /// Obtains a Windows authentication token using service account credentials.
        /// Used for impersonating network service accounts to access file systems
        /// that require specific authentication credentials.
        /// </summary>
        /// <param name="username">Service account username</param>
        /// <param name="domain">Service account domain</param>
        /// <param name="password">Service account password</param>
        /// <returns>Windows authentication token for impersonation</returns>
        /// <exception cref="InvalidOperationException">Thrown when authentication fails</exception>
        private IntPtr GetAuthToken(string username, string domain, string password)
        {
            if (!LogonUser(username, domain, password, LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, out IntPtr authToken))
            {
                throw new InvalidOperationException($"Unable to get auth token for service account {username} in domain {domain}");
            }

            return authToken;
        }
    }
}