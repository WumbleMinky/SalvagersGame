using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerData : NetworkBehaviour
{
    public delegate void onLocalPlayerStart(PlayerData pd);
    public static event onLocalPlayerStart startLocalPlayerCallback;

    public GameObject playerHandPanel;
    public GameObject tileEditPanel;
    public GameObject powerChoicePanel;
    [SyncVar]
    public string playerName;
    [SyncVar(hook = nameof(colorChanged))] 
    public Color myColor = Color.white;

    [SyncVar]
    public bool ready = false;

    [SyncVar]
    int playerId;

    public GameObject[] tileHand = new GameObject[5];
    public List<GameObject> cardHand = new List<GameObject>();
    //player card hand

    //public GameObject selectedCard;
    private LobbyPlayerName lobbyName;
    private GameBoard board;
    private GameManager gm;
    bool myTurn = false;
    public bool cardFocused = false;
    public Token myToken;

    private void Awake()
    {
        playerHandPanel = UIReference.Instance.playerHandPanel;
        tileEditPanel = UIReference.Instance.tileEditPanel;
        powerChoicePanel = UIReference.Instance.powerSelectionPanel;
    }
    void Start()
    {
        board = GameObject.FindObjectOfType<GameBoard>();
        gm = GameObject.FindObjectOfType<GameManager>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        startLocalPlayerCallback?.Invoke(this);
    }

    [Command]
    public void CmdChangeColor(Color color)
    {
        if (!gm.isColorTaken(color))
        {
            myColor = color;
        }
    }

    private void colorChanged(Color oldColor, Color newColor)
    {
        if (lobbyName != null)
        {
            lobbyName.transform.GetComponentInChildren<Image>().color = myColor;
            
        }
            
    }

    #region Turn Control

    [Server]
    public void setMyTurn(bool turn)
    {
        myTurn = turn;
        TargetSetMyTurn(connectionToClient, turn);
    }

    [TargetRpc]
    public void TargetSetMyTurn(NetworkConnection target, bool turn)
    {
        myTurn = turn;
        foreach (TileCard tc in playerHandPanel.GetComponentsInChildren<TileCard>())  //Changed this from TileCardButton To TileCard.
        {
            tc.setInteractable(myTurn);
        }
        tileEditPanel.SetActive(false);
    }

    #endregion

    #region Lobby

    [ClientRpc]
    public void RpcHideStartButton()
    {
        UIReference.Instance.startGameButton.SetActive(false);
    }

    [Command]
    public void CmdToggleReadyState()
    {
        ready = !ready;
        gm.readyClick(connectionToClient, ready, playerName);
    }

    public void createLobbyName(int position, string value)
    {
        lobbyName = Instantiate(UIReference.Instance.playerLobbyNamePrefab, UIReference.Instance.lobbyNamePanel.transform).GetComponent<LobbyPlayerName>();
        lobbyName.text = value;
        lobbyName.transform.GetComponentInChildren<Image>().color = myColor;
        RectTransform t = lobbyName.gameObject.GetComponent<RectTransform>();
        t.position -= t.up * t.rect.height * (position);
    }

    public void updateLobbyName(string value)
    {
        lobbyName.text = value;
    }

    #endregion

    #region Tiles

    [Client]
    public bool isEditingTile()
    {
        return board.editingTile;
    }

    [TargetRpc]
    public void TargetTileHandUpdated(NetworkConnection target, TileLayout[] hand)
    {
        CardSelection.Instance.addTileCards(hand);
        CardSelection.onTilePlayedDelegate += tilePlaced;  //TODO: replace this back with CmdTilePlaced...maybe
        CardSelection.onTilePlayedDelegate += ShowEditTilePanel;
        CardSelection.onTileConfirmedDelegate += CmdConfirmTile;
        CardSelection.NoCardSelectedDelegate += HideEditTilePanel;
    }

    public void tilePlaced(GameObject ghostTile, TileLayout layout)
    {
        if (myTurn)
        {
            CmdTilePlaced(ghostTile, layout);
        }
        
    }

    [Command]
    public void CmdTilePlaced(GameObject ghostTile, TileLayout layout)
    {
        if (myTurn)
        {
            board.placeTileOnGhost(ghostTile, layout);
            TargetSelectObject(board.unconfirmedTile.gameObject);
        }
    }

    [TargetRpc]
    public void TargetSelectObject(GameObject obj)
    {
        if (obj == null)
        {
            UIReference.Instance.ObjectSelector.deselect();
        }
        else
        {
            UIReference.Instance.ObjectSelector.selectObject(obj);
        }
    }

    [Client]
    public void ShowEditTilePanel(GameObject ghostTile, TileLayout layout)
    {
        if (myTurn)
            tileEditPanel.SetActive(true);
    }

    public void HideEditTilePanel()
    {
        tileEditPanel.SetActive(false);
    }

    [Command]
    public void CmdRotateTile()
    {
        board.rotateUnconfirmedTile();
    }

    [Command]
    public void CmdConfirmTile()
    {
        if (board.confirmTile())
        {
            RpcRemoveTileFromHand();
            TargetSelectObject(null);
            gm.playerFinishedPlacingTile();
        }
    }

    [ClientRpc]
    public void RpcRemoveTileFromHand()
    {
        CardSelection.Instance.removeSelectedTileCard();
    }

    [Command]
    public void CmdCancelTile()
    {
        board.cancelUnconfirmedTile();
        TargetSelectObject(null);
    }

    #endregion

    #region Power Selection & Player Cards

    [Command]
    public void CmdChoosePowerCard(Card card)
    {
        gm.playerSelectedPower(netIdentity.connectionToClient.connectionId, card);
    }

    [TargetRpc]
    public void TargetProvidedPowerChoices(NetworkConnection connection, Card[] powers)
    {
        CardSelection.onTilePlayedDelegate -= tilePlaced;
        CardSelection.onTilePlayedDelegate -= ShowEditTilePanel;
        CardSelection.onTileConfirmedDelegate -= CmdConfirmTile;
        CardSelection.NoCardSelectedDelegate -= HideEditTilePanel;

        powerChoicePanel.transform.parent.gameObject.SetActive(true);
        powerChoicePanel.GetComponent<PowerSelection>().addPowers(powers);
        PowerSelection.onConfirmDelegate += CmdChoosePowerCard;
        PowerSelection.onConfirmDelegate += hidePowerSelection;

        GameTile.WallSelectDelegate += wallSelected;
    }

    private void hidePowerSelection(Card card)
    {
        PowerSelection.onConfirmDelegate -= hidePowerSelection;
        PowerSelection.onConfirmDelegate -= CmdChoosePowerCard;

        powerChoicePanel.transform.parent.gameObject.SetActive(false);
    }

    [TargetRpc]
    public void TargetProvideCards(NetworkConnection connection, Card[] cards)
    {
        CardSelection.Instance.addCards(cards);
        CardSelection.Instance.enableOnlyMovementCards();
        CardSelection.CardSelectedDelegate += CmdLootMoveCardConfirmed;
    }

    [Command]
    private void CmdLootMoveCardConfirmed(Card card)
    {
        gm.getPlayerMoveChoice(connectionToClient, card);
    }

    #endregion

    private void wallSelected(GameTile tile, TileWall wall)
    {
        //Called via a Delegate on the TileWall class. Passes the data to the server for verification then passed back by RPC for visuals.
        if (myTurn)
            CmdWallSelected(tile.gridPos, wall.direction);
    }

    [Command]
    private void CmdWallSelected(Vector2Int tilePos, Vector2Int wallDirection)
    {
        gm.playerSelectsDock(tilePos, wallDirection);
    }


    [ClientRpc]
    public void RpcColourWall(Vector2Int tilePos, Vector2Int wallDirection)
    {
        board.getTileAt(tilePos).tileSides[wallDirection].changeColor(Color.red);
    }

}
