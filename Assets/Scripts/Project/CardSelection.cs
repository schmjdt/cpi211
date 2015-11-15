using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardSelection : MonoBehaviour {
    public static CardSelection instance;

    public RawImage topCard;
    public RawImage[] cards;

    public Sprite blankCard;

    public int cardsTotal;
    public int cardsRemaining;
     
    void Awake()
    {
        instance = this;

        topCard = GameObject.Find("cardDeck").GetComponentInChildren<RawImage>();
        cards = GameObject.Find("cardSelected").GetComponentsInChildren<RawImage>();
    }

	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
	    if (cardsRemaining == 0)
        {
        }
	}
}
