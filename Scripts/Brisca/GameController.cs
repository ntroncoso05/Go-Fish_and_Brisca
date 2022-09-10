using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Brisca;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public GameState gameState;
    public List<GameObject> Players;
    public GameObject CardObject, DeckArea, DropZone;
    public string HumanName = "Player1";
    public Sprite BackOfCardSprite;
    public int PlayersNumber;
    [HideInInspector]
    public string GameMode;

    private float timer;

    private void Start()
    {
        instance = this;
        NewGame();
    }

    private void FixedUpdate()
    {
        if (!gameState.GameOver)
        {
            if (gameState.IsDealing || gameState.IsFirstRound) StartDealing();
            else StartNextRound();
        }
    }

    public void NewGame()
    {
        List<string> names = SelectPlayersToPlay();

        gameState = new GameState(HumanName, names,
                    new Deck().Shuffle(DeckArea.transform.GetChild(0), CardObject));

        Players[0].name = HumanName;
        foreach (Player player in gameState.PlayersRound)
        {
            player.PlayerObject = Players.First(p => p.name == player.Name);
            player.PlayerObject.transform.GetChild(3).GetComponent<Text>().text = player.Status;
        }
    }

    private List<string> SelectPlayersToPlay()
    {
        List<string> names = new List<string>();
        switch (PlayersNumber)
        {
            case 2:
                names.AddRange(new List<string>() { "Player3", "Player5" });
                break;
            case 3:
                names.AddRange(new List<string>() { "Player3", "Player4", "Player5" });
                break;
            case 5:
                names = Enumerable.Range(2, PlayersNumber).Select(i => $"Player{i}").ToList();
                break;
            default:
                names.AddRange(new List<string>() { "Player4" });
                break;
        };
        return names;
    }

    private void StartDealing()
    {
        timer += Time.deltaTime;
        if (timer >= 0.7f)
        {
            if ((gameState.CardCounter != gameState.PlayersRound.Count() * gameState.cardsPerRound) && gameState.Stock.Count > 0)
            {
                gameState.DealingCardToPlayer();
                DeckArea.transform.GetChild(1).GetComponent<Text>().text = $"{gameState.Stock.Count} Cards";
                timer = 0f;
            }
            else gameState.StopDealing();
        }
    }

    private void StartNextRound()
    {
        if (!gameState.IsHumanPlayerTurn && (gameState.CardCounter != gameState.PlayersRound.Count() * gameState.cardsPerRound))
        {
            timer += Time.deltaTime;
            if (timer >= 1.3f)
            {
                gameState.StartRound();
                timer = 0f;
            }
            else gameState.CheckToSwapTriumph();
        }

        if (gameState.IsHumanPlayerTurn) gameState.CheckToSwapTriumph();

        if (gameState.IsWaitingForCards)
        {
            timer += Time.deltaTime;
            if (timer >= 2.5f)
            {
                gameState.CheckForRoundWinner();
                timer = 0;
                gameState.IsWaitingForCards = false;
            }
        }
    }
}