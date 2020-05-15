using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class PowerSelection : MonoBehaviour
{
    public delegate void OnConfirm(Card card);
    public static event OnConfirm onConfirmDelegate;

    CardObject selectedCard;
    Card selectedPower;

    CardObject powerA;
    CardObject powerB;

    public void addPowers(Card[] powerIndices)
    {
        powerA = powerIndices[0].createPrefab(transform).GetComponent<CardObject>();
        powerB = powerIndices[1].createPrefab(transform).GetComponent<CardObject>();

        powerA.OnClickDelegate += onClick;
        powerB.OnClickDelegate += onClick;
    }

    public void onClick(CardObject cardObj, Card card)
    {
        if (selectedCard != null)
            selectedCard.DeselectCard();
        selectedPower = card;
        selectedCard = cardObj;
        selectedCard.SelectCard();
    }

    public void confirmChoice()
    {
        onConfirmDelegate(selectedPower);
    }

    private void OnDisable()
    {
        powerA.OnClickDelegate -= onClick;
        powerB.OnClickDelegate -= onClick;
    }
}

