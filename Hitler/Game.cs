using DiscordBot.Handlers;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Hitler
{
    public class Game
    {
        public static readonly Random rng = new Random();
        private static Dictionary<ulong, Game> games = new Dictionary<ulong, Game>();

        public static Game Get(ulong guildId)
        {
            if (!games.ContainsKey(guildId))
                games.Add(guildId, new Game(Settings.Get(guildId)));

            return games[guildId];
        }

        public Settings Settings => settings;
        public Board Board => board;
        public List<Player> Players => players.Values.ToList();
        public int PlayerCount => players.Count;

        public DiscordChannel gameChannel;
        public DiscordChannel voiceChannel;
        public InteractivityExtension interactivity;
        public bool isReady = false;
        public bool isStarted = false;

        private Settings settings;
        private Dictionary<ulong, Player> players;
        private Board board;
        private GameState state;

        public Game(Settings settings)
        {
            players = new Dictionary<ulong, Player>();
            this.settings = settings;
        }

        public bool Join(DiscordMember member)
        {
            if (players.ContainsKey(member.Id))
                return false;
            Player newPlayer = new Player(member);
            players.Add(member.Id, newPlayer);

            return true;
        }

        public bool Leave(DiscordMember member)
        {
            if (!players.ContainsKey(member.Id))
                return false;

            players.Remove(member.Id);

            return true;
        }

        public bool Setup()
        {
            List<Player> players = Players;

            if (players.Count < 5)
                return false;
            if (players.Count > 10)
                return false;

            // Reset players
            foreach (Player player in players)
                player.Reset();


            int fascistCount = (players.Count - 1) / 2; // 5-6 2 fas. 7-8 3 fas. 9-10 4 fas (1 fascist is Hitler)
            List<Player> liberals = new List<Player>(players);
            // Random fascists
            for (int i = 0; i < fascistCount - 1; i++)
            {
                int fascistIndex = rng.Next(liberals.Count);
                liberals[fascistIndex].gameRole = GameRole.Fascist;
                liberals.RemoveAt(fascistIndex);
            }
            // Random Hitler
            int hitlerIndex = rng.Next(liberals.Count);
            liberals[hitlerIndex].gameRole = GameRole.Fascist;
            liberals[hitlerIndex].isHitler = true;

            isReady = true;
            return true;
        }

        public async Task<bool> StartAsync(DiscordChannel gameChannel, DiscordChannel voiceChannel) // TODO: Shuffle players before start?
        {
            this.gameChannel = gameChannel;
            this.voiceChannel = voiceChannel;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = "GAME",
                Color = DiscordColor.Orange,
                Description = GetString("hitler-embedtablecreating")
            };
            DiscordMessage boardMsg = await gameChannel.SendMessageAsync(embed.Build());
            board = new Board(boardMsg, this);
            await board.SetupAsync();

            isStarted = true;
            return true;
        }

        public async Task<bool> StartTestAsync(DiscordChannel gameChannel, DiscordChannel voiceChannel, InteractivityExtension interactivity) // TODO: Shuffle players before start?
        {
            this.gameChannel = gameChannel;
            this.voiceChannel = voiceChannel;
            this.interactivity = interactivity;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = "GAME",
                Color = DiscordColor.Orange,
                Description = GetString("hitler-embedtablecreating")
            };
            DiscordMessage boardMsg = await gameChannel.SendMessageAsync(embed.Build());
            board = new Board(boardMsg, this);
            await board.SetupTestAsync();

            isStarted = true;
            GameCycle();
            return true;
        }

        private void GameCycle()
        {
            Task.Run(async () =>
            {
                List<Player> players = Players;
                board.currentPresident = players[rng.Next(players.Count)];
                state = GameState.ChancellorSelecting;
                while (isStarted)
                {
                    switch (state)
                    {
                        case GameState.ChancellorSelecting:
                            await ChancellorSelecting_State();
                            break;
                        case GameState.Voting:
                            await Voting_State();
                            break;
                        case GameState.PresidentDraw:
                            await PresidentDraw_State();
                            break;
                        case GameState.ChancellorDraw:
                            await ChancellorDraw_State();
                            break;
                    }
                }
            });
        }

        private async Task ChancellorSelecting_State()
        {
            await board.Update(GetStringFormat("hitler-selectchancellor", board.currentPresident.member.Mention));
            List<int> busyPlayerIndexes = new List<int>();
            List<Player> players = Players;
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                if (player == board.currentPresident || player == board.previousPresident || player == board.previousChancellor)
                    busyPlayerIndexes.Add(i);
            }
        }

        private async Task Voting_State()
        {

        }

        private async Task PresidentDraw_State()
        {

        }

        private async Task ChancellorDraw_State()
        {

        }

        public StatusResponse CanStart(CommandContext ctx)
        {
            string errorId = "";
            bool success = false;
            if (!isReady && !isStarted)
                errorId = "hitler-startreadyerror";
            else if (isStarted)
                errorId = "hitler-startstartederror";
            else if (ctx.Member.VoiceState.Channel is null)
                errorId = "hitler-startvoiceerror";
            else
                success = true;

            return new StatusResponse(success, errorId);
        }

        public Player GetPlayer(ulong id) => players[id];
        public string GetString(string stringId) => settings.GetLocalString(stringId);
        public string GetStringFormat(string stringId, params object[] args) => settings.GetLocalStringFormat(stringId, args);

        public bool TestSetup()
        {
            players.Clear();

            int playerCount = rng.Next(5, 11);
            for (int i = 0; i < playerCount; i++)
            {
                players.Add((ulong)i, new Player(null)); 
            }

            return Setup();
        }
    }

    public enum GameState
    {
        ChancellorSelecting,
        Voting,
        PresidentDraw,
        ChancellorDraw,
    }
}
