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

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddNotificationHandler<AfterMetaTagsNotification, DeckMetaTagsEventHandler>()
    .AddNotificationHandler<ContentPublishedNotification, CardOverviewCacheClearer>()
    .AddNotificationHandler<ContentPublishedNotification, CardSetConnectEventHandler>()
    //.AddNotificationHandler<ContentPublishedNotification, CardSortingEventHandler>()
    .AddSlimsy()
    .Build();

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

ConfigureServices(builder.Services);

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseRewriter(new RewriteOptions()
    .AddRedirectToNonWwwPermanent());

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

app.Map("/metrics", metricsApp =>
{
    metricsApp.UseMiddleware<MetricsAuthMiddleware>();

    // We already specified URL prefix above, no need to specify it twice here.
    metricsApp.UseMetricServer("");
});

//app.MapRazorComponents<App>()
    //.AddInteractiveServerRenderMode();
app.MapBlazorHub();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
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

void ConfigureServices(IServiceCollection services)
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
    });

    services
        .Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = false;
        });
}