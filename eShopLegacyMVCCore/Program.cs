
using eShopLegacyMVCCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();

// Add services to the container.
var useMockData = bool.TryParse(builder.Configuration["UseMockData"], out bool parseOut) ? parseOut : false;
builder.AddCatalogServices();
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    "Default",
    "{controller=Catalog}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    "DefaultApi",
    "api/{controller}/{id?}"
);

app.Run();
