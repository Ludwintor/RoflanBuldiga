using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using DiscordBot.Handlers;
using DiscordBot.Commands;
using DiscordBot.Converters;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using System.Linq;
using DiscordBot.Services;
using DiscordBot.Music;
using DSharpPlus.Entities;
using DiscordBot.Helpers;

namespace DiscordBot
{
    public class Startup
    {
        private Config Config { get; }

        private DiscordClient Client { get; set; }
        private CommandsNextExtension Commands { get; set; }
        private IServiceProvider Provider { get; set; }

        public Startup(string[] args)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            string json = File.ReadAllText(path);
            Config = JsonConvert.DeserializeObject<Config>(json);
        }

        public static async Task RunAsync(string[] args)
        {
            Startup startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            // Initialize client
            DiscordConfiguration discordConfig = new DiscordConfiguration()
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
            Client = new DiscordClient(discordConfig);

            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            ConfigureServices(new ServiceCollection());

            InitCommandModule();

            InitInteractivityModule();

            Client.UseLavalink();

            new LogHandler(Client, Commands);

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Random>();
            services.AddSingleton<MusicService>();
            services.AddSingleton(new LavalinkService(Client));


            Provider = services.BuildServiceProvider();
        }

        private void InitCommandModule()
        {
            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = Config.CommandPrefixes,
                Services = Provider
            };
            Commands = Client.UseCommandsNext(commandsConfig);
            RegisterCommands();
        }

        private void InitInteractivityModule()
        {
            InteractivityConfiguration interactivityConfig = new InteractivityConfiguration()
            {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.MaxValue
            };
            Client.UseInteractivity(interactivityConfig);
        }

        private void RegisterCommands()
        {
            Commands.SetHelpFormatter<HelpFormatter>();

#if DEBUG
            Commands.RegisterCommands<TestModule>();
#endif
            Commands.RegisterCommands<MusicModule>();
            //commands.RegisterCommands<GameModule>();
            Commands.RegisterCommands<AdminModule>();

            Commands.RegisterConverter(new LanguageConverter());
            Commands.RegisterConverter(new RepeatModeConverter());
            Commands.RegisterConverter(new EqualizerModeConverter());
        }

        private async Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            MusicService music = Provider.GetService<MusicService>();
            GuildMusicData data = music.GetOrCreate(e.Guild);

            // Channel deleted or someone disconnected bot from channel
            if (e.After.Channel == null && e.User == Client.CurrentUser)
            {
                await data.StopAsync();
                await data.DestroyPlayerAsync();
                return;
            }

            if (e.User == Client.CurrentUser)
                return;

            // Bot doesn't connected or another channel used
            if (data.Channel == null || data.Channel != e.Before?.Channel)
                return;

            // Bot playing and no humans in channel
            if (data.IsPlaying && !data.Channel.Users.Any(user => !user.IsBot))
            {
                await data.PauseAsync();
                data.Save();

                if (data.CommandChannel != null)
                {
                    DiscordEmbed embed = EmbedHelper.CreateBasicEmbed("Player Status", "All users left channel, playback paused.", DiscordColor.Orange, e.Guild.CurrentMember).Build();

                    await data.CommandChannel.SendMessageAsync(embed);
                }
            }
        }
    }
}
