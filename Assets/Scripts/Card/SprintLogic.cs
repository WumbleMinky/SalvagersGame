using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardObject))]
public class SprintLogic : CardLogic
{
    public override void performAction(GameManager gm, PlayerData pd, Card choiceCard)
    {
        if (choiceCard == null)
            return; //TODO: raise error, this needs a choice Card
        StandardMovementLogic sml = choiceCard.getLogic() as StandardMovementLogic;
        if (sml == null)
            return; //CHoice card was not a movement card

        gm.moveToken(pd.myToken, sml.direction);
        gm.moveToken(pd.myToken, sml.direction);
    }
}
