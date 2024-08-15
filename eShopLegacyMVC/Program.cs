using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddWebOptimizer(pipeline =>
        {
            pipeline.AddJavaScriptBundle("/bundles/jquery", "Scripts/jquery-*.js").UseContentRoot();
            pipeline.AddJavaScriptBundle("/bundles/jqueryval", "Scripts/jquery.validate*").UseContentRoot();
            pipeline.AddJavaScriptBundle("/bundles/modernizr", "Scripts/modernizr-*").UseContentRoot();
            pipeline.AddJavaScriptBundle("/bundles/bootstrap", "Scripts/bootstrap.js", "Scripts/respond.js").UseContentRoot();
            pipeline.AddCssBundle("/Content/css", "Content/bootstrap.css", "Content/custom.css", "Content/base.css", "Content/site.css").UseContentRoot();

            if (!builder.Environment.IsDevelopment())
            {
                pipeline.MinifyCssFiles();
                pipeline.MinifyJsFiles();
            }
        });

        builder.Services.AddSession();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.Configure<FileServiceConfiguration>(builder.Configuration.GetSection("FileSettings"));
        builder.Services.AddScoped<FileService>();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDBContext")));

        // This adds authentication services (cookie scheme), also
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.AllowedForNewUsers = true;

            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                options.ViewLocationFormats.Add("C:\\src\\mjrousos\\eShopUpgrade\\eShopLegacyMVC\\Views\\{1}\\{0}" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
            });

        builder.Host
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(configure =>
            {
                var useMocks = bool.TryParse(builder.Configuration["UseMockData"], out bool parsedOutput) ? parsedOutput : false;
                configure.RegisterModule(new ApplicationModule(useMocks));
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Catalog/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseWebOptimizer();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseResponseCaching();
        app.UseSession();

        // Session_Start
        app.Use(async (context, next) =>
        {
            if (context.Session.IsAvailable && !context.Session.Keys.Contains("MachineName"))
            {
                context.Session.SetString("MachineName", Environment.MachineName);
            }

            if (context.Session.IsAvailable && !context.Session.Keys.Contains("SessionStartTime"))
            {
                context.Session.SetString("SessionStartTime", DateTime.Now.ToString());
            }

            await next();
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Catalog}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "DefaultApi",
            pattern: "api/{controller}/{id?}");

        app.Run();
    }
}