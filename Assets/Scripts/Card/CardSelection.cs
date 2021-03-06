﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CardSelection : MonoBehaviour
{

    public static CardSelection _inst;
    public static CardSelection Instance { get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<CardSelection>();
            return _inst;
        } 
    }

    public delegate void OnTilePlayed(GameObject ghostTile, TileLayout layout);
    public static event OnTilePlayed onTilePlayedDelegate;

    public delegate void OnTileConfirmed();
    public static event OnTileConfirmed onTileConfirmedDelegate;

    public delegate void NoCardSelected();
    public static event NoCardSelected NoCardSelectedDelegate;

    public delegate void CardSelected(Card card);
    public static event CardSelected CardSelectedDelegate;

    public GameObject previewPanel;
    public GameObject behindPreviewPanel;
    public GameObject confirmCardButton;

    TileLayout selectedLayout;
    GameObject selectedCardGO;
    Card selectedCard;
    public bool _preventDeselect = false;
    int cardCount = 0;

    public GameBoard board;

    private List<CardObject> cardsInHand = new List<CardObject>();

    #region Card Selection

    public void addCards(Card[] cards)
    {
        foreach(Card c in cards)
        {
            GameObject go = c.createPrefab(transform);
            CardObject cardObj = go.GetComponent<CardObject>();
            cardObj.OnClickDelegate += OnClickCard;
            cardObj.OnClickAwayDelegate += OnClickCardAway;
            cardsInHand.Add(cardObj);
            //TODO: Need to remove these delegates at some point
        }
    }

    private void OnDestroy()
    {
        foreach(CardObject c in cardsInHand)
        {
            c.OnClickDelegate -= OnClickCard;
            c.OnClickAwayDelegate -= OnClickCardAway;
        }
    }

    public void showConfirmCardButton()
    {
        confirmCardButton.SetActive(true);
    }

    public void hideConfirmCardButton()
    {
        confirmCardButton.SetActive(false);
    }

    public void OnClickCard(CardObject cardObj, Card card)
    {
        if (selectedCardGO != null)
            selectedCardGO.GetComponent<CardObject>().DeselectCard();
        selectedCard = card;
        selectedCardGO = cardObj.gameObject;
        cardObj.SelectCard();
        previewCard(cardObj);
        showConfirmCardButton();
    }

    public void OnClickCardAway()
    {
        if (selectedCardGO != null)
        {
            //Do not unselect over the confirm button
            RectTransform rt = confirmCardButton.GetComponent<RectTransform>();
            Vector2 localRT = rt.InverseTransformPoint(Input.mousePosition);
            if (!rt.rect.Contains(localRT))
            {
                deselectCards();
                hideConfirmCardButton();
            }
        }
    }

    public void previewCard(CardObject cardObj)
    {
        previewPanel.SetActive(true);
        previewPanel.GetComponent<Image>().sprite = cardObj.image.sprite;
    }

    public void movePreviewBehind()
    {
        behindPreviewPanel.SetActive(true);
        behindPreviewPanel.GetComponent<Image>().sprite = previewPanel.GetComponent<Image>().sprite;
    }

    public void confirmCardSelection()
    {
        if (selectedCard == null)
            return;
        disableAllCards();
        hideConfirmCardButton();
        CardSelectedDelegate?.Invoke(selectedCard);
    }

    public void enableOnlyMovementCards(bool includeHold = false)
    {
        foreach(CardObject card in cardsInHand)
        {
            if (ResourceContainer.Instance.standardCards.Contains(card.card))
            {
                if (!includeHold && card.card.title.ToLower().Equals("hold"))
                {
                    card.setInteractable(false);
                }
                else
                {
                    card.setInteractable(true);
                }
            }
            else
            {
                card.setInteractable(false);
            }
        }
    }

    public void setCardInteractive(bool cardsEnabled)
    {
        foreach (CardObject card in cardsInHand)
        {
            card.setInteractable(cardsEnabled);
        }
    }

    public void disableAllCards()
    {
        setCardInteractive(false);
    }

    public void enableAllCards()
    {
        setCardInteractive(true);
    }

    public void deselectCards()
    {
        selectedCardGO?.GetComponent<CardObject>().DeselectCard();
        selectedCard = null;
        selectedCardGO = null;
        previewPanel.SetActive(false);
        behindPreviewPanel.SetActive(false);
    }

    #endregion

    #region Tile Selection

    public void addTileCards(TileLayout[] tiles)
    {
        foreach (TileLayout tile in tiles)
        {
            GameObject go = tile.createPrefab(transform);
            go.GetComponent<TileCard>().OnClickDelegate += onClickTileCard;
            go.GetComponent<TileCard>().OnClickAwayDelegate += deselectLayout;
            cardCount++;
        }

        GhostTile.onMouseOverDelegate += preventDeselect;
        GhostTile.onMouseExitDelegate += enableDeselect;
    }

    public void removeSelectedTileCard()
    {
        if (selectedCardGO != null)
        {
            selectedCardGO.GetComponent<TileCard>().OnClickDelegate -= onClickTileCard;
            Destroy(selectedCardGO);
            deselectLayout(true);
            cardCount--;
        }

        if (cardCount <= 0)
        {
            GhostTile.onMouseOverDelegate -= preventDeselect;
            GhostTile.onMouseExitDelegate -= enableDeselect;
        }
    }

    public void deselectLayout()
    {
        deselectLayout(false);
    }

    public void deselectLayout(bool force)
    {
        if (!_preventDeselect || force)
        {
            if (selectedCardGO != null)
                selectedCardGO.GetComponent<TileCard>().DeselectCard();
            selectedLayout = null;
            selectedCardGO = null;
            if (force)
                _preventDeselect = false;
        }
        if (selectedCardGO == null)
            triggerNoCardSelected();
    }

    public void onClickTileCard(TileLayout layout, GameObject cardGO)
    {
        if (board.editingTile)
            return;
        if (selectedCardGO != null)
            selectedCardGO.GetComponent<TileCard>().DeselectCard();
        selectedLayout = layout;
        selectedCardGO = cardGO;
        selectedCardGO.GetComponent<TileCard>().SelectCard();
    }

    public void placeTile(GameObject ghostTile)
    {
        if (selectedLayout != null)
        {
            onTilePlayedDelegate?.Invoke(ghostTile, selectedLayout);
        }
    }

    public void confirmTileChoice()
    {
        onTileConfirmedDelegate();
    }

    #endregion



    private void preventDeselect()
    {
        _preventDeselect = true;
    }

    private void enableDeselect()
    {
        if (!board.editingTile)
            _preventDeselect = false;
    }

    public void triggerNoCardSelected()
    {
        NoCardSelectedDelegate();
    }

}
