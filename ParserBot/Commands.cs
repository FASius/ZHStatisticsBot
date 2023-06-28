using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ParserBot
{

    public class Commands : BaseCommandModule
    {
        private readonly PlayersStorage playersStorage = new FilesPlayersStorage();
        private readonly StatisticsStorage statistics = new FilesStatisticsStorage();

        [Command("reg")]
        [Description("Registration for data gaining"), RequireRoles(RoleCheckMode.None, "Registered")]
        public async Task Reg(CommandContext ctx, [Description("GentoolID and PC Info")] params string[] args)
        {
            if (args.Length != 2)
            {
                await ctx.RespondAsync("Wrong arguments!");
                return;
            }
            ulong userId = ctx.User.Id;
            var player = playersStorage.GetPlayer(userId);
            if (player == null)
            {
                player = new Player(args[0], args[1], userId);
                playersStorage.AddPlayerToStorage(player);
            }

            await ctx.RespondAsync($"Registration success!");
        }


        [Command("day"), RequireRoles(RoleCheckMode.Any, "Registered"), Description("Get stat about player for specific day")]
        public async Task Day(CommandContext ctx, DiscordMember member = null, params string[] args)
        {
            if (args.Length != 4 && args.Length != 1)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;day @player dd mm yyyy stat_type  or ;;day @player stat_type");
                return;
            }

            DateTime date = DateTime.UtcNow.AddDays(-1).AddHours(-1);
            string matchType = args[0];
            if (args.Length == 4) // ;;day fas 09 10 2021{
            {
                date = DateUtils.fromDate(args[2], args[1], args[0]);
                matchType = args[3];
            }

            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            var userId = ctx.User.Id;
            if (member != null)
            {
                userId = member.Id;
            }
            var player = playersStorage.GetPlayer(userId);

            var stats = statistics.getStats(year, month, day, player);
            var formatter = new PlayerStatsFormatter(stats);
            var msg = formatter.formatByGameMode(matchType);

            if (msg == null)
            {
                await ctx.RespondAsync("The member did not play that day");
                return;
            }

            await ctx.RespondAsync(msg);
        }

        [Command("month"), RequireRoles(RoleCheckMode.Any, "Registered")]
        public async Task Month(CommandContext ctx, DiscordMember member, params string[] args)
        {
            if (args.Length != 3)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;month @player mm yyyy stat_type");
                return;
            }

            var year = int.Parse(args[1]);
            var month = int.Parse(args[0]);
            var matchType = args[2];
            var userId = ctx.User.Id;
            if (member != null)
            {
                userId = member.Id;
            }
            var player = playersStorage.GetPlayer(userId);

            var stats = statistics.getStats(year, month, player);
            var formatter = new PlayerStatsFormatter(stats);
            var msg = formatter.formatByGameMode(matchType);

            if (msg == null)
            {
                await ctx.RespondAsync("The member did not play that month");
                return;
            }

            await ctx.RespondAsync(msg);
        }
    }
}
