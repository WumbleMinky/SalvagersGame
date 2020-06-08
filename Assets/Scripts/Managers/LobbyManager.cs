using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class LobbyManager : NetworkBehaviour
{

    public List<RoomPlayer> players = new List<RoomPlayer>();
    public GameObject playerLobbyNamePrefab;
    public GameObject playerListPanel;
    public RoomPlayer localPlayer;
    public ColorSelect dropdown;
    public Button startButton;
    public int startingLayoutIndex = -1;

    private void Start()
    {
        //DontDestroyOnLoad(gameObject);
        playerListPanel = GameObject.Find("Player List Panel");
    }

    [Server]
    public void addPlayer(int connectionId, GameObject player)
    {
        RoomPlayer rp = player.GetComponent<RoomPlayer>();
        players.Add(rp);
        rp.color = ResourceContainer.Instance.playerColors[players.Count - 1].color;
    }

    [Command]
    public void CmdRemovePlayer(GameObject roomPlayerGO)
    {

    }

    public void setLocalPlayer(RoomPlayer player)
    {
        localPlayer = player;
        dropdown.setColor(player.color);
    }

    public void setStartButtonEnabled(bool value)
    {
        startButton.interactable = value;
    }

    public void startButtonClicked()
    {
        DontDestroyOnLoad(this.gameObject);
        ((SalvagerNetworkManager)SalvagerNetworkManager.singleton).startGame();
    }

    public bool isColorTaken(Color color)
    {
        foreach(RoomPlayer rp in players)
        {
            if (rp.color == color)
                return true;
        }
        return false;
    }

    [Client]
    public LobbyPlayerName createLobbyPlayerName(RoomPlayer player)
    {
        LobbyPlayerName lobbyName = Instantiate(playerLobbyNamePrefab, playerListPanel.transform).GetComponent<LobbyPlayerName>();
        if (!isServer)
            players.Add(player);
        RectTransform t = lobbyName.gameObject.GetComponent<RectTransform>();
        t.position -= t.up * t.rect.height * (players.Count-1);
        return lobbyName;
    }
}
