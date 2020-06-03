using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardObject))]
public class StandardMovementLogic : CardLogic
{
    public Vector2Int direction;
    private Card card;

    public override void performAction(GameManager gm, PlayerData pd)
    {
        card = GetComponent<CardObject>().card;
        
        //check with GM if wall in direction
        //  - if wall found, rotate direction 90 deg CW and check again. Max 4 times.
        //  - if no opening found, no movement
        //if open, call GM.MoveToken in the given direction
        
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
