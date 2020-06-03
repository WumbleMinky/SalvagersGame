using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Specialized;

public class GameManager : NetworkBehaviour
{
    List<TileLayout> tileDeck;
    List<Card> cardDeck;
    public GameBoard board;

    [Header("UI Components")]
    public GameObject connectionPanel;
    public GameObject lobbyPanel;
    public GameObject ingamePanel;
    public Text infoTextField;

    private StartingBoardLayout startingBoardLayout;

    
    private List<Text> lobbyTextFields = new List<Text>();
    private int tilesLeftToPlace = 0;

    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
    private Dictionary<int, CardPlay> playersCardChoices = new Dictionary<int, CardPlay>();
    private List<int> playerTurnOrder = new List<int>();
    private SyncListStringColor lobbyNames = new SyncListStringColor();

    private SyncNamedStatusDict playerStatus = new SyncNamedStatusDict();

    private Dictionary<int, Text> inGamePlayerList = new Dictionary<int, Text>();

    private Dictionary<Card, CardLogic> cardLogicMap = new Dictionary<Card, CardLogic>();

    struct CardPlay
    {
        public Card mainCard;
        public Card choiceCard;

        public CardPlay(Card main, Card choice = null)
        {
            mainCard = main;
            choiceCard = choice;
        }
    }

    [SyncVar]
    private int currentPlayersTurn;

    [SyncVar(hook =nameof(updateInfoTextField))]
    public string infoText;

    //public class SyncPlayersDict : SyncDictionary<int, GameObject> { }

    private int chosenPowerCount = 0;
    private Token LootToken;
    public GameObject LootPrefab;
    public GameObject ItemPrefab;
    private Token ItemToken;
    public GameObject PlayerPrefab;

    private void Awake()
    {
        lobbyNames.Callback += lobbyNamesUpdated;
        playerStatus.Callback += playerStatusUpdated;
    }

    private void playerStatusUpdated(SyncNamedStatusDict.Operation op, int key, string value)
    {
        if (op == SyncNamedStatusDict.Operation.OP_ADD)
        {
            GameObject go = Instantiate(UIReference.Instance.playerStatusTextPrefab, UIReference.Instance.playerStatusPanel.transform);
            Text textComp = go.GetComponent<Text>();
            textComp.text = value;
            inGamePlayerList.Add(key, textComp);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.position -= rt.up * ( (rt.rect.height * rt.localScale.y + 5 ) * (inGamePlayerList.Count - 1) );
        }
        else if (op == SyncNamedStatusDict.Operation.OP_SET)
        {
            inGamePlayerList[key].text = value;
        }
    }

