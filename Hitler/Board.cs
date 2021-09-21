using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Hitler
{
    public class Board
    {
        private const int FASCIST_WIN = 6;
        private const int LIBERAL_WIN = 5;
        private const int ELECTION_MAX = 3;
        private static readonly string[] markers = new string[10] { "white", "black", "red", "blue", "brown", "purple", "green", "yellow", "orange", "radio_button" };

        public Player currentPresident;
        public Player currentChancellor;
        public Player previousPresident;
        public Player previousChancellor;
        public DiscordMessage boardMsg;

        private Game game;

        private int liberalPolicy;
        private int fascistPolicy;
        private int electionFail;
        private Deck drawDeck;
        private Deck discardDeck;

        public Board(DiscordMessage boardMsg, Game game)
        {
            this.boardMsg = boardMsg;
            this.game = game;
            liberalPolicy = 0;
            fascistPolicy = 0;
            electionFail = 0;
            drawDeck = new Deck(false);
            discardDeck = new Deck(true);
        }

        public async Task SetupAsync()
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(boardMsg.Embeds[0]);
            embed.WithColor(DiscordColor.Green);
            embed.WithDescription("");
            embed.AddField(game.GetString("hitler-liberals"), GetTrackerString(liberalPolicy, LIBERAL_WIN, GameRole.Liberal), true); // LIBERALS' TRACKER Index 0
            embed.AddField(game.GetString("hitler-fascists"), GetTrackerString(fascistPolicy, FASCIST_WIN, GameRole.Fascist), true); // FASCISTS' TRACKER Index 1
            embed.AddField(game.GetString("hitler-electiontracker"), GetElectionString(), true); // ELECTION TRACKER Index 2
            embed.AddField(game.GetString("hitler-players"), GetPlayerOrder(), true); // PLAYERS ORDER Index 3
            embed.AddField(game.GetString("hitler-decks"), GetDecksString(), true); // DECKS' STATUS Index 4

            embed.Description += electionFail == ELECTION_MAX - 1 ? Formatter.Bold(game.GetString("hitler-electionwarning")) : "";

            await boardMsg.ModifyAsync(embed.Build());
        }

        public async Task SetupTestAsync()
        {
            liberalPolicy = Game.rng.Next(LIBERAL_WIN + 1);
            fascistPolicy = Game.rng.Next(FASCIST_WIN + 1);
            electionFail = 2;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(boardMsg.Embeds[0]);
            embed.WithColor(DiscordColor.Green);
            embed.WithDescription("");
            embed.AddField(game.GetString("hitler-liberals"), GetTrackerString(liberalPolicy, LIBERAL_WIN, GameRole.Liberal), true); // LIBERALS' TRACKER Index 0
            embed.AddField(game.GetString("hitler-fascists"), GetTrackerString(fascistPolicy, FASCIST_WIN, GameRole.Fascist), true); // FASCISTS' TRACKER Index 1
            embed.AddField(game.GetString("hitler-electiontracker"), GetElectionString(), true); // ELECTION TRACKER Index 2
            embed.AddField(game.GetString("hitler-players"), GetPlayerOrderTest(), true); // PLAYERS ORDER Index 3
            embed.AddField(game.GetString("hitler-decks"), GetDecksString(), true); // DECKS' STATUS Index 4

            embed.Description += electionFail == ELECTION_MAX - 1 ? Formatter.Bold(game.GetString("hitler-electionwarning")) : ""; // TODO: Replace with method


            await boardMsg.ModifyAsync(embed.Build());
        }

        public async Task Update(string descriptionMessage)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(boardMsg.Embeds[0]);
            embed.Description = CreateDescription(descriptionMessage);
            IReadOnlyList<DiscordEmbedField> fields = embed.Fields;
            fields[0].Value = GetTrackerString(liberalPolicy, LIBERAL_WIN, GameRole.Liberal);
            fields[1].Value = GetTrackerString(fascistPolicy, FASCIST_WIN, GameRole.Fascist);
            fields[2].Value = GetElectionString();
            fields[3].Value = GetPlayerOrder();
            fields[4].Value = GetDecksString();


            await boardMsg.ModifyAsync(embed.Build());
        }

        public string GetMarker(int playerIndex) => playerIndex != 9 ? $":{markers[playerIndex]}_circle:" : $":{markers[playerIndex]}:";

        private string CreateDescription(string descriptionMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(descriptionMessage);
            if (electionFail == ELECTION_MAX - 1)
                sb.AppendLine(Formatter.Bold(game.GetString("hitler-electionwarning")));

            return sb.ToString();
        }

        private string GetTrackerString(int current, int max, GameRole role)
        {
            string fullfilEmoji = role == GameRole.Fascist ? ":red_square: " : ":blue_square: "; // BLUE = LIBERAL   RED = FASCIST

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < current; i++)
                sb.Append(fullfilEmoji);

            for (int i = 0; i < max - current; i++)
                sb.Append(":white_large_square: ");

            return sb.ToString();
        }

        private string GetPlayerOrder()
        {
            StringBuilder sb = new StringBuilder();
            List<Player> players = game.Players;

            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                sb.Append(GetMarker(i));

                string name = player.member.DisplayName;
                string playerLabel = Formatter.InlineCode(name.PadRight(32));
                if (player == currentPresident)
                    playerLabel += ":crown:"; // PRESIDENT
                else if (player == currentChancellor)
                    playerLabel += ":necktie:"; // CHANCELLOR
                else if (player == previousPresident)
                    playerLabel += ":crown::leftwards_arrow_with_hook:"; // PREV PRESIDENT
                else if (player == previousChancellor)
                    playerLabel += ":necktie::leftwards_arrow_with_hook:"; // PREV CHANCELLOR

                sb.AppendLine(playerLabel);
            }

            return sb.ToString();
        }

        private string GetElectionString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ELECTION_MAX; i++)
            {
                if (i != 0)
                    sb.Append(" :arrow_right: ");

                if (i == electionFail)
                    sb.Append(":bangbang:");
                else
                    sb.Append(":o:");
            }

            return sb.ToString();
        }

        private string GetDecksString() // :wastebasket: :flower_playing_cards:
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(":flower_playing_cards: ");
            sb.Append(Formatter.Bold(drawDeck.Count.ToString()));
            sb.Append("\u3000\u3000\u3000\u3000\u3000\u3000\u3000"); // EXTRA THICC SPACE \u3000
            sb.Append(Formatter.Bold(discardDeck.Count.ToString()));
            sb.Append(" :wastebasket:");


            return sb.ToString();
        }

        private string GetPlayerOrderTest()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 1; i <= 10; i++)
            {
                if (i - 1 != 9)
                    sb.Append($":{markers[i - 1]}_circle:");
                else
                    sb.Append($":{markers[i - 1]}:");
                string playerName = $"Player {i}";
                string playerLabel = Formatter.InlineCode(playerName.PadRight(32));
                if (i == 2)
                    playerLabel += ":crown:"; // PRESIDENT
                else if (i == 6)
                    playerLabel += ":necktie:"; // CHANCELLOR
                else if (i == 1)
                    playerLabel += ":crown::leftwards_arrow_with_hook:"; // PREV PRESIDENT
                else if (i == 8)
                    playerLabel += ":necktie::leftwards_arrow_with_hook:"; // PREV CHANCELLOR

                sb.AppendLine(playerLabel);
            }

            return sb.ToString();
        }
    }
}
