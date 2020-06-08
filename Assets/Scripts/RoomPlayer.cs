using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine.SceneManagement;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public string playerName;
    [SyncVar(hook = nameof(changeLobbyNameColor))]
    public Color color;
    LobbyPlayerName lobbyName;
    LobbyManager lm;

    private const string readyString = " ... [READY]";

    public override void OnStartClient()
    {
        base.OnStartClient();
        lm = GameObject.FindObjectOfType<LobbyManager>();
        lobbyName = lm.createLobbyPlayerName(this);
        lobbyName.text = playerName;
        if (readyToBegin)
            lobbyName.text = playerName + readyString;
        lobbyName.color = color;
    }

    public override void OnClientExitRoom()
    {
        base.OnClientExitRoom();
    }

    public override void OnStopClient()
    {
        Debug.Log("RoomPlayer.OnStopClient");
        SalvagerNetworkManager manager = (SalvagerNetworkManager)SalvagerNetworkManager.singleton;
        if (SceneManager.GetActiveScene().name == manager.GameplayScene)
            return;
        if (lobbyName != null)
            Destroy(lobbyName.gameObject);
        lm.players.Remove(this);
        base.OnStopClient();
    }

    private void changeLobbyNameColor(Color oldColor, Color newColor)
    {
        if (lobbyName != null)
        {
            lobbyName.color = newColor;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        lm.setLocalPlayer(this);
    }

    public override void OnClientReady(bool readyState)
    {
        base.OnClientReady(readyState);
        if (lobbyName == null)
            return;
        if (readyState)
        {
            Debug.Log(lobbyName);
            lobbyName.text = playerName + readyString;
        }
        else
        {
            lobbyName.text = playerName;
        }
    }

    [Command]
    public void CmdChangeColor(Color newColor)
    {
        color = newColor;
    }
}
