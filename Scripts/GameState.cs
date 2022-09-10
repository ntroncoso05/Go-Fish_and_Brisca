using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace GoFish
{
    public class GameState
    {
        public readonly IEnumerable<Player> Players;
        public readonly IEnumerable<Player> Opponents;
        public readonly Player HumanPlayer;
        public bool GameOver { get; private set; } = false;
        public readonly Deck Stock;

        /// <summary>
        /// Constructor creates the players and deals their first hands
        /// </summary>
        /// <param name="humanPlayerName">Name of the human player</param>
        /// <param name="opponentNames">Names of the computer players</param>
        /// <param name="stock">Shuffled stock of cards to deal from</param>
        public GameState(string humanPlayerName, IEnumerable<string> opponentNames, Deck stock)
        {
            Stock = stock;
            HumanPlayer = new Player(humanPlayerName);
            //HumanPlayer.GetNextHand(Stock);
            var opponents = new List<Player>();
            foreach (string name in opponentNames)
            {
                var player = new Player(name);
                //player.GetNextHand(stock);
                opponents.Add(player);
            }
            Opponents = opponents;
            Players = new List<Player>() { HumanPlayer }.Concat(Opponents);

            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.isFirstMove = true;
        }

        /// <summary>
        /// Gets a random player that doesn't match the current player
        /// </summary>
        /// <param name="currentPlayer">The current player</param>
        /// <returns>A random player that the current player can ask for a card</returns>
        public Player RandomPlayer(Player currentPlayer) => Players
            .Where(player => player != currentPlayer).Skip(Player.random.Next(Players.Count() - 1)).First();

        /// <summary>
        /// Makes one player play a round
        /// </summary>
        /// <param name="player">The player asking for a card</param>
        /// <param name="playerToAsk">The player being asked for a card</param>
        /// <param name="valueToAskFor">The value to ask the player for</param>
        /// <param name="stock">The stock to draw cards from</param>
        /// <returns>A message that describes what just happened</returns>
        public string PlayRound(Player player, Player playerToAsk, Values valueToAskFor, Deck stock)
        {
            int indice = (int)valueToAskFor;
            Valores valor = (Valores)indice;

            string valuePlural = (valor == Valores.Dos || valor == Valores.Tres || valor == Valores.Seis || valor == Valores.Rey)
                ? $"{valor}es" : (valor == Valores.Diez) ? "Dieces" : $"{valor}s";
            
            string message = $"Pregunta A {playerToAsk.Name} Por {valuePlural}{Environment.NewLine}";

            var cards = playerToAsk.DoYouHaveAny(valueToAskFor, stock);
            if (cards.Count() > 0)
            {
                player.AddCardsAndPullOutBooks(cards);
                message += $"{playerToAsk.Name} Tiene {cards.Count()} {valor} Carta{Player.S(cards.Count())}";
            }
            else
            {
                player.DrawCard(stock);
                message += $"{player.Name} Obtuvo Carta Del Mazo";
            }
            if (player.Hand.Count() == 0)
            {
                player.GetNextHand(stock);
                message += $"{Environment.NewLine}{player.Name} Se Quedo Sin Carta, Obtuvo {player.Hand.Count()} Del Mazo";
            }
            return message;
        }

        /// <summary>
        /// Checks for a winner by seeing if any players have any cards left, sets GameOver
        /// if the game is over and there's a winner
        /// </summary>
        /// <returns>A string with the winners, an empty string if there are no winners</returns>
        public string CheckForWinner()
        {
            var playerCards = Players.Select(player => player.Hand.Count()).Sum();
            if (playerCards > 0) return "No Ganadores Todavia";

            GameOver = true;
            var winningBookCount = Players.Select(player => player.Books.Count()).Max();
            var winners = Players.Where(player => player.Books.Count() == winningBookCount);

            if (winners.Count() == 1) return $"El Ganador Es {winners.First().Name}";

            return $"Los Ganadores Son {string.Join(" Y ", winners)}";
        }
    }
}