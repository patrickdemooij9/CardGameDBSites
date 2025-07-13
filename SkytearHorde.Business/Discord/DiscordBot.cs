using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using System.Reflection;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Discord
{
    public class DiscordBot
    {
        private readonly CommandService _commandService;
        private readonly ILogger _logger;
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly SettingsService _settingsService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IAbilityFormatter _abilityFormatter;
        private readonly ISiteService _siteService;
        private string _baseUrl;
        private readonly int _siteId;

        private DiscordSocketClient _client;

        public DiscordBot(ILogger logger, CardService cardService, CardPageService cardPageService, IUmbracoContextFactory umbracoContextFactory, SettingsService settingsService, ISiteAccessor siteAccessor, IAbilityFormatter abilityFormatter, ISiteService siteService, int siteId)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
            _commandService = new CommandService();
            _logger = logger;
            _cardService = cardService;
            _cardPageService = cardPageService;
            _umbracoContextFactory = umbracoContextFactory;
            _settingsService = settingsService;
            _siteAccessor = siteAccessor;
            _abilityFormatter = abilityFormatter;
            _siteService = siteService;
            _siteId = siteId;

            ExecutionContext.SuppressFlow();
            Task.Run(Startup);
            ExecutionContext.RestoreFlow();
        }

        public async Task Startup()
        {
            await InstallCommandsAsync();

            _client.Log += _client_Log;

            _siteAccessor.SetSiteId(_siteId);

            var settings = _settingsService.GetDiscordSettings();
            var token = settings?.Token;
            _baseUrl = settings?.BaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Could not find token for Discord bot");
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task _client_Log(LogMessage arg)
        {
            if (arg.Exception != null)
            {
                _logger.LogError(arg.Message, arg.Exception);
            }
            else
            {
                _logger.LogInformation(arg.Message);
            }
        }

        private async Task InstallCommandsAsync()
        {
            _client.MessageReceived += OnClientMessageReceived;

            await _commandService.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), services: null);
        }

        private async Task OnClientMessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            if (message.Author.IsBot) return;

            try
            {
                var matches = Regex.Matches(message.Content, "(?<=\\{{)(.*?)(?=\\}})");

                List<Card> cards = new List<Card>();
                foreach (Match match in matches)
                {
                    if (cards.Count >= 3) continue;
                    cards.AddRange(_cardService.Search(new CardSearchQuery(1, _siteId) { Query = match.Value }, out _));
                }

                if (cards.Count == 0) return;

                await messageParam.Channel.SendMessageAsync(embeds: cards.Select(CreateCardEmbed).ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
            }
        }

        private Embed CreateCardEmbed(Card card)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle(card.DisplayName);

            var attributes = _siteService.GetAllAttributes().ToDictionary(it => it.Name, it => it);
            foreach (var ability in card.Attributes)
            {
                if (!attributes.TryGetValue(ability.Key, out var attribute))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(attribute?.DisplayName) || attribute.HideFromDiscord) continue;

                foreach (var field in ability.Value.GetDiscordField())
                {
                    embedBuilder.AddField(_abilityFormatter.TranslateSpecialCharsToMarkdown(field.Name).ToString(), _abilityFormatter.TranslateSpecialCharsToMarkdown(field.Value), field.Inline);
                }
            }

            embedBuilder.WithUrl($"{_baseUrl}{_cardPageService.GetUrl(card)}");
            if (card.Questions?.Length > 0)
            {
                embedBuilder.AddField("FAQ", $"There are {card.Questions.Length} FAQ for this card");
            }
            if (card.Image != null)
            {
                embedBuilder.WithThumbnailUrl($"{_baseUrl}{card.Image.Url()}");
            }
            if (!string.IsNullOrWhiteSpace(card.EmbedFooterText))
            {
                embedBuilder.WithFooter(card.EmbedFooterText);
            }
            else
            {
                var settings = _settingsService.GetDiscordSettings();
                if (!string.IsNullOrWhiteSpace(settings.FooterText))
                {
                    embedBuilder.WithFooter(settings.FooterText);
                }
            }
            return embedBuilder.Build();
        }
    }
}
