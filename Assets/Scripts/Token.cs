using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Token : NetworkBehaviour
{

    public Vector2Int boardPosition { get; private set; }
    public GameObject tileTokenPos { get; private set; }
    public bool canSpace = false;
    bool animating = false;
    float moveDuration = 2;
    List<Vector3> movementQueue = new List<Vector3>();
    [SyncVar] Color myColor;
    [SyncVar] bool customColor = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (customColor)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.material.color = myColor;
            }
        }

    }

    public void setBoardPosition(Vector2Int pos, GameObject tokenPos = null)
    {
        boardPosition = pos;
        if (tokenPos != null)
        {
            setTileTokenPos(tokenPos);
        }
    }

    public void setBoardPosition(GameTile tile)
    {
        boardPosition = tile.gridPos;
    }

    public void setTileTokenPos(GameObject tokenPos)
    {
        tileTokenPos = tokenPos;
    }

    public bool isAnimating()
    {
        return animating;
    }

    [ClientRpc]
    public void RpcUpdateParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
    }

    public void moveTo(Vector3 pos)
    {
        movementQueue.Add(pos);
        if (!animating)
        {
            StartCoroutine(moveEnumerator());
        }
    }

    public IEnumerator moveEnumerator()
    {
        animating = true;
        Vector3 fromPos;
        Vector3 pos;
        while(movementQueue.Count > 0)
        {
            fromPos = transform.position;
            pos = movementQueue[0];
            for (float t = 0; t < 1; t += Time.deltaTime / moveDuration)
            {
                transform.position = Vector3.Lerp(fromPos, pos, t);
                yield return null;
            }
            transform.position = pos;
            movementQueue.RemoveAt(0);
        }
        animating = false;
    }

    public void setColor(Color color)
    {
        myColor = color;
        customColor = true;
    }
}
