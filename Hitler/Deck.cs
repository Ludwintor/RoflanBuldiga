using DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DiscordBot.Hitler
{
    public class Deck
    {
        private const int FASCIST_CARDS = 11;
        private const int LIBERAL_CARDS = 6;

        public int Count => cards.Count;
        public List<Card> Cards => cards;

        private List<Card> cards;

        public Deck(bool empty)
        {
            cards = new List<Card>(FASCIST_CARDS + LIBERAL_CARDS);
            if (empty) return;

            for (int i = 0; i < FASCIST_CARDS; i++)
                cards.Add(new Card(GameRole.Fascist));

            for (int i = 0; i < LIBERAL_CARDS; i++)
                cards.Add(new Card(GameRole.Liberal));
        }

        public void Shuffle()
        {
            Random rng = Game.rng;

            cards.Shuffle(rng);
        }

        public void ShiftDeck(Deck other)
        {
            cards.Concat(other.cards);
            Shuffle();
        }

        public Card TakeCardFromTop()
        {
            int index = cards.Count - 1;
            Card card = cards[index];
            cards.RemoveAt(index);

            return card;
        }

        public IEnumerable<Card> TakeCards(int count)
        {
            while (count > 0)
                yield return TakeCardFromTop();
        }

        public Card PeekCardFromTop()
        {
            int index = cards.Count - 1;

            return cards[index];
        }

        public IEnumerable<Card> PeekCards(int count)
        {
            while (count > 0)
                yield return PeekCardFromTop();
        }
    }

    public class Card
    {
        public GameRole color;

        public Card(GameRole color)
        {
            this.color = color;
        }
    }
}
