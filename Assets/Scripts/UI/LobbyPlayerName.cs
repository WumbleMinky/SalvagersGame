using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerName : Text
{

    PlayerData myPlayer;
    public Image colorIndicator;

    public void setPlayer(PlayerData player)
    {
        myPlayer = player;
        colorIndicator = transform.GetComponentInChildren<Image>();
    }

    private void Update()
    {
        //if (myPlayer == null)
        //    return;

        //text = myPlayer.playerName;
        //colorIndicator.color = myPlayer.myColor;
        //if (myPlayer.ready)
        //    text = text + " [READY]";
    }
}
