using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LootToken : Token
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        GameObject.FindObjectOfType<GameBoard>().LootToken = this;
    }

    
}
