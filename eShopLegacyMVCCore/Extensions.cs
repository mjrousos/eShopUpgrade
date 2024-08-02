using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using eShopLegacyMVCCore.Models;
using Microsoft.Extensions.Options;

namespace eShopLegacyMVCCore
{
    public static class Extensions
    {
        public const string DataSettingsSectionName = "DataSettings";
        public static void AddCatalogServices(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(DataSettingsSectionName));
            builder.Services.AddScoped<ICatalogService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<DataSettings>>().Value;
                return settings.UseMockData
                    ? ActivatorUtilities.CreateInstance<CatalogServiceMock>(sp)
                    : ActivatorUtilities.CreateInstance<CatalogService>(sp);
            });
            builder.Services.AddScoped(sp => new CatalogDBContext(
                sp.GetRequiredService<IConfiguration>().GetConnectionString("CatalogConnection")
                ?? string.Empty));
            builder.Services.AddScoped<CatalogDBInitializer>();
            builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
        }
    }
}
