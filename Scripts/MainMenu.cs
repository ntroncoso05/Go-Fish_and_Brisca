using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text showOpponentsNumber;
    public Slider sliderOpponents;
    public GameManager gameManager;
    public InputField PlayerName;
    public GameObject MainCanvas;

    private Sprite[] characterSprites;
    private Sprite[] cardSprites;
    private Transform characters;
    private Transform backOfCards;

    private void Start()
    {
        characterSprites = Resources.LoadAll<Sprite>("Players");
        cardSprites = Resources.LoadAll<Sprite>(@"Cards\backside-of-playing-cards");
        characters = gameObject.transform.GetChild(2).GetChild(1);
        backOfCards = gameObject.transform.GetChild(4).GetChild(1);
    }

    public void OpponentsNumber() => showOpponentsNumber.text = sliderOpponents.value.ToString();

    public void OnGameStart()
    {
        var d = PlayerName;
        if(PlayerName.text != "")
        {
            gameManager.PlayerName = PlayerName.text;
            gameManager.OpponentsNumber = (int)sliderOpponents.value;
            gameManager.gameObject.SetActive(true);
            MainCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void SelectCharacter()
    {
        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        Sprite sprite = characterSprites.Where(s => s.name == name).First();
        gameManager.PlayerCharacter = sprite;

        MarkSelection(name, characters);
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
        gameManager.CardBack = sprite;

        MarkSelection(name, backOfCards);
    }

    public void RestartGame() => UnityEngine.SceneManagement.SceneManager.LoadScene("GoFishScene");

    public void ExitApplication() => Application.Quit();
}
