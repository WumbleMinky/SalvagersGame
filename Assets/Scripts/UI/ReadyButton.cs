using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ReadyButton : MonoBehaviour
{
    RoomPlayer player;

    public void onClick()
    {
        if (player == null)
            player = NetworkClient.connection.identity.GetComponent<RoomPlayer>();
        player.CmdChangeReadyState(!player.readyToBegin);
    }
}
