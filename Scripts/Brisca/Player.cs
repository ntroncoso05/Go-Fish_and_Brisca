using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Brisca
{
    public class Player
    {
        public static System.Random random = new System.Random();
        public GameObject PlayerObject;
        public readonly bool IsHuman;

        private List<Card> hand = new List<Card>();
        private List<int> totalPoints = new List<int>();

        /// <summary>
        /// The cards in the player's hand
        /// </summary>
        public IEnumerable<Card> Hand => hand;

        /// <summary>
        /// The books that the player has pulled out
        /// </summary>
        public IEnumerable<int> CardPoints => totalPoints;

        public readonly string Name;

        /// <summary>
        /// Returns the current status of the player: the number of cards and books
        /// </summary>        
        public string Status => $"{Name}\n#Cards: {totalPoints.Count()}\nScore: {totalPoints.Sum(c => c)}";


        /// <summary>
        /// Constructor to create a player
        /// </summary>
        /// <param name="name">Player's name</param>
        public Player(string name, bool isHuman)
        {
            Name = name;
            IsHuman = isHuman;
        }

        /// <summary>
        /// Gets one card from the stock
        /// </summary>
        /// </summary>
        /// <param name="card">Card to add to player hand</param>
        public void DrawCard(Card card)
        {
                hand.Add(card);
                card.cardObject.GetComponent<CardAnimation>()
                    .SetAnimation(PlayerObject, 0f, false, (IsHuman) ? true : false);
        }

        public void PlayCard(Card cardToPlay, GameObject dropZone)
        {
            hand.Remove(cardToPlay);
            cardToPlay.cardObject.GetComponent<CardAnimation>()
                .SetAnimation(dropZone, 0f, false, true);
        }

        public void SwapCard(Card cardToGive, Card cardToTake)
        {
            hand.Remove(cardToGive);
            hand.Add(cardToTake);
        }

        public void AddPointsAndCollectCards(List<Card> playedCards)
        {
            float speedDelayer = 0f;
            foreach (Card card in playedCards)
            {
                totalPoints.Add((int)Enum.Parse(typeof(CardPoints), Enum.GetName(typeof(Values), card.Value)));

                card.cardObject.GetComponent<CardAnimation>().SetAnimation(PlayerObject, speedDelayer, true, true);
                speedDelayer += 0.2f;
            }
        }

        /// <summary>
        /// Set the variables used to decided wich card is selected from player's hand
        /// </summary>
        /// <param name="playedCards">The cards played in the current round by others players</param>
        /// <param name="totalPointsPlayed">The total points of all the card played in the current round</param>
        /// <param name="cardPerPoints">Collection keeps track of each player hand by points value</param>
        private void SetDecisionParameters(List<Card> playedCards, out int totalPointsPlayed, out Dictionary<Card, int> cardPerPoints)
        {
            totalPointsPlayed = 0;
            Type cardPoints = typeof(CardPoints);
            Type values = typeof(Values);

            foreach (var card in playedCards)
            {
                totalPointsPlayed += (int)Enum.Parse(cardPoints,
                                                     Enum.GetName(values, card.Value));
            }
            cardPerPoints = new Dictionary<Card, int>();
            foreach (var card in hand)
            {
                cardPerPoints.Add(card, (int)Enum.Parse(enumType: cardPoints,
                                                        value: Enum.GetName(values, card.Value)));
            }
        }

        /// <summary>
        /// Gets a value from the player's hand depending on the accumulated points of the current round
        /// </summary>
        /// <param name="triumphCard">The current triumph card, used to compare suit against player hand</param>
        /// <param name="cardToBeat">The current winner card of the round, used to compare suit against player hand</param>
        /// <param name="playedCards">The cards played in the current round by others players</param>
        /// <returns>The value of a selected card in the player's hand</returns>
        public Card SelectValueFromHand(Card triumphCard, Card cardToBeat, List<Card> playedCards)
        {
            SetDecisionParameters(playedCards, out int totalPointsPlayed, out Dictionary<Card, int> cardPerPoints);

            Suits triumphSuit = triumphCard.Suit;
            Suits cardToBeatSuit = (cardToBeat != null)
                ? cardToBeat.Suit
                : triumphSuit;
            Values cardToBeatValue = (cardToBeat != null)
                ? cardToBeat.Value
                : triumphCard.Value;

            Card cardSelected = (totalPointsPlayed == 0)
                ? GetCardWhenNoValueToGain(cardPerPoints, triumphSuit, cardToBeatSuit, cardToBeatValue)
                : GetCardWhenValueToGain(cardPerPoints, triumphSuit, cardToBeatSuit, cardToBeatValue);

            return cardSelected;
        }

        private Card GetCardWhenValueToGain(Dictionary<Card, int> cardPerPoints, Suits triumphSuit, Suits cardToBeatSuit, Values cardToBeatValue)
        {
            Card cardSelected;
            // totalPointsPlayed greater than 0
            // Able to win
            List<Card> winnerCardsTriumph = cardPerPoints
                .Where(c => c.Key.Suit == cardToBeatSuit)
                .Where(c => c.Key.Value > cardToBeatValue)
                .Select(c => c.Key).ToList();

            if (triumphSuit == cardToBeatSuit)
            {
                if (winnerCardsTriumph.Count > 0)
                {
                    cardSelected = winnerCardsTriumph.First(c => c.Value == winnerCardsTriumph.Min(c => c.Value));
                }
                else
                {
                    List<Card> cardsZeroPoints = cardPerPoints
                        .Where(c => c.Value == 0)
                        .Where(c => c.Key.Suit != cardToBeatSuit)
                        .Select(c => c.Key).ToList();

                    cardSelected = (cardsZeroPoints.Count > 0)
                        ? cardsZeroPoints.First(c => c.Value == cardsZeroPoints.Min(c => c.Value))
                        : hand.First(c => c.Value == hand.Min(c => c.Value));
                }
            }
            else
            {
                if (winnerCardsTriumph.Count > 0)
                    cardSelected = winnerCardsTriumph.First(c => c.Value == winnerCardsTriumph.Min(c => c.Value));
                else
                {
                    List<Card> winnerCards = cardPerPoints
                        .Where(c => c.Key.Suit == triumphSuit)
                        .Select(c => c.Key).ToList();

                    cardSelected = (winnerCards.Count > 0)
                        ? winnerCards.First(c => c.Value == winnerCards.Min(c => c.Value))
                        : hand.First(c => c.Value == hand.Min(c => c.Value));
                }
            }
            return cardSelected;
        }

        private Card GetCardWhenNoValueToGain(Dictionary<Card, int> cardPerPoints, Suits triumphSuit, Suits cardToBeatSuit, Values cardToBeatValue)
        {
            Card cardSelected;
            // totalPointsPlayed equals to 0
            if (triumphSuit == cardToBeatSuit)
            {
                // Use a card with zero point and not a trimph suit card if not use the minimun value at hand
                List<Card> cardsZeroPoints = cardPerPoints
                    .Where(c => c.Value == 0)
                    .Where(c => c.Key.Suit != cardToBeatSuit)
                    .Select(c => c.Key).ToList();

                cardSelected = (cardsZeroPoints.Count > 0)
                    ? cardsZeroPoints.First(c => c.Value == cardsZeroPoints.Min(c => c.Value))
                    : hand.First(c => c.Value == hand.Min(c => c.Value));
            }
            else
            {
                // zero points and card to beat is not triumph suit
                // Try to set this player card as card to beat if not, use the minimun value non triumph suit at hand if posible
                List<Card> winnerCardsNonTriumph = cardPerPoints
                    .Where(c => c.Key.Suit == cardToBeatSuit)
                    .Where(c => c.Key.Value > cardToBeatValue)
                    .Select(c => c.Key).ToList();

                if (winnerCardsNonTriumph.Count > 0)
                {
                    cardSelected = winnerCardsNonTriumph.First(c => c.Value == winnerCardsNonTriumph.Min(c => c.Value));
                }
                else
                {
                    List<Card> cardsNonTriumph = cardPerPoints
                        .Where(c => c.Key.Suit != triumphSuit)
                        .Select(c => c.Key).ToList();

                    cardSelected = (cardsNonTriumph.Count > 0)
                        ? cardsNonTriumph.First(c => c.Value == cardsNonTriumph.Min(c => c.Value))
                        : hand.First(c => c.Value == hand.Min(c => c.Value));
                }
            }
            return cardSelected;
        }
        public override string ToString() => Name;
    }
}