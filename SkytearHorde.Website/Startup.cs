using AdServer.Extensions;
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

namespace SkytearHorde
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web hosting environment.</param>
        /// <param name="config">The configuration.</param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337.
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddComposers()
                .AddNotificationHandler<AfterMetaTagsNotification, DeckMetaTagsEventHandler>()
                .AddNotificationHandler<ContentPublishedNotification, CardOverviewCacheClearer>()
                .AddNotificationHandler<ContentPublishedNotification, CardSortingEventHandler>()
                .Build();

            services.AddAdServer();

            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddOpenTelemetry()
                .WithMetrics(opt =>
            {
                opt.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter();
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

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseRewriter(new RewriteOptions()
                .AddRedirectToNonWwwPermanent());

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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

                    u.EndpointRouteBuilder.MapBlazorHub();
                });
        }
    }
}
