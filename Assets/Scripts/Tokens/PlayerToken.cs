using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToken : Token
{

    private PlayerData playerData;

    public void setPlayerData(PlayerData pd)
    {
        playerData = pd;
        pd.myToken = this;
    }

    public PlayerData getPlayer()
    {
        return playerData;
    }
}
