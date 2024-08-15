using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace eShopLegacyMVC.Services
{
    public class FileService
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_NEWCREDENTIALS = 9;

        private readonly FileServiceConfiguration configuration;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        public FileService(IOptions<FileServiceConfiguration> options)
        {
            this.configuration = options.Value;
        }

        public IEnumerable<string> ListFiles()
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var safeToken = new SafeAccessTokenHandle(authToken))
            {
                return WindowsIdentity.RunImpersonated(safeToken, () =>
                {
                    return Directory.GetFiles(configuration.BasePath).Select(Path.GetFileName);
                });
            }
        }

        public byte[] DownloadFile(string filename)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var safeToken = new SafeAccessTokenHandle(authToken))
            {
                return WindowsIdentity.RunImpersonated(safeToken, () =>
                {
                    var path = Path.Combine(configuration.BasePath, filename);
                    return File.ReadAllBytes(path);
                });
            }
        }

        public void UploadFile(IFormFileCollection files)
        {
            var authToken = string.IsNullOrEmpty(configuration.ServiceAccountUsername)
                ? WindowsIdentity.GetCurrent().Token
                : GetAuthToken(configuration.ServiceAccountUsername, configuration.ServiceAccountDomain, configuration.ServiceAccountPassword);

            using (var safeToken = new SafeAccessTokenHandle(authToken))
            {
                WindowsIdentity.RunImpersonated(safeToken, () =>
                {

                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var filename = Path.GetFileName(file.FileName);
                        var path = Path.Combine(configuration.BasePath, filename);

                        using (var fs = File.Create(path))
                        {
                            // TODO - Switch to CopyToAsync
                            file.CopyTo(fs);
                        }
                    }
                });
            }
        }

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