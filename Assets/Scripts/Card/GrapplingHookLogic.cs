using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookLogic : CardLogic
{
    public override void performAction(GameManager gm, PlayerData pd, Card choiceCard = null)
    {
        if (choiceCard == null)
            return;

        if (choiceCard.getLogic() is StandardMovementLogic sml)
        {
            Vector2Int gridPos = pd.myToken.boardPosition;
            Vector2Int realDirection = gm.board.getBoardDirection(sml.direction);
            GameTile tile = gm.board.adjacentTile(gridPos, sml.direction);
            Token enemyToken = null;
            bool wallHit = false;
            while (tile != null) //Find the furtherst wall in the chosen direction
            {
                if (!gm.board.hasTileAt(gridPos))
                    break;

                List<Token> enemyTokens = tile.getPlayerAndDroneTokens();
                if (enemyTokens.Count > 0 && tile.gridPos != pd.myToken.boardPosition)
                {
                    enemyToken = enemyTokens[UnityEngine.Random.Range(0, enemyTokens.Count)];
                    break;
                }

                if (!tile.isSideADoor(realDirection))
                {
                    wallHit = true;
                    break;
                }
                gridPos = gridPos + realDirection;
                tile = gm.board.adjacentTile(gridPos, sml.direction);
            }

            if (enemyToken != null)
            {
                int distance = gm.board.distanceToTile(enemyToken.boardPosition, pd.myToken.boardPosition);
                if (distance >= 2)
                {
                    gm.moveToken(enemyToken, -sml.direction, false, true);
                    gm.moveToken(pd.myToken, sml.direction, false, true);
                    return;
                }
                else if (distance == 1)
                {
                    gm.moveToken(pd.myToken, sml.direction, false, true);
                    return;
                }
            }
            if (wallHit)
            {
                gm.moveToken(pd.myToken, sml.direction, false, true);
                gm.moveToken(pd.myToken, sml.direction, false, true);
            }
        }
    }
}
