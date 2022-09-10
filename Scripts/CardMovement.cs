using GoFish;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour
{
    private bool isDealing, isPlayerHand = false;
    private float t, timeToReachTarget;
    private Vector3 startPosition, target;
    private Transform endPosition;
    private GameManager gameManager;
    private Sprite sprite;
    
    [HideInInspector]
    public GameObject SelectedCard;
    //[HideInInspector]
    //public bool DestroyCard;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Start()
    {
        SelectedCard = GameObject.Find("SelectedCard");
    }

    void Update()
    {

    }
    
    private void FixedUpdate()
    {
        if (isDealing)
        {
            t += Time.deltaTime / timeToReachTarget;
            transform.position = Vector3.Lerp(startPosition, target, t);
            
            if (endPosition.position == transform.position)
            {
                isDealing = false;
                this.GetComponent<Image>().sprite = sprite;
                gameObject.transform.SetParent(endPosition, false);
                gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f); 
                endPosition.parent.GetChild(1).GetComponent<Image>().color = new Color(255f, 255f, 255f);
            }
        }
    }
    
    public void StartMoving(GameObject player, GameObject card, float speedDelayer)
    {
        player.transform.GetChild(1).GetComponent<Image>().color = new Color(255f, 0f, 0f);
        if (player.name == gameManager.PlayerName)
        {
            isPlayerHand = true;
            sprite = Resources.Load<Sprite>($@"Cards\{card.name}");
        }
        else
        {
            isPlayerHand = false;
            sprite = gameManager.CardBack;
        }
        gameObject.transform.SetParent(gameManager.DeckArea.transform, true);

        Transform endPosition = player.transform.GetChild(0);
        this.endPosition = endPosition;

        t = 0;
        startPosition = transform.position;
        timeToReachTarget = 1f + speedDelayer;        
        target = endPosition.position; 
        isDealing = true;
    }

    public void RemoveCards(List<Card> cards, GameObject player)
    {
        float destroyTime = 2.25f;
        foreach (var card in cards)
        {
            Destroy(card.cardObject, destroyTime);
            destroyTime += 0.25f;
        }
        var bookObject = GameObject.Instantiate(gameManager.BookObject, new Vector2(0f, 0f), Quaternion.identity);
        bookObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($@"Cards\{cards[0].Value} of Spades");
        bookObject.transform.SetParent(player.transform.GetChild(3), false);
    }
    
    public void OnCardClick()
    {        
        if (isPlayerHand)
        {
            SelectedCard.GetComponent<Image>().sprite = sprite;
            gameManager.cardSelected = this.gameObject;

            foreach (var opponent in gameManager.gameController.Opponents)
            {
                opponent.PlayerObject.GetComponentInChildren<Opponent>().IsActive = true;
            }
        }
    }
}