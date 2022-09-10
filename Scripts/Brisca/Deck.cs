using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Brisca
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

        public Card DealTriumphCard()
        {
            int lastCardIndex = this.Count() - 1;

            Card triumphCard = this[lastCardIndex];
            triumphCard.cardObject.name = triumphCard.Name;

            this[lastCardIndex] = this[0];
            this[0] = triumphCard;

            return triumphCard;            
        }

        public void SwapTriumphCard(Card newTriumphCard) => this[0] = newTriumphCard;

        public void Reset()
        {
            Clear();
            for (int suit = 0; suit <= 3; suit++)
                for (int value = 2; value <= 14; value++)
                    if (value != 3) Add(new Card((Values)value, (Suits)suit));
        }

        public Deck Shuffle(Transform deckArea, GameObject cardObject)
        {
            List<Card> copy = new List<Card>(this);
            Clear();

            float deckPosition = 0f;
            while (copy.Count > 0)
            {
                int index = random.Next(copy.Count);
                Card card = copy[index];

                copy.RemoveAt(index);
                Add(card);

                card.cardObject = GameObject.Instantiate(cardObject, new Vector2(deckPosition, deckPosition), Quaternion.identity);
                deckPosition += 0.5f;

                if(GameController.instance.BackOfCardSprite != null)
                    cardObject.gameObject.GetComponent<Image>().sprite = GameController.instance.BackOfCardSprite;

                card.cardObject.gameObject.transform.SetParent(deckArea, false);
            }
            return this;
        }
    }
}
