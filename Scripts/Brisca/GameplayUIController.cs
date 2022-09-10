using Assets.Scripts.Brisca;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameplayUIController : MonoBehaviour
{
    public Text PlayersNumber;
    public TextMeshProUGUI volumeNumberText;
    public Slider sliderPlayers, sliderVolume;
    public InputField PlayerName;
    public GameObject MainCanvas, MainMenu, PauseMenu;
    public Transform BackOfCardSelected;
    public ToggleGroup GameModeToggles;

    [SerializeField]
    private GameController gameController;
    private Sprite[] cardSprites;
    private int playersNumber;
    private Toggle[] toggles;

    private void Start()
    {
        cardSprites = Resources.LoadAll<Sprite>(@"Cards\backside-of-playing-cards");

        toggles = GameModeToggles.GetComponentsInChildren<Toggle>();
        Toggle gameModePartners = toggles.Where(t => t.name == "Partners").FirstOrDefault();
    }

    public void OnGameStart()
    {
        if (PlayerName.text != "") gameController.HumanName = PlayerName.text;

        gameController.PlayersNumber = playersNumber;
        gameController.gameObject.SetActive(true);
        MainCanvas.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void ShowPlayerNumber()
    {
        playersNumber = sliderPlayers.value switch
        {
            1 => 2,
            2 => 3,
            3 => 5,
            _ => 1,
        };
        PlayersNumber.text = (playersNumber == 5) ? "6 Players" : $"{playersNumber + 1} Players";
        ToggleGameModes();
    }

    public void ShowVolumeNumber()
    {
        GetComponent<AudioSource>().volume = sliderVolume.value;
        volumeNumberText.SetText($"{Mathf.RoundToInt(sliderVolume.value * 100)}%");
    }

    private void ToggleGameModes()
    {
        Toggle gameModePartners = toggles.Where(t => t.name == "Partners").FirstOrDefault();
        Toggle gameModeEnemies = toggles.Where(t => t.name == "Enemy").FirstOrDefault();
        if (playersNumber >= 3)
        {
            gameModePartners.gameObject.SetActive(true);
        }
        else
        {
            gameModePartners.isOn = false;
            gameModeEnemies.isOn = true;
            gameModePartners.gameObject.SetActive(false);
        }
    }

    public void SelectToggleId()
    {
        gameController.GameMode = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
    }

    private static void MarkSelection(string name, Transform selectedSprite)
    {
        for (int i = 0; i < selectedSprite.childCount; i++)
        {
            if (selectedSprite.GetChild(i).name == name)
                selectedSprite.GetChild(i).GetComponent<Image>().color = new Color(0f, 255f, 255f);
            else
                selectedSprite.GetChild(i).GetComponent<Image>().color = new Color(255f, 255f, 255f);
        }
    }

    public void SelectCard()
    {
        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        Sprite sprite = cardSprites.Where(s => s.name == name).First();
        gameController.BackOfCardSprite = sprite;

        MarkSelection(name, BackOfCardSelected);
    }

    public void SwapCards(GameObject playerSender)
    {
        Player player = GameController.instance.gameState.PlayersRound.Where(p => p.Name == playerSender.name).First();
        GameController.instance.gameState.StartSwappingCards(player);
    }

    public void RestartGame()
    {
        foreach (Card card in gameController.gameState.TrackingCards) Destroy(card.cardObject.gameObject);

        Text textWinner = GameObject.Find("TextWinner").transform.GetComponent<Text>();
        textWinner.text = "";
        gameController.NewGame();
    }

    public void ExitApplication() => Application.Quit();

    public void OnPause()
    {
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnResume()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
}
