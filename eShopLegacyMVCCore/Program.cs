using eShopLegacyMVCCore;
using eShopLegacyMVCCore.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();

// Add services to the container.
builder.AddCatalogServices();
builder.AddFileServices();
builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection("MessageQueueSettings"));
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCaching();

// Add session-related services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddJavaScriptBundle("/bundles/jquery", "Scripts/jquery-*.js")
        .UseContentRoot();

    pipeline.AddJavaScriptBundle("/bundles/jqueryval", "Scripts/jquery.validate*")
        .UseContentRoot();

    pipeline.AddJavaScriptBundle("/bundles/modernizr", "Scripts/modernizr-*")
        .UseContentRoot();

    pipeline.AddJavaScriptBundle("/bundles/bootstrap", "Scripts/bootstrap.js", "Scripts/respond.js")
        .UseContentRoot();

    pipeline.AddCssBundle("/Content/css", 
                          "Content/bootstrap.css",
                          "Content/custom.css",
                          "Content/base.css",
                          "Content/site.css")
        .UseContentRoot();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();
app.UseResponseCaching();

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
