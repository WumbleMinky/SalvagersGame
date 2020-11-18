using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
    List<TileLayout> tileDeck;
    List<Card> cardDeck;
    public GameBoard board;
    public PlayerManager pm;
    private ResourceContainer resources;

    [Header("Debug/Testing")]
    public bool skipBoardBuiling = false;
    public bool skipShipEvents = false;

    [Header("UI Components")]
    public Text infoTextField;
    public int totalPlayers = 0;
    private StartingBoardLayout startingBoardLayout;

    private int tilesLeftToPlace = 0;
    public PlayerData localPlayer;
    [SerializeField] private List<int> playerTurnOrder = new List<int>();

    private List<DroneData> drones = new List<DroneData>();
    private Dictionary<int, PlayerStatusText> inGamePlayerList = new Dictionary<int, PlayerStatusText>();
    private Dictionary<Card, CardLogic> cardLogicMap = new Dictionary<Card, CardLogic>();

    [SyncVar]
    private int currentPlayersTurn;

    [SyncVar(hook = nameof(updateInfoTextField))]
    public string infoText;

    private int ShipEventCounter = 6;
    private int chosenPowerCount = 0;
    private Token LootToken;
    private Token ItemToken;

    [Header("Prefabs")]
    public GameObject LootPrefab;
    public GameObject ItemPrefab;
    public GameObject PlayerPrefab;
    public GameObject DronePrefab;
    public GameObject diceRollText;

    private event Action<int[]> diceAnimFinished;

    //Conflicts
    Queue<Vector2Int> fightPositions = new Queue<Vector2Int>();
    ConflictManager conflictManager = new ConflictManager();

    private void Awake()
    {
        pm.getStatusDict().Callback += playerStatusUpdated;
        resources = ResourceContainer.Instance;
    }

    private void OnDestroy()
    {
        pm.getStatusDict().Callback -= playerStatusUpdated;
    }

    private void playerStatusUpdated(SyncNamedStatusDict.Operation op, int key, string value)
    {
        if (op == SyncNamedStatusDict.Operation.OP_ADD)
        {
            GameObject go = Instantiate(UIReference.Instance.playerStatusTextPrefab, UIReference.Instance.playerStatusPanel.transform);
            PlayerStatusText pst = go.GetComponent<PlayerStatusText>();
            pst.text = value;
            pst.color = pm.getPlayerColor(key);
            inGamePlayerList.Add(key, pst);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.position -= rt.up * ((rt.rect.height * rt.localScale.y + 5) * (inGamePlayerList.Count - 1));
        }
        else if (op == SyncNamedStatusDict.Operation.OP_SET)
        {
            inGamePlayerList[key].text = value;
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
        PlayerData pd = playerObject.GetComponent<PlayerData>();
        pm.addPlayer(connId, pd);
        if (pd.isLocalPlayer)
            localPlayer = pd;
        playerTurnOrder.Add(connId);
        if (isServer && pm.Count == totalPlayers)
        {
            infoText = "Waiting for Players to Connect";
            StartCoroutine(waitForPlayerConnection());
        }

    }

    IEnumerator waitForPlayerConnection()
    {
        float maxTime = 10;
        float waited = 0;
        bool allConnected = false;
        //Wait for all the players to be connected or for 10 seconds and then start the game.
        while (waited < maxTime && !allConnected)
        {
            yield return new WaitForSeconds(0.5f);
            allConnected = true;
            foreach (Player pd in pm.getPlayers())
            {
                if (pd.data.connectionToClient == null)
                {
                    allConnected = false;
                    break;
                }
            }

            waited += 0.5f;
        }
        startGame();
        yield return null;
    }

    public void startGame(bool force = false)
    {
        currentPlayersTurn = playerTurnOrder[0];
        initializeTileDeck();
        createStartingBoard();
        tileDeck = shuffleDeck<TileLayout>(tileDeck);

        if (skipBoardBuiling)
            playerFinishedPlacingTile();
        else
            dealTilesToPlayers();

        for (int i = 0; i < playerTurnOrder.Count; i++)
        {
            int pConnId = playerTurnOrder[i];
            pm.setStatus(pConnId, "Playing");
            changePlayerTurnTilePlacement(pConnId, pConnId == currentPlayersTurn);
        }


    }

    #endregion

    #region Make The Board


    public void setStartingBoard(int index)
    {
        if (index < 0) //Random Layout
        {
            int randomIndex = UnityEngine.Random.Range(0, resources.startingBoardLayouts.Count);
            startingBoardLayout = resources.startingBoardLayouts[randomIndex];
        }
        else
        {
            startingBoardLayout = resources.startingBoardLayouts[index];
        }
    }

    private void createStartingBoard()
    {
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
        pm.setTurn(playerKey, turn);
        if (turn)
        {
            pm.setStatus(playerKey, "Placing Tile");
        }
        else
        {
            pm.setStatus(playerKey, "Waiting");
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
            foreach (int id in pm.getIds())
            {
                if (!playerTiles.ContainsKey(id))
                    playerTiles.Add(id, new List<TileLayout>());
                playerTiles[id].Add(tileDeck[0]);
                tileDeck.RemoveAt(0);
            }
        }

        foreach (int id in pm.getIds())
        {
            pm.getPlayerData(0).TargetTileHandUpdated(NetworkServer.connections[id], playerTiles[id].ToArray());
            tilesLeftToPlace += 5;
        }

    }

    #endregion

    #region Deal Cards to Players

    private void mapCardLogic(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            cardLogicMap.Add(card, card.prefab.GetComponent<CardLogic>());
        }
    }

    [Server]
    private void providePlayerPowerChoices()
    {
        infoText = "Choose your Ability";

        cardDeck = new List<Card>(resources.powerCards);
        cardDeck = shuffleDeck<Card>(cardDeck);

        Dictionary<int, Card[]> powers = new Dictionary<int, Card[]>();

        int cardDeckIndex = 0;
        for (int i = 0; i < 2; i++)
        {
            foreach (int id in pm.getIds())
            {
                if (!powers.ContainsKey(id))
                    powers.Add(id, new Card[2]);
                powers[id][i] = cardDeck[cardDeckIndex];
                cardDeckIndex++;
            }
        }

        foreach (int connId in pm.getIds())
        {
            if (resources.testCard01 != null)
            {
                pm.getPlayerData(connId).TargetProvidedPowerChoices(NetworkServer.connections[connId], new Card[] { resources.testCard01, resources.testCard02 });
            }
            else
            {
                pm.getPlayerData(connId).TargetProvidedPowerChoices(NetworkServer.connections[connId], powers[connId]);
            }
            pm.setStatus(connId, "Selecting Ability");
        }
    }

    [Server]
    public void playerSelectedPower(int connId, Card card)
    {
        cardDeck.Remove(card);
        List<Card> playersHand = new List<Card>();
        playersHand.Add(card);
        playersHand.AddRange(new List<Card>(ResourceContainer.Instance.standardCards));
        pm.getPlayerData(connId).TargetProvideCards(NetworkServer.connections[connId], playersHand.ToArray());
        pm.setStatus(connId, "Ability Chosen. Waiting.");
        chosenPowerCount++;

        if (chosenPowerCount >= pm.Count)
        {
            spawnTheLoot();
        }
    }

    #endregion

    #region Move the Loot

    private bool gameHasItem()
    {
        return pm.Count >= RuleManager.Instance.minPlayersForLoot;
    }

    private void spawnTheLoot()
    {
        LootToken = spawnToken(LootPrefab, true);
        if (gameHasItem())
        {
            ItemToken = spawnToken(ItemPrefab);
            //TODO: associate an Ability with the Item

        }
    }

    public void getPlayerMoveChoice(NetworkConnection connection, Card card)
    {
        pm.setActionCard(connection.connectionId, card);
        if (pm.allPlayersHaveChosenAction())
        {
            infoText = "Loot moving!";
            foreach (int playConnId in playerTurnOrder)
            {
                pm.setStatus(playConnId, pm.getPlayer(playConnId).actionCard.title);
                StandardMovementLogic logic = pm.getActionCard(playConnId).getLogic() as StandardMovementLogic;
                logic.moveLootAction(this, LootToken, gameHasItem() ? ItemToken : null);
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
            foreach (Token token in board.tokens)
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
        foreach (int key in playerTurnOrder)
        {
            if (key != currentPlayersTurn)
                changePlayerTurnExits(key, false);
        }
        changePlayerTurnExits(currentPlayersTurn, true);
    }

    private void changePlayerTurnExits(int playerKey, bool turn)
    {
        pm.setTurn(playerKey, turn);
        if (turn)
            pm.setStatus(playerKey, "Placing Dock");
        else
            pm.setStatus(playerKey, "Waiting");
    }

    public void playerSelectsDock(Vector2Int gridPos, Vector2Int wallDir)
    {
        if (board.tryAddDock(gridPos, wallDir))
        {
            board.RpcAddDock(gridPos, wallDir);
            GameObject playerPosGO = board.getOpenTokenPosition(gridPos, true);
            PlayerToken playerToken = spawnToken(PlayerPrefab, pm.getPlayerColor(currentPlayersTurn)) as PlayerToken;
            playerToken.transform.position = playerPosGO.transform.position;
            playerToken.setPlayerData(pm.getPlayerData(currentPlayersTurn));
            board.updateTokenPosition(playerToken, gridPos, playerPosGO);
            NetworkServer.Spawn(playerToken.gameObject);
            if (board.docks.Count >= pm.Count)
            {
                //Go to the next phase
                infoText = "Game On";
                board.RpcResetExteriorWallsColor();
                board.setCheckTileMouseDown(false);
                spawnDrones();
                triggerSelectActions();
                return;
            }
            changePlayerTurnExits(currentPlayersTurn, false);
            currentPlayersTurn = playerTurnOrder[(playerTurnOrder.IndexOf(currentPlayersTurn) + 1) % playerTurnOrder.Count];
            changePlayerTurnExits(currentPlayersTurn, true);
        }
    }

    #endregion

    private void spawnDrones()
    {
        int droneCount = RuleManager.Instance.minNumberOfDrones;
        if (pm.Count >= RuleManager.Instance.minPlayersForExtraDrone)
        {
            droneCount++;
        }
        for (int i = 0; i < droneCount; i++)
        {
            Token t = spawnToken(DronePrefab, resources.droneColors[i]);
            DroneData dd = new DroneData(-(i + 1));
            dd.setToken(t);
            drones.Add(dd);
        }
    }

    #region Select Actions

    private void triggerSelectActions()
    {
        pm.clearCardChoices();
        foreach (int key in pm.getIds())
        {
            if (pm.getPlayerData(key).stunned)
            {
                pm.getPlayerData(key).RpcDeselectActions();
                pm.setStatus(key, "Stunned");
                setPlayerAction(NetworkServer.connections[key], resources.noAction);
            }
            else if (pm.isSpaceWalking(key))
            {
                pm.getPlayerData(key).RpcDeselectActions();
                pm.setStatus(key, "Space Walking");
                setPlayerAction(NetworkServer.connections[key], resources.noAction);
            }
            else
            {
                pm.getPlayerData(key).RpcChooseActionPhase();
                pm.setStatus(key, "Choosing action");
            }
            
        }
        infoText = "Choose Actions";
    }

    public void actionSelected(NetworkConnection conn, Card card)
    {
        setPlayerAction(conn, card);
    }

    public void actionAndChoiceSelected(NetworkConnection conn, Card actionCard, Card choiceCard)
    {
        setPlayerAction(conn, actionCard, choiceCard);
    }

    private void setPlayerAction(NetworkConnection conn, Card actionCard, Card choiceCard = null)
    {
        pm.setActionCard(conn.connectionId, actionCard);
        pm.setChoiceCard(conn.connectionId, choiceCard);
        pm.setStatus(conn.connectionId, "Ready");
        if (pm.allPlayersHaveChosenAction())
        {
            infoText = "Actions Selected. Revealing Hold Cards";
            showHoldCards();
        }
    }

    #endregion

    #region Hold & Ship Event

    private void showHoldCards()
    {
        int holdCount = 0;
        foreach (int id in pm.getIds())
        {
            if (pm.getActionCard(id).title == "Hold")
            {
                pm.setStatus(id, "HOLD");
                holdCount++;
            }
        }
        if (skipShipEvents)
            moveDrones();
        else
        {
            StartCoroutine(delayedAction(1, () =>
            {
                rollForShipEventTrigger(holdCount);
            }));
        }
    }

    private void rollForShipEventTrigger(int holds)
    {
        infoText = "Check for Ship Event";
        int[] eventRoll = DiceUtil.rollDice(3 + holds);
        bool shipEventFound = DiceUtil.hasPairLessThan(eventRoll, ShipEventCounter);
        diceAnimFinished += shipEventTriggerFinished;
        RpcShowShipEventTriggerRoll(eventRoll, shipEventFound);
    }

    private void shipEventTriggerFinished(int[] outcome)
    {
        bool shipEventFound = DiceUtil.hasPairLessThan(outcome, ShipEventCounter);
        StartCoroutine(delayedAction(2f, () =>
        {
            diceAnimFinished -= shipEventTriggerFinished;
            if (shipEventFound)
                rollForShipEventAction();
            else
                moveDrones();
        }));
    }

    private void rollForShipEventAction()
    {
        infoText = "Roll for Ship Event";
        diceAnimFinished += shipEventActionFinished;
        int roll = DiceUtil.rollDie(12);
        RpcShowShipEventActionRoll(roll);
    }

    private void shipEventActionFinished(int[] outcome)
    {
        diceAnimFinished -= shipEventActionFinished;
        moveDrones();
    }

    [ClientRpc]
    private void RpcShowShipEventTriggerRoll(int[] vals, bool shipEventFound)
    {
        ShipEventPanel eventPanel = UIReference.Instance.shipEventPanel;
        eventPanel.activate();
        eventPanel.diceLayoutController.clear();
        List<Text> diceTexts = new List<Text>();
        foreach (int val in vals)
        {
            GameObject drt = Instantiate(diceRollText);
            Text text = drt.GetComponent<Text>();
            diceTexts.Add(text);
            drt.SetActive(false);
            eventPanel.diceLayoutController.addItem(drt);
        }
        StartCoroutine(EventDiceAnimator(0.75f, diceTexts, vals, shipEventFound ? UIReference.Instance.incomingShipEvent : ""));
    }

    [ClientRpc]
    private void RpcShowShipEventActionRoll(int roll)
    {
        ShipEventPanel eventPanel = UIReference.Instance.shipEventPanel;
        eventPanel.diceLayoutController.clear();
        List<Text> diceTexts = new List<Text>();
        GameObject drt = Instantiate(diceRollText);
        Text text = drt.GetComponent<Text>();
        diceTexts.Add(text);
        eventPanel.diceLayoutController.addItem(drt);

        StartCoroutine(EventDiceAnimator(1, diceTexts, new int[] { roll }, UIReference.Instance.shipEvent01));
    }

    IEnumerator EventDiceAnimator(float totalSeconds, List<Text> diceTexts, int[] diceVals, string displayText)
    {
        float timeCount;
        float animTime = 0.025f;
        for (int i = 0; i < diceVals.Length; i++)
        {
            if (diceTexts[i] == null)
                continue;
            timeCount = 0;
            diceTexts[i].gameObject.SetActive(true);
            while (timeCount < totalSeconds)
            {
                diceTexts[i].text = UnityEngine.Random.Range(1, 7).ToString();
                yield return new WaitForSeconds(animTime);
                timeCount += animTime;
            }
            diceTexts[i].text = diceVals[i].ToString();
            yield return null;
        }
        UIReference.Instance.shipEventPanel.text = displayText;
        if (isServer)
            diceAnimFinished?.Invoke(diceVals);
    }

    #endregion

    #region Drone Movement

    private void moveDrones()
    {
        infoText = "Drone Movement";
        RpcHideShipEvent();
        moveSingleDrone(0);
    }

    [ClientRpc]
    private void RpcHideShipEvent()
    {
        UIReference.Instance.shipEventPanel.deactivate();
    }

    private void moveSingleDrone(int index)
    {
        if (index >= drones.Count)
        {
            checkSpaceWalkers(); //Go to the next Phase
            return;
        }
        if (drones[index].stunned)
        {
            moveSingleDrone(index + 1);
            return;
        }
        Token droneToken = drones[index].getToken();

        if (drones[index].getCaughtPlayer() != null)
        {
            moveTowardsPoint(droneToken, Vector2Int.zero);
        }
        else
        {
            int droneAction = UnityEngine.Random.Range(0, 6);
            drones[index].action = (DroneData.Actions)droneAction;
            switch (droneAction)
            {
                case 0:
                    moveToken(droneToken, board.FORWARD);
                    break;
                case 1:
                    moveToken(droneToken, board.RIGHT);
                    break;
                case 2:
                    moveToken(droneToken, board.BACK);
                    break;
                case 3:
                    moveToken(droneToken, board.LEFT);
                    break;
            }
        }
        StartCoroutine(droneMoving(droneToken, index + 1));
    }

    IEnumerator droneMoving(Token droneToken, int nextIndex)
    {
        while (droneToken.isAnimating())
        {
            yield return new WaitForSeconds(0.25f);

        }
        //TODO: Pause here to give Hijack a chance to work

        moveSingleDrone(nextIndex);
    }

    #endregion

    #region Player Actions

    private void checkSpaceWalkers()
    {
        if (pm.anyoneSpaceWalking())
        {
            infoText = "Space Walking";

            foreach(int id in playerTurnOrder)
            {
                if (pm.getSpaceCount(id) == 2)
                {
                    pm.setStatus(id, "Choosing Entry Point");
                    pm.getPlayerData(id).TargetSpaceWalkingChooseEntry();
                    return;
                }
            }
        }
        playerActions();
    }

    public void selectSpaceEntry(int playerId, Vector2Int pos)
    {
        GameObject obj = board.getOpenSpaceTokenPosition(pos);
        pm.getToken(playerId).moveTo(obj.transform.position);
        board.updateTokenPosition(pm.getToken(playerId), pos, obj);
        int index = playerTurnOrder.IndexOf(playerId);

        for(int i = index+1; i < playerTurnOrder.Count; i++)
        {
            if (pm.getSpaceCount(playerTurnOrder[i]) == 2)
            {
                pm.setStatus(playerTurnOrder[i], "Choosing Entry Point");
                pm.getPlayerData(playerTurnOrder[i]).TargetSpaceWalkingChooseEntry();
                return;
            }
        }
        playerActions();
    }

    IEnumerator waitForSpaceWalkers()
    {
        yield return null;
    }

    private void playerActions()
    {
        infoText = "Player Actions";
        List<int> playerActionIds = new List<int>();
        foreach (int id in playerTurnOrder)
        {
            if (resources.standardCards.Contains(pm.getActionCard(id)))
                playerActionIds.Add(id);
        }

        List<DroneData> specialDroneActions = new List<DroneData>();
        foreach(DroneData dd in drones)
        {
            if (dd.action >= DroneData.Actions.LOOT)
                specialDroneActions.Add(dd);
        }

        List<int> abilityActionOrder = new List<int>();
        foreach (int id in playerTurnOrder)
        {
            if (!resources.standardCards.Contains(pm.getActionCard(id)) && pm.getActionCard(id) != resources.noAction)
                abilityActionOrder.Add(id);
        }

        StartCoroutine(waitForPlayerActions(playerActionIds, specialDroneActions, abilityActionOrder));
    }

    IEnumerator waitForPlayerActions(List<int> actionOrder, List<DroneData> droneActions, List<int> abilityOrder)
    {
        foreach (int id in actionOrder)  //Process the standard actions
        {
            Card action = pm.getActionCard(id);
            if (action == resources.noAction)
                continue;
            action.getLogic().performAction(this, pm.getPlayerData(id));//, action.choice ? pm.getChoiceCard(id) : null);
            while (pm.getToken(id).isAnimating())
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
        PlayerData pd = pm.getPlayerWithLoot();
        if (pd != null && pd.myToken.boardPosition == board.dockedShipPos)
        {
            playerWins(pd);
        }

        foreach(DroneData dd in droneActions)  //Process the Drone Loot and Radar rolls
        {
            if (dd.stunned)
                continue;
            switch (dd.action)
            {
                case DroneData.Actions.LOOT:
                    droneLoot(dd);
                    break;
                case DroneData.Actions.RADAR:
                    droneRadar(dd);
                    break;
            }

            while (dd.getToken().isAnimating())
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }
        foreach(int id in abilityOrder)  //Process the player ability actions
        {
            Card action = pm.getActionCard(id);
            if (action == resources.noAction)
                continue;
            action.getLogic().performAction(this, pm.getPlayerData(id), action.choice ? pm.getChoiceCard(id) : null);
            while (pm.getToken(id).isAnimating())
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }

        findConflicts();
    }

    #endregion

    #region Resolve Conflicts

    private void findConflicts()
    {
        infoText = "Resolve Conflicts";
        foreach (int id in pm.getIds())
        {
            if (!pm.getPlayerData(id).stunned)
            {
                Vector2Int position = pm.getToken(id).boardPosition;
                conflictManager.addPlayer(id, position, pm.getPlayerData(id).hasLoot);
            }
        }

        for (int i = 0; i < drones.Count; i++)
        {
            if (!drones[i].stunned)
                conflictManager.addDrone(DroneData.convertIndexToId(i), drones[i].getToken().boardPosition);
        }

        conflictManager.prepareConflicts();
        nextFight();
    }

    private void nextFight()
    {
        List<int> fightIds = conflictManager.nextFight();

        string combat1;
        string combat2;
        bool combat1Drone = false;
        bool combat2Drone = false;
        if (fightIds == null)
        {
            conflictManager.Clear();
            PlayerData.LocalPlayer.RpcHideConflictPanel();
            grabTheLoot();
            return;
        }

        if (fightIds[0] < 0)
        {
            combat1 = "Drone " + (-fightIds[0]);
            combat1Drone = true;
        }
        else
        {
            combat1 = pm.getPlayerData(fightIds[0]).playerName;
        }

        if (fightIds[1] < 0)
        {
            combat2Drone = true;
            combat2 = "Drone " + (-fightIds[1]);
        }
        else
        {
            combat2 = pm.getPlayerData(fightIds[1]).playerName;
        }

        PlayerData.LocalPlayer.RpcShowConflictPanel(combat1, combat1Drone, combat2, combat2Drone);
        if (!combat1Drone)
        {
            pm.getPlayerData(fightIds[0]).TargetShowDiceButton(NetworkServer.connections[fightIds[0]], ConflictPanel.LEFTSIDE);
        }
        if (!combat2Drone)
        {
            pm.getPlayerData(fightIds[1]).TargetShowDiceButton(NetworkServer.connections[fightIds[1]], ConflictPanel.RIGHTSIDE);
        }
    }

    public void rollConflict(int side)
    {
        int index = (side == ConflictPanel.LEFTSIDE ? 0 : 1);
        int otherIndex = (index + 1) % 2;
        List<int> fight = conflictManager.getCurrentFight();
        int[] dice;
        int[] droneRoll = DiceUtil.rollDice(4);// new int[] { 3, 2, 5, 5 };
        if (fight[index] < 0)
            dice = new int[] { 3, 2, 5, 5};//dice = DiceUtil.rollDice(4);
        else
            dice = DiceUtil.rollDice(3); //new int[] { 5, 5, 6 };

        if (side == ConflictPanel.LEFTSIDE)
        {
            conflictManager.fightRollLeft(new List<int>(dice));
            if (fight[otherIndex] < 0)
                conflictManager.fightRollRight(new List<int>(droneRoll));//DiceUtil.rollDice(4)));
        }
        else
        {
            conflictManager.fightRollRight(new List<int>(dice));
            if (fight[otherIndex] < 0)
                conflictManager.fightRollLeft(new List<int>(droneRoll));// DiceUtil.rollDice(4)));
        }

        if (fight[otherIndex] < 0)
            RpcDisplayConflictRoll(conflictManager.getLeftRoll().ToArray(), conflictManager.getRightRoll().ToArray());
        else
        {
            if (side == ConflictPanel.LEFTSIDE)
                RpcDisplayConflictRoll(conflictManager.getLeftRoll().ToArray(), null);
            else
                RpcDisplayConflictRoll(null, conflictManager.getRightRoll().ToArray());
        }

    }

    [ClientRpc]
    private void RpcDisplayConflictRoll(int[] diceValsLeft, int[] diceValsRight)
    {
        ConflictPanel cp = UIReference.Instance.conflictPanel;
        if (diceValsLeft != null)
            StartCoroutine(conflictDiceAnimator(3f / diceValsLeft.Length, cp.LeftDiceTexts, diceValsLeft, cp.LeftDiceTotal, ConflictPanel.LEFTSIDE));

        if (diceValsRight != null)
            StartCoroutine(conflictDiceAnimator(3f / diceValsRight.Length, cp.RightDiceTexts, diceValsRight, cp.RightDiceTotal, ConflictPanel.RIGHTSIDE));

    }

    private void diceAnimationComplete(int side)
    {
        ConflictPanel cp = UIReference.Instance.conflictPanel;
        if (side == ConflictPanel.LEFTSIDE)
            cp.leftAnimComplete = true;
        else
            cp.rightAnimComplete = true;
        if (cp.leftAnimComplete && cp.rightAnimComplete)
        {
            if (conflictManager.getLeftTotal() > conflictManager.getRightTotal())
                RpcConflictLost(ConflictPanel.RIGHTSIDE);
            else if (conflictManager.getLeftTotal() < conflictManager.getRightTotal())
                RpcConflictLost(ConflictPanel.LEFTSIDE);
        }
    }

    [ClientRpc]
    private void RpcConflictLost(int side)
    {
        UIReference.Instance.conflictPanel.displayLostImage(side);
    }

    private void checkConflictResult()
    {
        int winnerId = conflictManager.getWinnerId();
        if (winnerId == ConflictManager.NOT_OVER)
            return;

        if (winnerId != ConflictManager.TIED)
        {
            int loserId = conflictManager.getLoserId();
            if (loserId < 0)
                drones[DroneData.convertIdToIndex(loserId)].setStun(true);
            else
            {
                PlayerData pd = pm.getPlayerData(loserId);
                pd.stunned = true;

                if (pd.hasLoot)
                    dropLoot(pd);

                if (winnerId < 0)
                {
                    drones[DroneData.convertIdToIndex(winnerId)].catchPlayer(pd);
                }
            }
        }
        conflictManager.fightResults();
        // if loser has Loot, drop Loot
        nextFight();
    }

    IEnumerator conflictDiceAnimator(float secondsPerDie, Text[] diceTexts, int[] diceVals, Text diceTotalText, int side)
    {
        float timeCount;
        float animTime = 0.025f;
        int total = 0;
        for (int i = 0; i < diceVals.Length; i++)
        {
            total += diceVals[i];
            timeCount = 0;
            while (timeCount < secondsPerDie)
            {
                diceTexts[i].text = UnityEngine.Random.Range(1, 7).ToString();
                yield return new WaitForSeconds(animTime);
                timeCount += animTime;
            }
            diceTexts[i].text = diceVals[i].ToString();
            yield return null;
        }
        diceTotalText.text = total.ToString();
        if (isServer)
        {
            yield return new WaitForSeconds(0.5f);
            diceAnimationComplete(side);
            if (UIReference.Instance.conflictPanel.bothAnimsComplete())
            {
                yield return new WaitForSeconds(2);
                checkConflictResult();
            }
        }
    }

    #endregion

    #region Grab Loot & End of Turn

    private void grabTheLoot()
    {
        if (pm.getPlayerWithLoot() == null)
        {
            List<PlayerData> pds = pm.getPlayersAtPosition(LootToken.boardPosition);
            if (pds.Count > 0)
            {
                if (pds.Count > 1)
                {
                    List<PlayerData> unstunned = new List<PlayerData>();
                    foreach (PlayerData pd in pds)
                    {
                        if (!pd.stunned)
                            unstunned.Add(pd);
                    }
                    if (unstunned.Count == 1)
                        pickUpLoot(unstunned[0]);
                }
                else
                {
                    if (!pds[0].stunned)
                        pickUpLoot(pds[0]);
                }
            }
        }

        endOfTurn();
    }

    private void pickUpLoot(PlayerData player)
    {
        LootToken.setFollow(player.myToken.heldItemPosition);
        player.hasLoot = true;

        //IF this player did not call the current ship, or if there is no ship docked.
        Vector2Int pos = board.docks[UnityEngine.Random.Range(0, board.docks.Count)];
        board.spawnShipAt(pos);
    }

    private void dropLoot(PlayerData player)
    {
        player.hasLoot = false;
        GameObject pos = board.getOpenTokenPosition(player.myToken.boardPosition);
        LootToken.boardPosition = player.myToken.boardPosition;
        LootToken.setFollow(null);
        LootToken.transform.SetPositionAndRotation(pos.transform.position, Quaternion.identity);
    }

    private void endOfTurn()
    {

        rollForEscapes();

        updateStuns();

        triggerSelectActions();
    }

    private void rollForEscapes()
    {
        foreach (Player p in pm.getPlayers())
        {
            if (p.data.stunned && p.data.caught)
            {
                //roll to escape
            }
        }
    }

    private void updateStuns()
    {
        foreach (Player p in pm.getPlayers())
        {
            if (p.data.stunned)
            {
                if (p.data.stunCount >= 1)
                    p.data.stunned = false;
                else
                    p.data.stunCount++;
            }
            else if (p.data.myToken.spaceRounds > 0)
            {
                if (p.data.myToken.spaceRounds == 2)
                {
                    if (p.data.hasLoot)
                        p.data.myToken.spaceRounds = 1;
                    else
                        p.data.myToken.spaceRounds = 0;
                }
                else
                {
                    p.data.myToken.spaceRounds--;
                }
            }
        }

        foreach (DroneData dd in drones)
        {
            if (dd.stunned)
            {
                if (dd.stunCount >= 1)
                    dd.setStun(false);
                else
                    dd.stunCount++;
            }
        }
    }

    [ClientRpc]
    private void RpcResetStuff()
    {
        StopAllCoroutines();
    }

    #endregion

    #region Card & Drone Action Functions

    public void moveToken(Token token, Vector2Int direction, bool force = false, bool noRotate = false, bool isNormalMove = false)
    {
        if (direction.Equals(Vector2Int.zero))
            return;

        Vector2Int dir = board.getBoardDirection(direction); // Get the board direction. Takes into account any compass rotations.
        GameTile tile = board.getTileAt(token.boardPosition);

        PlayerData pd = null;
        if (token is PlayerToken)
        {
            pd = ((PlayerToken)token).getPlayer();
        }
            

        if (!force)
        {
            int sideCount = 0;
            while(sideCount < 4 && !noRotate)
            {
                bool tileAndDoor = board.hasTileAt(token.boardPosition + dir) && board.tileOpenInDir(token.boardPosition, dir);// tile.isSideADoor(dir);
                bool isSpaceAndCanSpace = token.canSpace && board.hasSpaceAt(token.boardPosition + dir) && board.tileOpenInDir(token.boardPosition, dir) && !board.hasSpaceAt(token.boardPosition); // tile.isSideADoor(dir);
                bool hasLootAndIsDock = pd != null && pd.hasLoot && board.dockedShipPos == (token.boardPosition + dir) && isNormalMove;
                
                if (tileAndDoor || isSpaceAndCanSpace || hasLootAndIsDock)
                    break;
                dir = VectorUtils.rotate90CW(dir);
                sideCount++;
            }
            
            //if (!board.tileOpenInDir(token.boardPosition, dir))//tile.isSideADoor(dir))
            //    dir = Vector2Int.zero;
        }

        if (dir == Vector2Int.zero)
            return;

        if (board.hasTileAt(token.boardPosition + dir) || (!token.canSpace && board.spaceCoords.Contains(token.boardPosition + dir) && !noRotate))
        {
            GameObject go = board.getOpenTokenPosition(token.boardPosition + dir);
            board.updateTokenPosition(token, token.boardPosition + dir, go);
            token.moveTo(go.transform.position);
        }
        else if (token.canSpace && board.spaceCoords.Contains(token.boardPosition + dir))
        {
            GameObject go = board.getOpenSpaceTokenPosition(token.boardPosition + dir);
            token.spaceRounds = 3;
            board.updateTokenPosition(token, token.boardPosition + dir, go);
            token.moveTo(go.transform.position);
        }
    }

    public void moveTowardsPoint(Token token, Vector2Int point)
    {
        if (!board.hasTileAt(point) && !board.hasSpaceAt(point))
            return;

        List<List<Vector2Int>> paths = board.getPathsToTile(token.boardPosition, point);
        if (paths.Count > 0)
        {
            List<Vector2Int> path = paths[UnityEngine.Random.Range(0, paths.Count)];
            Vector2Int dir;
            if (path.Count == 1)
                dir = path[0] - token.boardPosition;
            else
                dir = path[1] - token.boardPosition;
            moveToken(token, dir);
        }
    }

    public void droneLoot(DroneData drone)
    {
        moveTowardsPoint(drone.getToken(), LootToken.boardPosition);
    }

    public void droneRadar(DroneData drone)
    {
        int dist = int.MaxValue;
        List<Vector2Int> points = new List<Vector2Int>();
        PlayerData lootPlayer = pm.getPlayerWithLoot();
        if (lootPlayer != null)
        {
            moveTowardsPoint(drone.getToken(), lootPlayer.myToken.boardPosition);
            return;
        }

        foreach (int id in pm.getIds())
        {
            Vector2Int pos = pm.getPlayerPosition(id);
            int droneDist = board.distanceToTile(drone.getToken().boardPosition, pos);
            if (droneDist < dist)
            {
                points.Clear();
                dist = droneDist;
            }
            if (droneDist == dist)
                points.Add(pos);
        }
        if (points.Count > 0)
        {
            moveTowardsPoint(drone.getToken(), points[UnityEngine.Random.Range(0, points.Count)]);
        }
    }

    #endregion

    private void playerWins(PlayerData playerData)
    {
        Debug.Log("[GM] - PLAYER '" + playerData.playerName + "' just WON!" );
    }

    private Token spawnToken(GameObject prefab, bool isLoot = false)
    {
        Token token = instantiateToken(prefab);
        board.addToken(token, isLoot);
        NetworkServer.Spawn(token.gameObject);
        return token;
    }

    private Token spawnToken(GameObject prefab, Color color)
    {
        Token token = instantiateToken(prefab);
        token.setColor(color);
        board.addToken(token);
        NetworkServer.Spawn(token.gameObject);
        return token;
    }

    private Token instantiateToken(GameObject prefab)
    {
        GameObject startSpot = board.getOpenTokenPosition(Vector2Int.zero);
        Token token = Instantiate(prefab, startSpot.transform.position, Quaternion.identity).GetComponent<Token>();
        board.updateTokenPosition(token, Vector2Int.zero, startSpot);

        return token;
    }

    IEnumerator delayedAction(float seconds, UnityAction action)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
        yield return null;
    }
}
