using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerData : NetworkBehaviour
{
    public GameObject playerHandPanel;
    public GameObject tileEditPanel;
    public GameObject powerChoicePanel;
    public string playerName;

    [SyncVar]
    public bool ready = false;

    [SyncVar]
    int playerId;

    public GameObject[] tileHand = new GameObject[5];
    public List<GameObject> cardHand = new List<GameObject>();
    //player card hand

    //public GameObject selectedCard;
    private GameBoard board;
    private GameManager gm;
    bool myTurn = false;
    public bool cardFocused = false;

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
            board.placeTileOnGhost(ghostTile, layout);
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
            gm.nextPlayersTurn();
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
        //cardHand = new List<GameObject>();
        //foreach (Card c in cards)
        //{
        //    cardHand.Add(c.createPrefab(playerHandPanel.transform).gameObject);
        //}
        //LayoutRebuilder.ForceRebuildLayoutImmediate(playerHandPanel.GetComponent<RectTransform>());
        CardSelection.Instance.addCards(cards);
        CardSelection.Instance.enableOnlyMovementCards();
    }

    #endregion



   

}
