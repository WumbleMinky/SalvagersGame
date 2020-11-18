using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardObject))]
public class StandardMovementLogic : CardLogic
{
    public Vector2Int direction;
    private Card card;

    public override void performAction(GameManager gm, PlayerData pd, Card choiceCard = null)
    {
        card = GetComponent<CardObject>().card;
        gm.moveToken(pd.myToken, direction, false, false, true);
    }

    public void moveLootAction(GameManager gm, Token loot, Token item)
    {
        gm.moveToken(loot, direction, false, true);
        if (item != null)
        {
            float negDist = Vector2Int.Distance(item.boardPosition - direction, loot.boardPosition);
            float posDist = Vector2Int.Distance(item.boardPosition + direction, loot.boardPosition);
            if (posDist > negDist)
                gm.moveToken(item, direction, false, true);
            else
                gm.moveToken(item, -direction, false, true);
        }
            
    }
}
