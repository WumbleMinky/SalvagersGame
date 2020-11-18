using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Token : NetworkBehaviour
{
    [SyncVar] public Vector2Int boardPosition;// { get; private set; }

    public GameObject heldItemPosition;

    public GameObject tileTokenPos { get; private set; }
    public bool canSpace = false;
    public GameObject followObj;
    public GameObject[] colorableObjects;
    public GameObject stunObj;
    float moveDuration = 2;
    List<Vector3> movementQueue = new List<Vector3>();
    [SyncVar] Color myColor;
    [SyncVar] bool customColor = false;

    private float animTime = 0;
    Vector3 currentPosition;

    [SyncVar (hook = nameof(stunChanged))]
    public bool stunned = false;

    [SyncVar]
    public int spaceRounds = 0;

    public enum TokenType
    {
        player,
        drone,
        loot,
        item,
        other
    };

    public TokenType type;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (customColor)
        {
            foreach (GameObject obj in colorableObjects)
            {
                obj.GetComponent<Renderer>().material.color = myColor;
            }
        }
        currentPosition = transform.position;
    }

    private void Update()
    {
        if (movementQueue.Count > 0)
        {
            animTime += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPosition, movementQueue[0], animTime);
            if ((transform.position - movementQueue[0]).sqrMagnitude <= 0.1)
            {
                transform.position = movementQueue[0];
                currentPosition = transform.position;
                animTime = 0;
                movementQueue.RemoveAt(0);
            }
        }

        if (followObj != null)
            transform.position = followObj.transform.position;
    }

    private void stunChanged(bool oldVal, bool newVal)
    {
        stunObj?.SetActive(newVal);
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
        return movementQueue.Count > 0;
    }

    [ClientRpc]
    public void RpcUpdateParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
    }

    public void setFollow(GameObject obj)
    {
        followObj = obj;
    }

    public void moveTo(Vector3 pos)
    {
        movementQueue.Add(pos);
    }

    public void setColor(Color color)
    {
        myColor = color;
        customColor = true;
    }

    public GameObject getHeldItemPosition()
    {
        return heldItemPosition;
    }

}
