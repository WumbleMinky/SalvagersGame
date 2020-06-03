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

    //public Color[] playerColors = new Color[]
    //{
    //    Color.red,
    //    Color.blue,
    //    Color.green,
    //    Color.magenta,
    //    Color.white,
    //    Color.yellow
    //};

    [Serializable]
    public class PlayerColor
    {
        public string name;
        public Color color;
        public Sprite sprite;

        public PlayerColor(string n, Color c, Sprite s)
        {
            name = n;
            color = c;
            sprite = s;
        }
    }
    public PlayerColor[] playerColors;

    public TileLayout securityStationTile;
    public TileLayout[] tileLayouts = new TileLayout[5];

    public List<StartingBoardLayout> startingBoardLayouts = new List<StartingBoardLayout>();

    public List<Card> powerCards = new List<Card>();
    public List<Card> standardCards = new List<Card>();

    public Dictionary<string, Card> allCards = new Dictionary<string, Card>();
    public Dictionary<string, TileLayout> tileLayoutDict = new Dictionary<string, TileLayout>();

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
}
