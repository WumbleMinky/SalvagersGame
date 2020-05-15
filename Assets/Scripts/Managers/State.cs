using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBoardState 
{

    public void Begin(GameManager gm)
    {
        gm.startGame();
    }

    public void doActions(GameManager gm)
    {
        
        //When ActionTrigger happens, doAction --> when player finishes turn, activate next player

        //if all players have played 5 tiles, trigger End()
    }

    public void End(GameManager gm)
    {
        
        //gm.setState(new PowerSelectState())

    }

}
