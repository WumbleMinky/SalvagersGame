using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net.Sockets;

public class SalvagerNetworkManager : NetworkManager
{

    public bool quickTest = false;   // <----- a temporary bool for triggering quick testing

    public string playerName { get; set; }
    public string ipaddress { get; set; }
    public GameManager gm;

    public class CreatePlayerMessage : MessageBase
    {
        public string name;
    }

    public override void Start()
    {
        base.Start();
        UIReference.Instance.ConnectionPanel.SetActive(true);
        UIReference.Instance.LobbyPanel.SetActive(false);
        UIReference.Instance.InGamePanel.SetActive(false);
        quickTest = GameObject.FindObjectOfType<NetworkManagerHUD>().showGUI;
        ipaddress = "localhost";

    }

    public void safeStartClient()
    {
        StartClient();
    }

    public void setHostName(string hostname)
    {
        networkAddress = hostname;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        networkAddress = ipaddress;
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        conn.Send(new CreatePlayerMessage() { name = playerName});
        if (quickTest && mode == NetworkManagerMode.Host)
            gm.startGame(true);
    }

    private void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        if (gm == null)
            gm = GameObject.FindObjectOfType<GameManager>();
        GameObject playerGO = Instantiate(playerPrefab);
        playerGO.GetComponent<PlayerData>().playerName = createPlayerMessage.name;
        gm.addPlayer(connection.connectionId, playerGO);
        NetworkServer.AddPlayerForConnection(connection, playerGO);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        gm.gameStopped();
    }
}
