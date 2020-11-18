using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneData 
{

    public enum Actions
    {
        FORWARD = 0,
        RIGHT = 1,
        BACK = 2,
        LEFT = 3,
        LOOT = 4,
        RADAR = 5
    }

    Token token;
    int id;
    PlayerData caughtPlayer;
    public bool stunned { get; private set; }
    public int stunCount = 0;
    public Actions action;

    public DroneData(int id)
    {
        this.id = id;
    }

    public void setToken(Token t)
    {
        token = t;
    }

    public Token getToken()
    {
        return token;
    }

    public void catchPlayer(PlayerData pd)
    {
        caughtPlayer = pd;
        pd.myToken.setFollow(token.getHeldItemPosition());
        pd.caught = true;
    }

    public PlayerData getCaughtPlayer()
    {
        return caughtPlayer;
    }

    public static int convertIdToIndex(int id)
    {
        return -id - 1;
    }

    public static int convertIndexToId(int index)
    {
        return -(index + 1);
    }

    public void setStun(bool val)
    {
        if (stunned != val && val)
            stunCount = 0;
        stunned = val;
        token.stunned = val;
    }
}
