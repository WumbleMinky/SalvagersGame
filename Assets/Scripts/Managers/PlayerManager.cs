using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections.ObjectModel;

public class PlayerManager : NetworkBehaviour
{
    public int Count { get { return players.Count; } }
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private SyncNamedStatusDict statuses = new SyncNamedStatusDict();

    private void Start()
    {
        if (isClientOnly)
        {
            foreach(PlayerData pd in GameObject.FindObjectsOfType<PlayerData>())
            {
                players.Add(pd.id, new Player(pd.id, pd));
            }
        }
    }

    public void addPlayer(int id, PlayerData pd)
    {
        players.Add(id, new Player(id, pd));
    }

    public Dictionary<int, Player>.KeyCollection getIds()
    {
        return players.Keys;
    }

    public Dictionary<int, Player>.ValueCollection getPlayers()
    {
        return players.Values;
    }

    public Player getPlayer(int id)
    {
        return players[id];
    }

    public PlayerData getPlayerData(int id)
    {
        return players[id].data;
    }

    public void setTurn(int id, bool turn)
    {
        players[id].data.setMyTurn(turn);
    }

    public PlayerToken getToken(int id)
    {
        return players[id].data.myToken;
    }

    public void setToken(int id, PlayerToken token)
    {
        players[id].data.myToken = token;
    }

    public SyncNamedStatusDict getStatusDict()
    {
        return statuses;
    }

    public void setStatus(int id, string text)
    {
        if (!statuses.ContainsKey(id))
            statuses.Add(id, players[id].data.playerName, text);
        else
            statuses[id] = text;
    }

    public Card getActionCard(int id)
    {
        return players[id].actionCard;
    }

    public Card getChoiceCard(int id)
    {
        return players[id].choiceCard;
    }

    public void setActionCard(int id, Card card)
    {
        players[id].actionCard = card;
    }

    public void setChoiceCard(int id, Card card)
    {
        players[id].choiceCard = card;
    }

    public bool allPlayersHaveChosenAction()
    {
        foreach(Player p in players.Values)
        {
            if (p.actionCard == null)
            {
                return false;
            }
                
        }
        return true;
    }

    public int totalCardChoices()
    {
        int total = 0;
        foreach(Player p in players.Values)
        {
            if (p.actionCard != null)
                total++;
        }
        return total;
    }

    public void clearCardChoices()
    {
        foreach(Player p in players.Values)
        {
            p.actionCard = null;
            p.choiceCard = null;
        }
    }

    public Color getPlayerColor(int id)
    {
        return players[id].data.myColor;
    }

    public PlayerData getPlayerWithLoot()
    {
        foreach(Player player in players.Values)
        {
            if (player.data.hasLoot)
                return player.data;
        }
        return null;
    }

    public void setHasLoot(int id, bool hasLoot)
    {
        players[id].data.hasLoot = hasLoot;
    }

    public List<PlayerData> getPlayersAtPosition(Vector2Int pos)
    {
        List<PlayerData> pds = new List<PlayerData>();
        foreach(Player p in players.Values)
        {
            if (p.data.myToken.boardPosition == pos)
                pds.Add(p.data);
        }

        return pds;
    }

    public Vector2Int getPlayerPosition(int id)
    {
        return players[id].data.myToken.boardPosition;
    }

    public bool isSpaceWalking(int id)
    {
        return players[id].data.myToken.spaceRounds > 0;
    }

    public bool anyoneSpaceWalking()
    {
        foreach(Player p in players.Values)
        {
            if (p.data.myToken.spaceRounds > 0)
                return true;
        }
        return false;
    }

    public int getSpaceCount(int id)
    {
        if (players.ContainsKey(id))
            return players[id].data.myToken.spaceRounds;
        return 0;
    }
}

public class Player
{
    int connectionId;
    public PlayerData data;
    public Card actionCard;
    public Card choiceCard;

    public Player(int connId, PlayerData pd)
    {
        connectionId = connId;
        data = pd;
    }

}
