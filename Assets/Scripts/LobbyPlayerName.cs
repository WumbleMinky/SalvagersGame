using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerName : Text
{

    PlayerData myPlayer;

    public void setPlayer(PlayerData player)
    {
        myPlayer = player;
    }

    private void Update()
    {
        if (myPlayer == null)
            return;

        text = myPlayer.playerName;
        if (myPlayer.ready)
            text = text + " [READY]";
    }
}
