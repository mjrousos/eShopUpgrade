namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// Configuration settings for the FileService to enable network file system access.
    /// Contains the network path and optional Windows service account credentials
    /// required for authenticated file operations in enterprise environments.
    /// </summary>
    public class FileServiceConfiguration
    {
        /// <summary>
        /// Base directory path for file operations (local or network path)
        /// </summary>
        public string BasePath { get; set; }
        
        /// <summary>
        /// Windows domain for service account authentication (optional)
        /// </summary>
        public string ServiceAccountDomain { get; set; }
        
        /// <summary>
        /// Username for service account authentication (optional)
        /// </summary>
        public string ServiceAccountUsername { get; set; }
        
        /// <summary>
        /// Password for service account authentication (optional)
        /// </summary>
        public string ServiceAccountPassword { get; set; }
    }
}