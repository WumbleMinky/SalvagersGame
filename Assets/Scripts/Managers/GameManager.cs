using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameManager : NetworkBehaviour
{

    //public Vector2Int FORWARD = Vector2Int.up;
    //public Vector2Int RIGHT = Vector2Int.right;
    //public Vector2Int BACK = Vector2Int.down;
    //public Vector2Int LEFT = Vector2Int.left;

    List<TileLayout> tileDeck;
    List<Card> cardDeck;
    public GameBoard board;

    [Header("UI Components")]
    public GameObject connectionPanel;
    public GameObject lobbyPanel;
    public GameObject ingamePanel;
    public Dropdown startingDropDown;
    public Text infoTextField;

    private StartingBoardLayout startingBoardLayout;

    private List<Text> lobbyTextFields = new List<Text>();
    private int tilesLeftToPlace = 0;

    private SyncPlayersDict players = new SyncPlayersDict();
    private List<int> playerTurnOrder = new List<int>();
    private SyncListString lobbyNames = new SyncListString();

    [SyncVar]
    private int currentPlayersTurn;

    [SyncVar(hook =nameof(updateInfoTextField))]
    public string infoText;

    public class SyncPlayersDict : SyncDictionary<int, GameObject> { }

    private int chosenPowerCount = 0;
    private Token LootToken;
    public GameObject LootPrefab;

    public GameObject ItemPrefab;
    private Token ItemToken;

    private void Awake()
    {
        List<string> dropdownAdds = new List<string>();
        foreach(StartingBoardLayout sbl in ResourceContainer.Instance.startingBoardLayouts)
        {
            dropdownAdds.Add(sbl.displayName);
        }
        startingDropDown.AddOptions(dropdownAdds);
        lobbyNames.Callback += lobbyNamesUpdated;
    }

    private void lobbyNamesUpdated(SyncListString.Operation op, int index, string oldVal, string newVal)
    {
        if (op == SyncListString.Operation.OP_ADD)
        {
            LobbyPlayerName lpn = Instantiate(UIReference.Instance.playerLobbyNamePrefab, UIReference.Instance.lobbyNamePanel.transform).GetComponent<LobbyPlayerName>();
            lpn.text = newVal;
            lobbyTextFields.Add(lpn);
            RectTransform t = lpn.gameObject.GetComponent<RectTransform>();
            t.position -= t.up * t.rect.height * (lobbyTextFields.Count - 1);
        } else if  (op == SyncListString.Operation.OP_SET)
        {
            lobbyTextFields[index].text = newVal;
        }
    }

    private void updateInfoTextField(string oldValue, string newValue)
    {
        infoTextField.text = newValue;
    }

    [Server]
    List<T> shuffleDeck<T>(List<T> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            T tmp = deck[i];
            int rand = UnityEngine.Random.Range(i, deck.Count);
            deck[i] = deck[rand];
            deck[rand] = tmp;
        }
        return deck;
    }

    #region Connection & Lobby

    public void addPlayer(int connId, GameObject playerObject)
    {
        players.Add(connId, playerObject);
        lobbyNames.Add(playerObject.GetComponent<PlayerData>().playerName);
        playerTurnOrder.Add(connId);
    }

    public void startGame(bool force = false)
    {
        bool allReady = true;
        foreach(GameObject go in players.Values)
        {
            if (!go.GetComponent<PlayerData>().ready)
            {
                allReady = false;
                break;
            }
        }

        if (!allReady && !force)
            return;
        currentPlayersTurn = playerTurnOrder[0];
        RpcGameStarted();
        initializeTileDeck();
        createStartingBoard();
        tileDeck = shuffleDeck<TileLayout>(tileDeck);
        dealTilesToPlayers();
        for (int i = 0; i < playerTurnOrder.Count; i++)
        {
            changePlayerTurn(playerTurnOrder[i], playerTurnOrder[i] == currentPlayersTurn);
        }
    }

    [ClientRpc]
    public void RpcGameStarted()
    {
        connectionPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        ingamePanel.SetActive(true);
    }

    [Client]
    public void gameStopped()
    {
        connectionPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        ingamePanel.SetActive(false);
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (lobbyTextFields.Count > 0)
            return;

        //Initialize the lobby text fields for new clients.
        for(int i = 0; i < lobbyNames.Count; i++)
        {
            lobbyTextFields.Add(Instantiate(UIReference.Instance.playerLobbyNamePrefab, UIReference.Instance.lobbyNamePanel.transform).GetComponent<LobbyPlayerName>());
            lobbyTextFields[i].text = lobbyNames[i];
            RectTransform t = lobbyTextFields[i].GetComponent<RectTransform>();
            t.position -= t.up * t.rect.height * i;
        }
    }

    public void readyClick(NetworkConnection conn, bool ready, string playerName)
    {
        int index = playerTurnOrder.IndexOf(conn.connectionId);
        string lobbyNameVal = playerName;
        if (ready)
            lobbyNameVal = lobbyNameVal + " ... [READY]";
        lobbyNames[index] = lobbyNameVal;
    }

    #endregion

    #region Make The Board

    private void createStartingBoard()
    {
        string dropdownText = startingDropDown.options[startingDropDown.value].text;
        if (dropdownText.Equals("Random"))
        {
            int boardIndex = UnityEngine.Random.Range(0, ResourceContainer.Instance.startingBoardLayouts.Count);
            startingBoardLayout = ResourceContainer.Instance.startingBoardLayouts[boardIndex];
        }
        else
        {
            foreach (StartingBoardLayout sbl in ResourceContainer.Instance.startingBoardLayouts)
            {
                if (sbl.displayName.Equals(dropdownText))
                {
                    startingBoardLayout = sbl;
                    break;
                }
            }
        }
        board.addStartingTiles(startingBoardLayout);
        foreach (GameTile tile in board.grid.Values)
        {
            tileDeck.Remove(tile.layout);
        }
    }

    [Server]
    public void nextPlayersTurn()
    {
        changePlayerTurn(currentPlayersTurn, false);
        currentPlayersTurn = playerTurnOrder[(playerTurnOrder.IndexOf(currentPlayersTurn) + 1) % playerTurnOrder.Count];
        changePlayerTurn(currentPlayersTurn, true);
        tilesLeftToPlace--;

        if (tilesLeftToPlace <= 0)
        {
            board.removeAllGhosts();
            providePlayerPowerChoices();
        }
    }

    [Server]
    private void changePlayerTurn(int playerKey, bool turn)
    {
        players[playerKey].GetComponent<PlayerData>().setMyTurn(turn);
        if (turn)
            infoText = "Current Turn: " + players[playerKey].GetComponent<PlayerData>().playerName;
    }

    [Server]
    void initializeTileDeck()
    {
        List<TileLayout> deck = new List<TileLayout>();
        foreach (TileLayout layout in ResourceContainer.Instance.tileLayouts)
        {
            deck.AddRange(Enumerable.Repeat(layout, layout.total));
        }
        tileDeck = deck;
    }

    [Server]
    void dealTilesToPlayers()
    {
        Dictionary<int, List<TileLayout>> playerTiles = new Dictionary<int, List<TileLayout>>();

        for (int i = 0; i < 5; i++)
        {
            foreach (int id in players.Keys)
            {
                if (!playerTiles.ContainsKey(id))
                    playerTiles.Add(id, new List<TileLayout>());
                playerTiles[id].Add(tileDeck[0]);
                tileDeck.RemoveAt(0);
            }
        }

        foreach (int id in players.Keys)
        {
            players[id].GetComponent<PlayerData>().TargetTileHandUpdated(NetworkServer.connections[id], playerTiles[id].ToArray());
            tilesLeftToPlace += 5;
        }

    }

    #endregion

    #region Deal Cards to Players

    [Server]
    private void providePlayerPowerChoices()
    {
        infoText = "Choose your Ability";

        cardDeck = new List<Card>(ResourceContainer.Instance.powerCards);
        cardDeck = shuffleDeck<Card>(cardDeck);

        Dictionary<int, Card[]> powers = new Dictionary<int, Card[]>();

        int cardDeckIndex = 0;
        for (int i = 0; i < 2; i++)
        {
            foreach (int id in players.Keys)
            {
                if (!powers.ContainsKey(id))
                    powers.Add(id, new Card[2]);
                powers[id][i] = cardDeck[cardDeckIndex];
                cardDeckIndex++;
            }
        }

        foreach (int connId in players.Keys)
        {
            PlayerData pd = players[connId].GetComponent<PlayerData>();
            pd.TargetProvidedPowerChoices(NetworkServer.connections[connId], powers[connId]);
        }
    }

    [Server]
    public void playerSelectedPower(int connId, Card card)
    {
        cardDeck.Remove(card);
        List<Card> playersHand = new List<Card>();
        playersHand.Add(card);
        playersHand.AddRange(new List<Card>(ResourceContainer.Instance.standardCards));
        Debug.Log("[GM] " + playersHand.Count);
        players[connId].GetComponent<PlayerData>().TargetProvideCards(NetworkServer.connections[connId], playersHand.ToArray());

        chosenPowerCount++;

        if (chosenPowerCount >= players.Count)
        {
            //trigger next phase
            spawnTheLoot();
        }
    }

    #endregion

    #region Move the Loot

    private void spawnTheLoot()
    {
        GameObject startSpot = board.getOpenTokenPosition(Vector2Int.zero);
        LootToken = Instantiate(LootPrefab, startSpot.transform.position, Quaternion.identity).GetComponent<Token>();
        LootToken.setBoardPosition(Vector2Int.zero);
        NetworkServer.Spawn(LootToken.gameObject);
        if (players.Count > 2)
        {
            GameObject itemSpot = board.getOpenTokenPosition(Vector2Int.zero);
            ItemToken = Instantiate(ItemPrefab, itemSpot.transform.position, Quaternion.identity).GetComponent<Token>();
            ItemToken.setBoardPosition(Vector2Int.zero);
            NetworkServer.Spawn(ItemToken.gameObject);
        }
    }

    #endregion


}
