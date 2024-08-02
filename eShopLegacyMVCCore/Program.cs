using eShopLegacyMVCCore;
using eShopLegacyMVCCore.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();

// Add services to the container.
builder.AddCatalogServices();
builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection("MessageQueueSettings"));
builder.Services.AddControllersWithViews();

// Add session-related services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.UseSession();

app.MapControllerRoute(
    "Default",
    "{controller=Catalog}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    "DefaultApi",
    "api/{controller}/{id?}"
);

app.Run();
