using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GoFish
{
    public class Deck : ObservableCollection<Card>
    {
        private static readonly System.Random random = Player.random;

        public Deck()
        {
            Reset();
        }

        public Card Deal(int index)
        {
            Card cardToDeal = base[index];
            RemoveAt(index);
            cardToDeal.cardObject.name = cardToDeal.Name;
            return cardToDeal;
        }

        public void Reset()
        {
            Clear();
            for (int suit = 0; suit <= 3; suit++)
                for (int value = 1; value <= 13; value++)
                    Add(new Card((Values)value, (Suits)suit));
        }

        public Deck Shuffle()
        {
            List<Card> copy = new List<Card>(this);
            Clear();

            Vector2 newPosition;
            float deckPosition = 0f;
            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            while (copy.Count > 0)
            {
                int index = random.Next(copy.Count);
                Card card = copy[index];
                copy.RemoveAt(index);
                Add(card);

                newPosition = new Vector2(deckPosition, deckPosition++);
                card.cardObject.transform.position = newPosition;
                card.cardObject.gameObject.transform.SetParent(gameManager.DeckArea.transform.GetChild(0), false);
            }
            return this;
        }

        public void Sort()
        {
            List<Card> sortedCards = new List<Card>(this);
            sortedCards.Sort(new CardComparerByValue());
            Clear();
            foreach (Card card in sortedCards)
            {
                Add(card);
            }
        }
    }
}
