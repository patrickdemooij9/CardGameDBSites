using AdServer.Middleware;
using AdServer.Repositories.AdRepository;
using AdServer.Repositories.CampaignRepository;
using AdServer.Repositories.MetricDataRepository;
using AdServer.Repositories.MetricRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SkytearHorde.Business.BackgroundRunners;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.ContentFinders;
using SkytearHorde.Business.CustomCardMaker;
using SkytearHorde.Business.DataSources.Overview;
using SkytearHorde.Business.Discord;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Processors;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Repositories.AdServer;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Business.Startup.Migrations;
using SkytearHorde.Entities.Interfaces;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace SkytearHorde.Business.Startup
{
    public class MainComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<CardService>();
            builder.Services.AddSingleton<CardPageService>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<DeckService>();
            builder.Services.AddSingleton<ICardSearchService, CardSearchService>();
            builder.Services.AddSingleton<DeckListService>();
            builder.Services.AddSingleton<ContentCreatorService>();
            builder.Services.AddSingleton<DeckTrackingProcessor>();
            builder.Services.AddSingleton<SkillSetService>();
            builder.Services.AddSingleton<IAbilityFormatter, AbilityFormatter>();
            builder.Services.AddScoped<MemberInfoService>();
            builder.Services.AddSingleton<CommentService>();
            builder.Services.AddScoped<CollectionService>();
            builder.Services.AddSingleton<CardPriceService>();
            builder.Services.AddScoped<RandomizeService>();

            builder.Services.AddSingleton<DeckRepository>();
            builder.Services.AddSingleton<DeckViewRepository>();
            builder.Services.AddSingleton<DeckListRepository>();
            builder.Services.AddSingleton<ContentCreatorBlogPostRepository>();
            builder.Services.AddSingleton<DeckLikeRepository>();
            builder.Services.AddSingleton<DeckCalculateScoreRepository>();
            builder.Services.AddSingleton<DeckCommentRepository>();
            builder.Services.AddSingleton<CardCommentRepository>();
            builder.Services.AddSingleton<CollectionSetRepository>();
            builder.Services.AddSingleton<CollectionCardRepository>();
            builder.Services.AddSingleton<CardPriceRepository>();
            builder.Services.AddSingleton<CardRepository>();
            builder.Services.AddSingleton<CollectionPackRepository>();
            builder.Services.AddSingleton<RedditDailyCardRepository>();

            builder.Services.AddSingleton<IAdRepository, AdRepository>();
            builder.Services.AddSingleton<IMetricRawDataRepository, MetricRawDataRepository>();
            builder.Services.AddSingleton<IMetricRepository, MetricRepository>();
            builder.Services.AddSingleton<ICampaignRepository, CampaignRepository>();

            builder.Services.AddScoped<CardMaker>();

            builder.Services.AddSingleton<ISiteAccessor, SiteAccessor>();
            builder.Services.AddSingleton<ISiteService, SiteService>();

            builder.Services.AddSingleton<ViewSessionGenerator>();
            builder.Services.AddSingleton<ViewTrackingProcessor>();

            builder.Services.AddScoped<ViewRenderHelper>();

            builder.Services
                .AddScoped<IOverviewDataSource, CardOverviewDataSource>()
                .AddScoped<IOverviewDataSource, CollectionSetDetailDataSource>()
                .AddScoped<IOverviewDataSource, DeckOverviewDataSource>();

            builder.ContentFinders().Append<CardPageContentFinder>();
            builder.ContentFinders().Append<DeckContentFinder>();
            builder.ContentFinders().Append<ListContentFinder>();
            builder.ContentFinders().Append<SetContentFinder>();

            builder.Components().Append<MigrationComponent>();
            builder.Components().Append<DiscordBotComponent>();
            builder.Components().Append<ExamineSiteIdComponent>();
            builder.Components().Append<ExamineCardValuesComponent>();
            //builder.Components().Append<ViewSessionComponent>();

            builder.Services.AddHostedService<DeckTrackingSyncTask>();
            //builder.Services.AddHostedService<PageViewSyncTask>();
            builder.Services.AddHostedService<CreatorSyncTask>();
            builder.Services.AddHostedService<DeckCalculatorTask>();
            builder.Services.AddHostedService<AdReportTask>();
            builder.Services.AddHostedService<CardPriceSyncTask>();
            builder.Services.AddHostedService<RedditDailyCardTask>();

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "DeckTracker",
                    applicationBuilder =>
                    {
                        applicationBuilder.UseMiddleware<SiteSetterMiddleware>();
                        applicationBuilder.UseMiddleware<AdRedirectMiddleware>();
                        applicationBuilder.UseMiddleware<CountryMiddleware>();
                        applicationBuilder.UseMiddleware<TrackDeckMiddleware>();
                        //applicationBuilder.UseMiddleware<TrackViewMiddleware>();
                        applicationBuilder.UseMiddleware<FaviconMiddleware>();
                    },
                    applicationBuilder =>
                    {
                    },
                    applicationBuilder => { }
                ));
            });
        }
    }
}
