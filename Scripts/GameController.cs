using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GoFish
{
    public class GameController
    {
        public GameState gameState;

        public bool GameOver { get { return gameState.GameOver; } }
        public Player HumanPlayer { get { return gameState.HumanPlayer; } }
        public IEnumerable<Player> Opponents { get { return gameState.Opponents; } }
        public string Status { get; private set; }

        /// <summary>
        /// Constructs a new GameController
        /// </summary>
        /// <param name="humanPlayerName">Name of the human player</param>
        /// <param name="computerPlayerNames">Names of the computer players</param>
        public GameController(string humanPlayerName, IEnumerable<string> computerPlayerNames)
        {
            gameState = new GameState(humanPlayerName, computerPlayerNames, new Deck().Shuffle());
            //Status = $"Empezando un Nuevo Juego Con Los Jugadores {string.Join(", ", gameState.Players)}";
        }

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        /// <summary>
        /// Plays the next round, ending the game if everyone ran out of cards
        /// </summary>
        /// <param name="playerToAsk">Which player the human is asking for a card</param>
        /// <param name="valueToAskFor">The value of the card the human is asking for</param>
        public void NextRound(Player playerToAsk, Values valueToAskFor)
        {
            gameManager.isPlayerRound = true;
            //gameManager.currentPlayer = gameState.HumanPlayer;
            gameManager.playerToAsk = playerToAsk;
            gameManager.valueToAskFor = valueToAskFor;
            gameManager.isHumanPlayer = true;
            //Status = gameState.PlayRound(gameState.HumanPlayer, playerToAsk, valueToAskFor, gameState.Stock) + Environment.NewLine;
            //ComputerPlayersPlayNextRound();
            
            //Status += string.Join(Environment.NewLine, gameState.Players.Select(player => player.Status))+Environment.NewLine;
            
            //Status += $"The stock has {gameState.Stock.Count()} card{Player.S(gameState.Stock.Count())}";

            //Status += Environment.NewLine + gameState.CheckForWinner();
        }

        /// <summary>
        /// All of the computer players that have cards play the next round. If the human is
        /// out of cards, then the deck is depleted and they play out the rest of the game.
        /// </summary>
        public void ComputerPlayersPlayNextRound()
        {
            do
            {
                foreach (var opponent in Opponents)
                {
                    if (opponent.Hand.Count() > 0)
                        Status += $"{gameState.PlayRound(opponent, gameState.RandomPlayer(opponent), opponent.RandomValueFromHand(), gameState.Stock)}" +
                            $"{Environment.NewLine}";
                }
            } while ((gameState.HumanPlayer.Hand.Count() == 0) && (gameState.Opponents.Sum(player => player.Hand.Count()) > 0));
        }

        public void UpdateStatus(string status)
        {
            Status = status;
        }
        /// <summary>
        /// Starts a new game with the same player names
        /// </summary>
        public void NewGame()
        {
            Status = "Starting a new game";
            gameState = new GameState(gameState.HumanPlayer.Name, gameState.Opponents.Select(player => player.Name), new Deck().Shuffle());
        }
    }
}