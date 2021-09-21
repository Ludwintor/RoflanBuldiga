using DSharpPlus.Entities;

namespace DiscordBot.Hitler
{
    public class Player
    {
        public readonly DiscordMember member;
        public GameRole gameRole;
        public bool isHitler;

        public Player(DiscordMember member)
        {
            this.member = member;
            Reset();
        }

        public void Reset()
        {
            gameRole = GameRole.Liberal;
            isHitler = false;
        }
    }
}
