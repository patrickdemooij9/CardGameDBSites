using Discord.Commands;

namespace SkytearHorde.Business.Discord.Modules
{
    public class CardModule : ModuleBase<SocketCommandContext>
    {
        [Command("card")]
        [Summary("Displays a card.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        => ReplyAsync(echo);
    }
}
