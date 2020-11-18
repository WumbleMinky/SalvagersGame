using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhaseLogic : CardLogic
{
    public override void performAction(GameManager gm, PlayerData pd, Card choiceCard = null)
    {
        if (choiceCard == null)
            return;
        
        if (choiceCard.getLogic() is StandardMovementLogic sml)
        {
            gm.moveToken(pd.myToken, sml.direction, true);
        }
        
    }
}
