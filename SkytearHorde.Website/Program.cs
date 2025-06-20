using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using OfficeOpenXml;
using OpenTelemetry.Metrics;
using Prometheus;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Cache;
using SkytearHorde.EventHandlers;
using SkytearHorde.Seo;
using Umbraco.Cms.Core.Notifications;
using AdServer.Extensions;
using OpenTelemetry.Trace;
using Sentry.Profiling;
using Sentry.OpenTelemetry;
using Slimsy.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using SkytearHorde.Business.Config;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddNotificationHandler<AfterMetaTagsNotification, DeckMetaTagsEventHandler>()
    .AddNotificationHandler<ContentPublishedNotification, CardOverviewCacheClearer>()
    //.AddNotificationHandler<ContentPublishedNotification, CardSetConnectEventHandler>()
    .AddNotificationHandler<ContentPublishedNotification, CardVariantsEventHandler>()
    //.AddNotificationHandler<ContentPublishedNotification, CardSortingEventHandler>()
    .AddSlimsy()
    .Build();

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var config = new CardGameSettingsConfig();
var section = builder.Configuration.GetSection("CardGameSettings");
builder.Services.Configure<CardGameSettingsConfig>(section);
section.Bind(config);

if (!string.IsNullOrWhiteSpace(config.SentryLink))
{
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = config.SentryLink;
        options.EnableTracing = true;
        options.UseOpenTelemetry();
        options.TracesSampleRate = 0.5f;
        options.SampleRate = 0.5f;
        options.AddIntegration(new ProfilingIntegration());
    });
}

ConfigureServices(builder.Services, !builder.Environment.IsDevelopment());

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
else
{
    app.UseHttpsRedirection();
    app.UseRewriter(new RewriteOptions()
    .AddRedirectToNonWwwPermanent());
}

app.UseSession();

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//app.MapRazorComponents<App>()
//.AddInteractiveServerRenderMode();
app.MapBlazorHub();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        app.UseCors("api");
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();

void ConfigureServices(IServiceCollection services, bool isProduction)
{
    services.AddAdServer();

    var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(rootDirectory + "/umbraco"))
        .SetApplicationName("CardDatabaseSites");

    services.AddRazorComponents()
        .AddInteractiveServerComponents();

    services.AddOpenTelemetry()
        .WithMetrics(opt =>
        {
            opt.AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation();
        })
        .WithTracing(opt =>
        {
            opt.AddAspNetCoreInstrumentation()
                .AddSentry();
        });

    services.AddSession((option) =>
    {
        option.Cookie.Name = "CardDatabaseSites";
        option.Cookie.HttpOnly = true;
        option.Cookie.SameSite = isProduction ? SameSiteMode.Lax : SameSiteMode.None;
        option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    services
        .Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = false;
        });

    services.AddCors(builder =>
    {
        builder.AddPolicy("api", cors =>
        {
            cors
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .Build();
        });

        builder.AddPolicy("api-login", cors =>
        {
            cors
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithOrigins("https://aidalon.local:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build();
        });
    });
}