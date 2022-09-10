using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Brisca
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
        }

        public string Name { get { return $"{Value} of {Suit}"; } }

        public override string ToString() => Name;
    }
}
