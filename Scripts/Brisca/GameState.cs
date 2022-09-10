using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Brisca
{
    public class GameState
    {
        private GameController gameController;
        private List<Card> playedCards = new List<Card>();
        private Card CardToBeat, TriumphCard;
        private bool twoPlayerModeActive = true;

        public bool GameOver { get; private set; } = false;
        public List<Player> PlayersRound = new List<Player>();
        public List<Card> TrackingCards = new List<Card>();
        public readonly Deck Stock;
        public int PlayerIndex, cardsPerRound, CardCounter;
        public bool IsFirstRound, IsHumanPlayerTurn, IsDealing, IsWaitingForCards, IsCardClickable;
        public Sprite[] CardsSprite;
        public Card CardToPlay;
        public Player CurrentPlayer;

        /// <summary>
        /// Constructor creates the players and deals their first hands
        /// </summary>
        /// <param name="humanPlayerName">Name of the human player</param>
        /// <param name="playerNames">Names of the computer players</param>
        /// <param name="stock">Shuffled stock of cards to deal from</param>
        public GameState(string humanPlayerName, IEnumerable<string> playerNames, Deck stock)
        {
            gameController = GameController.instance;

            Stock = stock;
            TrackingCards = stock.ToList();
            bool switchPlayers = false;
            foreach (string name in playerNames)
            {
                if (switchPlayers && gameController.GameMode == "Partners")
                {
                    PlayersRound.Add(new Player(name, true));
                    switchPlayers = false;
                }
                else
                {
                    PlayersRound.Add(new Player(name, false));
                    switchPlayers = (gameController.GameMode == "Partners") ? true : false;
                }
            }
            PlayersRound.Add(new Player(humanPlayerName, true));
            IsFirstRound = true;
            cardsPerRound = 3;
            CardsSprite = Resources.LoadAll<Sprite>(@"Brisca Cartas\Baraja_española");
        }

        public void DealingCardToPlayer()
        {
            Player player = PlayersRound.ElementAt(PlayerIndex++);
            Card cardToDeal = Stock.Deal(Stock.Count() - 1);
            player.DrawCard(cardToDeal);
            CardCounter++;
            if (PlayerIndex >= PlayersRound.Count()) PlayerIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopDealing()
        {
            IsDealing = false;
            CardCounter = 0;
            if (IsFirstRound)
            {
                IsFirstRound = false;
                PlayerIndex = 0;
                TriumphCard = Stock.DealTriumphCard();
                TriumphCard.cardObject.GetComponent<CardAnimation>().SetAnimation(gameController.DeckArea, 0f, false, true);
                cardsPerRound = (PlayersRound.Count == 2) ? 2 : 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartRound()
        {
            CurrentPlayer = PlayersRound.ElementAt(PlayerIndex++);
            if (!CurrentPlayer.IsHuman)
            {
                CardToPlay = CurrentPlayer.SelectValueFromHand(TriumphCard, CardToBeat, playedCards);
                PlayerPlayRound();
            }
            else
            {
                IsHumanPlayerTurn = true;
                IsCardClickable = true;
            }
        }
        int scoreInRound;
        public void PlayerPlayRound()
        {
            CurrentPlayer.PlayCard(CardToPlay, gameController.DropZone);
            playedCards.Add(CardToPlay);
            CardCounter++;

            if (CardToBeat != null) CardToBeat = SetCardToBeat();
            else
            {
                CardToBeat = CardToPlay;
                scoreInRound = 0;
            }

            scoreInRound += (int)Enum.Parse(typeof(CardPoints), Enum.GetName(typeof(Values), CardToPlay.Value));
            gameController.DropZone.transform.GetChild(1).GetComponent<Text>().text = $"Cards Played [{scoreInRound} points]";

            if (PlayersRound.Count > 2) twoPlayerModeActive = false;

            if (PlayerIndex >= PlayersRound.Count && !twoPlayerModeActive) IsWaitingForCards = true;

            if (PlayerIndex >= PlayersRound.Count && PlayersRound.Count == 2 && twoPlayerModeActive)
            {
                PlayerIndex = 0;
                twoPlayerModeActive = false;
            }
            IsHumanPlayerTurn = false;
        }        

        public void CheckForRoundWinner()
        {

            int CardToBeatIndex = playedCards.IndexOf(CardToBeat);
            int indexToUse = (PlayersRound.Count == 2) 
                ? (CardToBeatIndex - 2 < 0)
                    ? CardToBeatIndex
                    : CardToBeatIndex - 2
                : CardToBeatIndex;

            Player player = PlayersRound[indexToUse];
            player.AddPointsAndCollectCards(playedCards);
            player.PlayerObject.transform.GetChild(3).GetComponent<Text>().text = player.Status;
            playedCards.Clear();

            SetNextRoundOrder(indexToUse);
            IsDealing = true;
            PlayerIndex = 0;
            CardCounter = 0;
            twoPlayerModeActive = true;
            CardToBeat = null;
            gameController.DropZone.transform.GetChild(1).GetComponent<Text>().text = "";

            if (PlayersRound.Sum(player => player.Hand.Count()) == 0)
            {
                GameOver = true;
                GetGameWinner();
                Debug.Log("Game Over");
                return;
            }
        }

        private void GetGameWinner()
        {
            Text textWinner = GameObject.Find("TextWinner").transform.GetComponent<Text>();
            GameOver = true;

            if (gameController.GameMode == "Partners")
            {
                List<(int totalPoints, bool isHuman, int countCards)> playerPoints = PlayersRound
                    .GroupBy(p => p.IsHuman)
                    .Select(p => (totalPoints: p.Sum(c => c.CardPoints.Sum()), isHuman: p.Key, 
                    countCards: p.Sum(c => c.CardPoints.Count())))
                    .OrderByDescending(a => a.totalPoints)
                    .ToList();

                int humanPoints = playerPoints.First(p => p.isHuman).totalPoints;
                int npcPoints = playerPoints.First(p => !p.isHuman).totalPoints;
                int humanCount = playerPoints.First(p => p.isHuman).countCards;
                int npcCount = playerPoints.First(p => !p.isHuman).countCards;

                if (humanPoints == npcPoints)
                {
                    bool isHumanPlayer = playerPoints.First(p => p.countCards == playerPoints.Max().countCards).isHuman;
                    textWinner.text = (isHumanPlayer)
                        ? $"The Winner Is\n{gameController.HumanName}\n{humanPoints} points and {humanCount} cards"
                        : $"The Winner Is\nComputador\n{npcPoints} points and {npcCount} Cards";
                }
                else
                {
                    textWinner.text = (humanPoints > npcPoints)
                        ? $"The Winner Is\n{gameController.HumanName}\n{humanPoints} points and {humanCount} cards"
                        : $"The Winner Is\nComputador\n{npcPoints} points and {npcCount} cards";
                }
            }
            else
            {
                int playerPoints = PlayersRound.Select(player => player.CardPoints.Sum()).Max();
                IEnumerable<Player> winners = PlayersRound.Where(player => player.CardPoints.Sum() == playerPoints);
                int playerCards = winners.Select(player => player.CardPoints.Count()).Max();

                if (winners.Count() == 1) textWinner.text = $"The Winner Is {winners.First().Name}\n{playerPoints} points and {playerCards} cards";
                else
                {
                    var tiedWinners = winners.Where(player => player.CardPoints.Count() == playerCards);

                    if (tiedWinners.Count() == 1) textWinner.text = $"The Winner Is {tiedWinners.First().Name}\n{playerPoints} points and {playerCards} cards";
                    else textWinner.text = $"The Winners Are {string.Join(" And ", winners)}";
                }
            }            
        }

        private void SetNextRoundOrder(int indexToUse)
        {
            List<Player> playersRoundCopy = PlayersRound.ToList();
            PlayersRound.Clear();
            for (int i = 0; i < playersRoundCopy.Count; i++)
            {
                PlayersRound.Add(playersRoundCopy[indexToUse++]);
                if (indexToUse > playersRoundCopy.Count - 1) indexToUse = 0;
            }
        }

        private Card SetCardToBeat()
        {
            List<Card> triumphSuits = playedCards.Where(c => c.Suit == TriumphCard.Suit).ToList();
            Card cardToBeat;

            if (triumphSuits.Count() > 0)
            {
                cardToBeat = triumphSuits.Where(card => card.Value == triumphSuits.Max(c => c.Value)).First();
            }
            else
            {
                List<Card> cardsSuitToBeat = playedCards.Where(c => c.Suit == CardToBeat.Suit).ToList();

                cardToBeat = (cardsSuitToBeat.Count() > 0)
                    ? cardsSuitToBeat.Where(card => card.Value == cardsSuitToBeat.Max(c => c.Value)).First()
                    : CardToBeat;
            }
            return cardToBeat;
        }

        public void CheckToSwapTriumph()
        {
            if (Stock.Count > 0)
            {
                Values triumphValueToSwap = (TriumphCard.Value > Values.Seven) ? Values.Seven : Values.Two;
                foreach (var player in PlayersRound)
                {
                    if (player.CardPoints.Count() > 0)
                    {
                        bool value = player.Hand.Select(c => c.Name).Contains($"{triumphValueToSwap} of {TriumphCard.Suit}");
                        if (player.IsHuman)
                        {
                            GameObject swapButton = player.PlayerObject.transform.GetChild(4).gameObject;
                            if (value) swapButton.SetActive(true);
                            else swapButton.SetActive(false);
                        }
                        else if (value) StartSwappingCards(player);
                    }
                } 
            }
        }

        public void StartSwappingCards(Player player)
        {
            Card oldTriumph = TriumphCard;
            Card newTriumph = (oldTriumph.Value > Values.Seven)
                ? player.Hand.Where(c => c.Suit == oldTriumph.Suit)
                             .First(c => c.Value == Values.Seven)
                : player.Hand.Where(c => c.Suit == oldTriumph.Suit)
                             .First(c => c.Value == Values.Two);

            CardAnimation triumphAnimation = oldTriumph.cardObject.GetComponent<CardAnimation>();
            CardAnimation playerAnimation = newTriumph.cardObject.GetComponent<CardAnimation>();

            triumphAnimation.SetAnimation(player.PlayerObject, 0.3f, false, (player.IsHuman) ? true : false);
            triumphAnimation.isTriumph = false;

            playerAnimation.isTriumph = true;
            playerAnimation.SetAnimation(gameController.DeckArea, 0.3f, false, true);
            player.SwapCard(newTriumph, oldTriumph);

            TriumphCard = newTriumph;
            Stock.SwapTriumphCard(newTriumph);
        }
    }
}