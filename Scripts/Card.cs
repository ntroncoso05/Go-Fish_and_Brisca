using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GoFish
{
    public class Card
    {
        public Values Value { get; private set; }
        public Suits Suit { get; set; }

        public GameObject cardObject;

        public Card(Values value, Suits suit)
        {
            Value = value;
            Suit = suit;

            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            //Sprite[] sprites = Resources.LoadAll<Sprite>(@"Cards\backside-of-playing-cards");
            Sprite sprite = gameManager.CardBack;

            cardObject = GameObject.Instantiate(gameManager.CardObject, new Vector2(0f, 0f), Quaternion.identity);
            cardObject.gameObject.GetComponent<Image>().sprite = sprite;
        }

        public string Name { get { return $"{Value} of {Suit}"; } }

        public override string ToString()
        {
            return Name;
        }
    }
}
