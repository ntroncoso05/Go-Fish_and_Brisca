using GoFish;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Opponent : MonoBehaviour
{
    public GameManager gameManager;
    public bool IsActive;
    public void StartNextRound()
    {
        if (IsActive)
        {
            string[] valueName = gameManager.cardSelected.name.Split(' ');
            Values valueToAsk = (Values)System.Enum.Parse(typeof(Values), valueName[0]);

            gameManager.gameController.NextRound(gameManager.gameController.
                Opponents.Where(o => o.Name == gameObject.name).First(), valueToAsk);
            gameManager.cardSelected.GetComponent<CardMovement>().
                SelectedCard.transform.GetComponent<Image>().sprite = Resources.Load<Sprite>($@"Cards\back_of_card");

            foreach (var opponent in gameManager.gameController.Opponents)
            {
                opponent.PlayerObject.GetComponentInChildren<Opponent>().IsActive = false;
            } 
        }
    }
}