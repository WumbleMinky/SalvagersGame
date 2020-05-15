using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Token : NetworkBehaviour
{

    Vector2Int boardPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setBoardPosition(Vector2Int pos)
    {
        boardPosition = pos;
    }

    public void setBoardPosition(GameTile tile)
    {
        boardPosition = tile.gridPos;
    }

    [ClientRpc]
    public void RpcUpdateParent(GameObject newParent)
    {
        Debug.Log("[Token] " + newParent);
        transform.SetParent(newParent.transform);
    }
}
