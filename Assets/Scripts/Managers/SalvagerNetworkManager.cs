using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net.Sockets;
using System;

public class SalvagerNetworkManager : NetworkRoomManager
{

    public bool quickTest = false;   // <----- a temporary bool for triggering quick testing

    public string playerName { get; set; }
    public string ipaddress { get; set; }
    public GameManager gm;
    public LobbyManager lm;

    public class CreatePlayerMessage : MessageBase
    {
        public string name;
    }

    public override void Start()
    {
        base.Start();
        quickTest = GameObject.FindObjectOfType<NetworkManagerHUD>().showGUI;
        ipaddress = "localhost";
    }

    public void setHostName(string hostname)
    {
        networkAddress = hostname;
    }

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();
        networkAddress = ipaddress;
        
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    public override void OnRoomClientConnect(NetworkConnection conn)
    {
        base.OnRoomClientConnect(conn);
        conn.Send(new CreatePlayerMessage() { name = playerName });
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        //transfer info from roomPlayer to gamePlayer

        PlayerData pd = gamePlayer.GetComponent<PlayerData>();
        RoomPlayer rp = roomPlayer.GetComponent<RoomPlayer>();

        pd.playerName = rp.playerName;
        pd.myColor = rp.color;
        gm.addPlayer(conn.connectionId, gamePlayer);
        Destroy(roomPlayer);
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    private void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        if (lm == null)
            lm = GameObject.FindObjectOfType<LobbyManager>();

        GameObject playerGO = Instantiate(roomPlayerPrefab.gameObject);
        playerGO.GetComponent<RoomPlayer>().playerName = createPlayerMessage.name;
        lm.addPlayer(connection.connectionId, playerGO);
        NetworkServer.AddPlayerForConnection(connection, playerGO);
    }

    public override void OnRoomServerPlayersReady()
    {
        //left empty to prevent the parent from automatically going to the next scene.
    }

    public void startGame()
    {
        ServerChangeScene(GameplayScene);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName != GameplayScene)
            return;
        gm = GameObject.FindObjectOfType<GameManager>();
        gm.totalPlayers = lm.players.Count;
        gm.setStartingBoard(lm.startingLayoutIndex);

        //transfer data from LobbyManager to GameManager then delete LobbyManager
        Destroy(lm.gameObject);
    }

    public override void OnRoomServerDisconnect(NetworkConnection conn)
    {
        base.OnRoomServerDisconnect(conn);
        if (lm != null && lm.players.Count == 1)
        {
            Destroy(lm.gameObject);
            lm = null;
        }
    }

    public override void OnRoomServerConnect(NetworkConnection conn)
    {
        base.OnRoomServerConnect(conn);
        allPlayersReady = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (NetworkServer.localClientActive)
            {
                Debug.Log("Stopping Host");
                this.StopHost();
            }
            else
            {
                Debug.Log("Stopping Client");
                this.StopClient(); 
            }
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (lm!=null)
            lm.setStartButtonEnabled(allPlayersReady);
    }
}
