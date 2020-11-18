using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;
using UnityEngine.EventSystems;

public class GameTile : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public delegate void WallSelect(GameTile tile, TileWall wall);
    public static event WallSelect WallSelectDelegate;

    public GameBoard board;
    public float rotateDuration = 0.5f;
    public Dictionary<Vector2Int, TileWall> tileSides;
    private bool rotating = false;

    [SyncVar(hook = nameof(updateLayout))]
    public TileLayout layout = null;
    [SyncVar]
    public Vector2Int gridPos = Vector2Int.zero;

    [SyncVar(hook = nameof(updateValidity))]
    public bool invalid;

    public GameObject invalidImage;
    public GameObject jailObject;
    public GameObject dock;

    public List<GameObject> tokenPositions;
    public List<GameObject> jailTokenPositions;

    public List<Token> tokens = new List<Token>();

    private Collider myCollider;

    void Awake()
    {
        tileSides = new Dictionary<Vector2Int, TileWall>();
        tileSides.Add(Vector2Int.left, transform.Find("Left Side").GetComponent<TileWall>());
        tileSides.Add(Vector2Int.up, transform.Find("Forward Side").GetComponent<TileWall>());
        tileSides.Add(Vector2Int.right, transform.Find("Right Side").GetComponent<TileWall>());
        tileSides.Add(Vector2Int.down, transform.Find("Back Side").GetComponent<TileWall>());
        myCollider = GetComponent<Collider>();
    }

    public List<PlayerToken> getPlayerTokens()
    {
        List<PlayerToken> pTokens = new List<PlayerToken>();
        foreach (PlayerToken t in tokens)
        {
            if (t.type == Token.TokenType.player)
                pTokens.Add(t);
        }
        return pTokens;
    }

    public List<Token> getDroneTokens()
    {
        List<Token> pTokens = new List<Token>();
        foreach (Token t in tokens)
        {
            if (t.type == Token.TokenType.drone)
                pTokens.Add(t);
        }
        return pTokens;
    }

    public List<Token> getPlayerAndDroneTokens()
    {
        List<Token> pTokens = new List<Token>();
        foreach (Token t in tokens)
        {
            if (t.type == Token.TokenType.player || t.type == Token.TokenType.drone)
                pTokens.Add(t);
        }
        return pTokens;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            board = GameObject.FindObjectOfType<GameBoard>();
            transform.SetParent(board.gameObject.transform);
            board.clientAddTile(this);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (!isServer)
        {
            board.clientRemoveTile(gridPos);
        }
    }

    #region Initial Creation

    public void updateLayout(TileLayout oldLayout, TileLayout newLayout)
    {
        foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down })
        {
            if (newLayout.doorDirections.Contains(dir))
                setDoor(dir);
            else
                setWall(dir);
        }
        if (newLayout.name.Equals("Security Station Tile"))
        {
            jailObject.SetActive(true);
        }
    }

    public void rotate90CW(bool animateRotation = true)
    {
        if (rotating)
            return;
        
        Dictionary<Vector2Int, TileWall> newDict = new Dictionary<Vector2Int, TileWall>();
        foreach (Vector2Int dir in tileSides.Keys)
        {
            TileWall tw = tileSides[dir];
            Vector2Int rotDir = VectorUtils.rotate90CW(dir);
            tw.setDirection(rotDir);
            newDict.Add(rotDir, tw);
        }
        tileSides = newDict;
        RpcRotateWallDirections();
        if (animateRotation)
            StartCoroutine(RotateMe());
        else
            transform.rotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * 90);
    }

    [ClientRpc]
    private void RpcRotateWallDirections()
    {
        if (isServer)
            return;
        Dictionary<Vector2Int, TileWall> newDict = new Dictionary<Vector2Int, TileWall>();
        foreach (Vector2Int dir in tileSides.Keys)
        {
            TileWall tw = tileSides[dir];
            Vector2Int rotDir = VectorUtils.rotate90CW(dir);
            tw.setDirection(rotDir);
            newDict.Add(rotDir, tw);
        }
        tileSides = newDict;
    }

    IEnumerator RotateMe()
    {
        rotating = true;
        Quaternion fromAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + Vector3.up * 90);
        for (float t = 0; t < 1; t += Time.deltaTime / rotateDuration)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        transform.rotation = toAngle;
        rotating = false;
    }

    public void updateValidity(bool oldValue, bool newValue)
    {
        invalidImage.transform.parent.gameObject.SetActive(newValue);
        invalidImage.SetActive(newValue);
    }

    #endregion

    public void removeToken(Token token)
    {
        tokens.Remove(token);
    }

    public void addToken(Token token)
    {
        if (!tokens.Contains(token))
            tokens.Add(token);
    }

    public void setDoor(Vector2Int direction)
    {
        tileSides[direction].activateDoor();
    }

    public void setWall(Vector2Int direction)
    {
        tileSides[direction].activateWall();
    }

    [ClientRpc]
    public void RpcSetWallAsExterior(Vector2Int direction, bool val)
    {
        tileSides[direction].isExterior = val;
    }

    public bool isSideADoor(Vector2Int dir)
    {
        return tileSides[dir].isDoor();
    }

    [ClientRpc]
    public void RpcEnableWallMouseDown()
    {
        foreach(TileWall tw in tileSides.Values)
        {
            tw.enablePointerDown();
        }
    }

    [ClientRpc]
    public void RpcDisableWallMouseDown()
    {
        foreach(TileWall tw in tileSides.Values)
        {
            tw.disablePointerDown();
        }
    }

    public void enablePointerEnter()
    {
        myCollider.enabled = true;
    }

    public void disablePointerEnter()
    {
        myCollider.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myCollider.enabled)
        {

        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (myCollider.enabled)
        {

        }
    }

    public void wallClicked(TileWall wall)
    {
        WallSelectDelegate?.Invoke(this, wall);
    }

    public void setDock(Vector2Int wallDir)
    {
        if (tileSides[wallDir].isExterior && !dock.activeSelf)
        {
            foreach(TileWall wall in tileSides.Values)
            {
                wall.resetColor();
                wall.disablePointerDown();
            }
            dock.SetActive(true);
            dock.transform.SetParent(tileSides[wallDir].transform);
            dock.transform.localRotation = Quaternion.identity;
            tileSides[wallDir].activateWall();
            tileSides[wallDir].hasDock = true;
        }
    }

    public Vector2Int getDockWallDir()
    {
        foreach(Vector2Int side in tileSides.Keys)
        {
            if (tileSides[side].hasDock)
                return side;
        }
        return Vector2Int.zero;
    }
}