    private void lobbyNamesUpdated(SyncListStringColor.Operation op, int index, string oldVal, string newVal)
    {
        PlayerData pd = players[playerTurnOrder[index]];
        if (op == SyncListString.Operation.OP_ADD)
        {
            //LobbyPlayerName lpn = Instantiate(UIReference.Instance.playerLobbyNamePrefab, UIReference.Instance.lobbyNamePanel.transform).GetComponent<LobbyPlayerName>();
            pd.createLobbyName(index, newVal);
            //lpn.text = newVal;
            //lobbyTextFields.Add(lpn);
            //lpn.transform.GetComponentInChildren<Image>().color = lobbyNames.getColor(index);
            //RectTransform t = lpn.gameObject.GetComponent<RectTransform>();
            //t.position -= t.up * t.rect.height * (lobbyTextFields.Count - 1);
        } else if  (op == SyncListString.Operation.OP_SET)
        {
            //lobbyTextFields[index].text = newVal;
            pd.updateLobbyName(newVal);
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
        players.Add(connId, playerObject.GetComponent<PlayerData>());
        players[connId].myColor = ResourceContainer.Instance.playerColors[players.Count-1].color;
        playerTurnOrder.Add(connId);
        lobbyNames.Add(players[connId].playerName, players[connId].myColor);
    }

    public bool isColorTaken(Color color)
    {
        foreach(PlayerData pd in players.Values)
        {
            if (pd.myColor == color)
                return true;
        }
        return false;
    }

    public void startGame(bool force = false)
    {
        bool allReady = true;
        foreach(PlayerData pData in players.Values)
        {
            if (!pData.ready)
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
            int pConnId = playerTurnOrder[i];
            playerStatus.Add(pConnId, players[pConnId].playerName , "Playing");
            changePlayerTurnTilePlacement(pConnId, pConnId == currentPlayersTurn);
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
        if (isServer || lobbyTextFields.Count > 0)
            return;

        //Initialize the lobby text fields for new clients.
        for(int i = 0; i < lobbyNames.Count; i++)
        {
            //PlayerData pd = players[playerTurnOrder[i]];
            //pd.createLobbyName(i, pd.playerName);
            lobbyTextFields.Add(Instantiate(UIReference.Instance.playerLobbyNamePrefab, UIReference.Instance.lobbyNamePanel.transform).GetComponent<LobbyPlayerName>());
            lobbyTextFields[i].text = lobbyNames[i];
            lobbyTextFields[i].transform.GetComponentInChildren<Image>().color = lobbyNames.getColor(i);
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
        string dropdownText = UIReference.Instance.startingLayoutDropDown.options[UIReference.Instance.startingLayoutDropDown.value].text;
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
    public void playerFinishedPlacingTile()
    {
        changePlayerTurnTilePlacement(currentPlayersTurn, false);
        currentPlayersTurn = playerTurnOrder[(playerTurnOrder.IndexOf(currentPlayersTurn) + 1) % playerTurnOrder.Count];
        changePlayerTurnTilePlacement(currentPlayersTurn, true);
        tilesLeftToPlace--;

        if (tilesLeftToPlace <= 0)
        {
            board.removeAllGhosts();
            board.findSpaceTiles();
            providePlayerPowerChoices();
        }
    }

    [Server]
    private void changePlayerTurnTilePlacement(int playerKey, bool turn)
    {
        players[playerKey].setMyTurn(turn);
        if (turn)
        {
            
            playerStatus[playerKey] = "Placing Tile";
        }
        else
        {
            playerStatus[playerKey] = "Waiting";
        }
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
        infoText = "Placing Tiles";
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
            players[id].TargetTileHandUpdated(NetworkServer.connections[id], playerTiles[id].ToArray());
            tilesLeftToPlace += 5;
        }

    }

    #endregion

    #region Deal Cards to Players

    private void mapCardLogic(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            cardLogicMap.Add(card, card.prefab.GetComponent<CardLogic>());
        }
    }

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
            players[connId].TargetProvidedPowerChoices(NetworkServer.connections[connId], powers[connId]);
            playerStatus[connId] = "Selecting Ability";
        }
    }

    [Server]
    public void playerSelectedPower(int connId, Card card)
    {
        cardDeck.Remove(card);
        List<Card> playersHand = new List<Card>();
        playersHand.Add(card);
        playersHand.AddRange(new List<Card>(ResourceContainer.Instance.standardCards));
        players[connId].TargetProvideCards(NetworkServer.connections[connId], playersHand.ToArray());
        playerStatus[connId] = "Ability Chosen. Waiting.";
        chosenPowerCount++;

        if (chosenPowerCount >= players.Count)
        {
            RpcShowCardConfirmButton();
            spawnTheLoot();
        }
    }

    #endregion

    #region Move the Loot

    [ClientRpc]
    public void RpcShowCardConfirmButton()
    {
        CardSelection.Instance.showConfirmCardButton();
    }

    private bool gameHasItem()
    {
        return players.Count > 0;
    }

    private void spawnTheLoot()
    {
        GameObject startSpot = board.getOpenTokenPosition(Vector2Int.zero);
        LootToken = Instantiate(LootPrefab, startSpot.transform.position, Quaternion.identity).GetComponent<Token>();
        LootToken.setBoardPosition(Vector2Int.zero, startSpot);
        board.addToken(LootToken, true);
        NetworkServer.Spawn(LootToken.gameObject);
        if (gameHasItem())
        {
            GameObject itemSpot = board.getOpenTokenPosition(Vector2Int.zero);
            ItemToken = Instantiate(ItemPrefab, itemSpot.transform.position, Quaternion.identity).GetComponent<Token>();
            ItemToken.setBoardPosition(Vector2Int.zero, itemSpot);
            board.addToken(ItemToken);
            NetworkServer.Spawn(ItemToken.gameObject);

            //TODO: associate an Ability with the Item

        }
        playersCardChoices = new Dictionary<int, CardPlay>();
    }

