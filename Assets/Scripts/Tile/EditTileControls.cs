using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class EditTileControls : MonoBehaviour
{
    PlayerData player;
    
    private void updatePlayer()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<PlayerData>();
        }
    }

    public void onRotateClick()
    {
        updatePlayer();
        player.CmdRotateTile();
    }

    public void onCancelClick()
    {
        updatePlayer();
        player.CmdCancelTile();
        CardSelection.Instance.deselectLayout(true);
    }

    public void onConfirmClick()
    {
        //updatePlayer();
        //player.CmdConfirmTile();
        //EventSystem.current.SetSelectedGameObject(null);
    }

}
