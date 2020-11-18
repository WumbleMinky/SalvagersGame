using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceContainer : MonoBehaviour
{
    private static ResourceContainer _inst;

    public static ResourceContainer Instance { get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<ResourceContainer>();
            return _inst;
        }
    }

    [Serializable]
    public class PlayerColor
    {
        public string name;
        public Color color;
        public Sprite sprite;
        public Color textColor;

        public PlayerColor(string n, Color c, Sprite s, Color tColor)
        {
            name = n;
            color = c;
            sprite = s;
            textColor = tColor;
        }
    }
    public PlayerColor[] playerColors;

    public Color[] droneColors;

    public TileLayout securityStationTile;
    public TileLayout[] tileLayouts = new TileLayout[5];

    public List<StartingBoardLayout> startingBoardLayouts = new List<StartingBoardLayout>();

    public List<Card> powerCards = new List<Card>();
    public List<Card> standardCards = new List<Card>();
    public Card noAction; //This is a placeholder card used for players that are stunned

    public Dictionary<string, Card> allCards = new Dictionary<string, Card>();
    public Dictionary<string, TileLayout> tileLayoutDict = new Dictionary<string, TileLayout>();

    public Card testCard01;
    public Card testCard02;

    public void Awake()
    {
        if (ResourceContainer.Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        foreach(Card card in powerCards)
        {
            allCards.Add(card.title, card);
        }
        foreach(Card card in standardCards)
        {
            allCards.Add(card.title, card);
        }

        foreach(TileLayout t in tileLayouts)
        {
            tileLayoutDict.Add(t.name, t);
        }
        tileLayoutDict.Add(securityStationTile.name, securityStationTile);
    }

    public Card getCardByTitle(string title)
    {
        return allCards[title];
    }

    public TileLayout getTileLayoutByName(string name)
    {
        return tileLayoutDict[name];
    }

    public Color getTextColor(Color c)
    {
        foreach(PlayerColor pc in playerColors)
        {
            if (pc.color == c)
                return pc.textColor;
        }
        return Color.white;
    }
}