    public void getPlayerMoveChoice(NetworkConnection connection, Card card)
    {
        playersCardChoices[connection.connectionId] = new CardPlay(card);
        if (playersCardChoices.Count >= players.Count)
        {
            infoText = "Loot moving!";
            foreach(int playConnId in playerTurnOrder)
            {
                playerStatus[playConnId] = playersCardChoices[playConnId].mainCard.title;
                playersCardChoices[playConnId].mainCard.prefab.GetComponent<StandardMovementLogic>().moveLootAction(this, LootToken, gameHasItem() ? ItemToken : null);
            }
            StartCoroutine(waitForTokens());
        }
    }

    IEnumerator waitForTokens()
    {
        yield return new WaitForSeconds(1);

        bool tokensAnimating = true;
        while (tokensAnimating)
        {
            tokensAnimating = false;
            foreach(Token token in board.tokens)
            {
                if (token.isAnimating())
                {
                    tokensAnimating = true;
                    break;
                }
            }
            yield return null;
        }
        startPlaceExits();
    }

    #endregion

    #region Place Exits

    public void startPlaceExits()
    {
        infoText = "Placing Exits";
        board.RpcColorExteriorWalls(Color.white);
        board.setCheckTileMouseDown(true);
        currentPlayersTurn = playerTurnOrder[0];
        foreach(int key in playerTurnOrder)
        {
            if (key != currentPlayersTurn)
                changePlayerTurnExits(key, false);
        }
        changePlayerTurnExits(currentPlayersTurn, true);
        //TODO: Create Dock Model. Needs to be colourable to signify players (or add text)
    }

    private void changePlayerTurnExits(int playerKey, bool turn)
    {
        players[playerKey].setMyTurn(turn);
        if (turn)
            playerStatus[playerKey] = "Placing Dock";
        else
            playerStatus[playerKey] = "Waiting";
    }

    public void playerSelectsDock(Vector2Int gridPos, Vector2Int wallDir)
    {
        if(board.tryAddDock(gridPos, wallDir))
        {
            board.RpcAddDock(gridPos, wallDir);
            GameObject playerPosGO = board.getOpenTokenPosition(gridPos, true);
            Token playerToken = Instantiate(PlayerPrefab, playerPosGO.transform.position, Quaternion.identity).GetComponent<Token>();
            playerToken.setColor(players[currentPlayersTurn].myColor);
            players[currentPlayersTurn].myToken = playerToken;
            NetworkServer.Spawn(playerToken.gameObject);
            if (board.docks.Count >= players.Count)
            {
                //Go to the next phase
                infoText = "Game On";
                board.RpcResetExteriorWallsColor();
                return;
            }
            changePlayerTurnExits(currentPlayersTurn, false);
            currentPlayersTurn = playerTurnOrder[(playerTurnOrder.IndexOf(currentPlayersTurn) + 1) % playerTurnOrder.Count];
            changePlayerTurnExits(currentPlayersTurn, true);
        }
    }

    #endregion

    #region Card Action Functions

    public void moveToken(Token token, Vector2Int direction, bool force = false, bool noRotate = false)
    {
        Vector2Int dir = board.getBoardDirection(direction); // Get the board direction. Takes into account any compass rotations.
        GameTile tile = board.getTileAt(token.boardPosition);
        if (!force)
        {
            int sideCount = 0;
            while (!tile.isSideADoor(dir) && sideCount < 4 && !noRotate)
            {
                dir = VectorUtils.rotate90CW(dir);
                sideCount++;
            }

            if (!tile.isSideADoor(dir))
                dir = Vector2Int.zero;
        }

        if (board.hasTileAt(token.boardPosition + dir) && dir != Vector2Int.zero)
        {
            GameObject go = board.getOpenTokenPosition(token.boardPosition + dir);
            token.moveTo(go.transform.position);
            token.setBoardPosition(token.boardPosition + direction, go);
        }else if (token.canSpace)
        {
            // go to space
        }
    }

    #endregion
}
