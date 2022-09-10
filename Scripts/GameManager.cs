using GoFish;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GoFish.GameController gameController;
    public List<GameObject> OpponentAreas;
    public GameObject CardObject, DeckArea, PlayerArea, cardSelected, BookObject;

    private float timer;
    public bool isFirstMove, isPlayerRound, isHumanPlayer;
    public int timeIndex;
    public IEnumerable<Card> matchingCards;
    public Player playerToAsk;
    public Values valueToAskFor;
    public string Status;
    public List<string> playerStatus;

    public string PlayerName;
    public Sprite PlayerCharacter, CardBack;
    public int OpponentsNumber;

    void Start()
    {
        PlayerArea.name = PlayerName;
        for (int i = 0; i < 5; i++)
        {
            playerStatus.Add("");
        }
        //isFromDeck = true;

        gameController = new GoFish.GameController(PlayerName, Enumerable.Range(1, OpponentsNumber).Select(i => $"Opponent{i}"));
        Status = $"Empezando un Nuevo Juego Con Los Jugadores {string.Join(", ", gameController.gameState.Players)}";

        gameController.HumanPlayer.PlayerObject.transform.GetChild(1).GetComponent<Image>().sprite = PlayerCharacter;
        foreach (var opponentArea in gameController.Opponents)
        {
            opponentArea.PlayerObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        CheckStatus();
        if (isFirstMove)
        {
            timer += Time.deltaTime;
            if (timer >= 2f)
            {
                Player player = gameController.gameState.Players.ElementAt(timeIndex++);
                player.GetNextHand(gameController.gameState.Stock);
                if (gameController.gameState.Players.Count() == timeIndex)
                {
                    isFirstMove = false;
                    timeIndex = 0;
                }
                timer = 0;
            }
        }

        if (isPlayerRound)
        {
            timer += Time.deltaTime;
            if (isHumanPlayer)
            {
                playerStatus[0] =  gameController.gameState.PlayRound(gameController.gameState.HumanPlayer, playerToAsk, valueToAskFor, gameController.gameState.Stock) + Environment.NewLine;
                isHumanPlayer = false;
                timer = 0;
            }
            if (timer >= 2f)
            {
                do
                {
                    Player player = gameController.gameState.Opponents.ElementAt(timeIndex++);
                    if (player.Hand.Count() > 0)
                        playerStatus[timeIndex] = $"{gameController.gameState.PlayRound(player, gameController.gameState.RandomPlayer(player), player.RandomValueFromHand(), gameController.gameState.Stock)}" +
                            $"{Environment.NewLine}";
                    if (gameController.gameState.Opponents.Count() == timeIndex)
                    {
                        timeIndex = 0;
                        isPlayerRound = false;
                        //Status += string.Join(Environment.NewLine, gameController.gameState.Players.Select(player => player.Status)) + Environment.NewLine;

                        Status = $"Hay {gameController.gameState.Stock.Count()} Carta{Player.S(gameController.gameState.Stock.Count())} En El Mazo";

                        Status += Environment.NewLine + gameController.gameState.CheckForWinner();
                        //gameController.UpdateStatus(Status);
                    }
                    timer = 0;
                } while ((gameController.gameState.HumanPlayer.Hand.Count() == 0) && (gameController.gameState.Opponents.Sum(player => player.Hand.Count()) > 0));
                if (gameController.gameState.Players.Sum(player => player.Hand.Count()) == 0)
                {
                    //Status += string.Join(Environment.NewLine, gameController.gameState.Players.Select(player => player.Status)) + Environment.NewLine;

                    Status = $"Hay {gameController.gameState.Stock.Count()} Carta{Player.S(gameController.gameState.Stock.Count())} En El Mazo";

                    Status += Environment.NewLine + gameController.gameState.CheckForWinner();
                    //gameController.UpdateStatus(Status);
                }
            }
        }
    }

    private void CheckStatus()
    {
        //GameObject opponentObject;
        //string playerHand = "";
        int i = 0;
        foreach (var opponent in gameController.Opponents)
        {
            //i++;
            //foreach (var item in opponent.Hand)
            //{
            //    playerHand += $"\n{item.Name}";
            //}
            //opponentObject = OpponentAreas.Where(o => o.name.Substring(0, o.name.Length-4) == opponent.Name).First();
            opponent.PlayerObject.transform.Find("PlayerStatusUI").GetComponent<Text>().text = $"{opponent.Status}\n{playerStatus[++i]}";
            //playerHand = "";
        }
        
        //foreach (var item in gameController.HumanPlayer.Hand)
        //{
        //    playerHand += $"\n{item.Name}";
        //}
        PlayerArea.transform.Find("PlayerStatusUI").GetComponent<Text>().text = $"{gameController.HumanPlayer.Status}\n{playerStatus[0]}";
        DeckArea.transform.Find("GameStatusUI").GetComponent<Text>().text = Status;
    }    
}