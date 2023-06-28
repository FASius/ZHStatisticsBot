using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;

namespace ParserBot
{
    public class Program
    {
        private const int MIN_GAME_LENGTH_SECONDS = 150;
        public readonly EventId BotEventId = new EventId(42, "Bot-Ex02");

        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }

        public static void Main()
        {
            Thread Update = new Thread(new ThreadStart(UpdateDB));
            Update.Start();
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        private static void UpdateDB()
        {
            GentoolParser gentoolParser = new GentoolParser(new WebClient());
            PlayersStorage playersStorage = new FilesPlayersStorage();
            StatisticsStorage statisticsStorage = new FilesStatisticsStorage();
            ReplayTxtParser txtParser = new ReplayTxtParser(MIN_GAME_LENGTH_SECONDS);
            while (true)
            {
                if (DateTime.UtcNow.Hour == 0)
                {
                    var date = DateTime.UtcNow;
                    var players = playersStorage.GetPlayers();
                    var playersFiles = gentoolParser.ParseDay(DateTime.Now.AddDays(-1), players.ToArray());
                    var playersStats = new Dictionary<Player, PlayerStats>();
                    foreach (var playerFilePair in playersFiles)
                    {
                        var stat = txtParser.Parse(playerFilePair.Value);
                        if (!playersStats.ContainsKey(playerFilePair.Key))
                        {
                            playersStats[playerFilePair.Key] = new PlayerStats(stat);
                        }
                        else
                        {
                            playersStats[playerFilePair.Key] += stat;
                        }
                    }
                    foreach (var playerStatPair in playersStats)
                    {
                        statisticsStorage.saveStats(date.Year, date.Month, date.Day, playerStatPair.Key, playerStatPair.Value);
                    }
                    

                }
                Thread.Sleep(1000 * 60 * 59);
            }
        }

        public async Task RunBotAsync()
        {
            // first, let's load our configuration file
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            // next, let's load the values from that file
            // to our client's configuration
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };

            // then we want to instantiate our client
            this.Client = new DiscordClient(cfg);

            // next, let's hook some events, so we know
            // what's going on
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;

            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] { cfgjson.CommandPrefix },

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            this.Commands = this.Client.UseCommandsNext(ccfg);

            // let's hook some command events, so we know what's 
            // going on
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            // let's add a converter for a custom type and a name
            //var mathopcvt = new MathOperationConverter();
            //Commands.RegisterConverter(mathopcvt);
            //Commands.RegisterUserFriendlyTypeName<MathOperation>("operation");

            // up next, let's register our commands

            this.Commands.RegisterCommands<Commands>();
            //this.Commands.RegisterCommands<ExampleExecutableGroup>();

            // set up our custom help formatter
            this.Commands.SetHelpFormatter<SimpleHelpFormatter>();

            // finally, let's connect and log in
            await this.Client.ConnectAsync();

            // when the bot is running, try doing <prefix>help
            // to see the list of registered commands, and 
            // <prefix>help <command> to see help about specific
            // command.

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed);
            }
        }
    }

    // this structure will hold data from config.json
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }
}