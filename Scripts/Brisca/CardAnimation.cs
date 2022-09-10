using Assets.Scripts.Brisca;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardAnimation : MonoBehaviour
{
    private bool startAnimation = false;
    private float timeToTarget, timeToReachTarget;
    private Vector3 startPosition;
    private Transform target;
    private GameController gameController;
    private Sprite sprite;

    public bool isTriumph;

    private void Start()
    {
        gameController = GameController.instance;
    }

    private void FixedUpdate()
    {
        if (startAnimation) StartCardAnimation();
    }

    private void StartCardAnimation()
    {
        timeToTarget += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, target.position, timeToTarget);

        if (target.position == transform.position)
        {
            startAnimation = false;
            transform.SetParent(target, false);
            GetComponent<Image>().sprite = sprite; // TODO - move line to better place?
            if (target.parent.gameObject == gameController.DeckArea)
            {
                transform.SetSiblingIndex(0);
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                transform.localPosition = new Vector3(-90f, 0f, 0f);
                isTriumph = true;
            }
        }
    }

    public void SetAnimation(GameObject targetObject, float speedDelayer, bool stolen, bool showCard)
    {
        // transform.Rotate() adds value to angle
        if (targetObject != gameController.DeckArea)
        {
            if(transform.parent.parent.gameObject.name == "Player3") 
                transform.rotation = Quaternion.Euler(0f, 0f, 124f);

            transform.SetParent(gameController.DeckArea.transform, true); 
        }
        else
        {
            transform.SetParent(transform, true);
            transform.localPosition = new Vector3(0f, -60f, 0f);
        }

        if (isTriumph) transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (showCard)
            sprite = gameController.gameState.CardsSprite.Where(s => s.name == name).First();
        else sprite = gameController.BackOfCardSprite;

        target = (!stolen) ? targetObject.transform.GetChild(0) : targetObject.transform.GetChild(1);
        timeToTarget = 0f;
        startPosition = transform.position;
        timeToReachTarget = 1f + speedDelayer;
        startAnimation = true;
    }

    public void OnCardClick()
    {
        GameState gameState = gameController.gameState;
        if (gameState.IsCardClickable)
        {
            Card cardToPlay = gameState.CurrentPlayer.Hand.Where(c => c.Name == gameObject.name).FirstOrDefault();
            if (cardToPlay != null)
            {
                gameState.CardToPlay = cardToPlay;
                gameState.IsCardClickable = false;
                gameState.PlayerPlayRound();
            }
        }
    }
}
