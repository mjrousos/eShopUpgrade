﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace eShopLegacyMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        private int? _zipCode = null; 

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

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}