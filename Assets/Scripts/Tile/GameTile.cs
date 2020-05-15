using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class GameTile : NetworkBehaviour
{
    public float rotateDuration = 0.5f;
    public Dictionary<Vector2Int, Side> tileSides;
    private bool rotating = false;

    [SyncVar(hook =nameof(updateLayout))]
    public TileLayout layout = null;
    [SyncVar]
    public Vector2Int gridPos = Vector2Int.zero;

    [SyncVar(hook = nameof(updateValidity))]
    public bool invalid;

    public GameObject invalidImage;
    public GameObject jailObject;

    public List<GameObject> tokenPositions;
    public List<GameObject> jailTokenPositions;

    public struct Side
    {
        public GameObject Door;
        public GameObject Solidwall;

        public Side(Transform parent)
        {
            Door = null;
            Solidwall = null;
            foreach (Transform part in parent)
            {
                if (part.name.Equals("Solid Wall"))
                    Solidwall = part.gameObject;
                else if (part.name.Equals("Doorway"))
                    Door = part.gameObject;
            }
        }

        public bool isDoor()
        {
            return Door.activeInHierarchy;
        }
    }

    void Awake()
    {
        tileSides = new Dictionary<Vector2Int, Side>();
        tileSides.Add(Vector2Int.left, new Side(transform.Find("Left Side")));
        tileSides.Add(Vector2Int.up, new Side(transform.Find("Forward Side")));
        tileSides.Add(Vector2Int.right, new Side(transform.Find("Right Side")));
        tileSides.Add(Vector2Int.down, new Side(transform.Find("Back Side")));
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
        Dictionary<Vector2Int, Side> newDict = new Dictionary<Vector2Int, Side>();
        foreach (Vector2Int dir in tileSides.Keys)
        {
            newDict.Add(rotateDirectionCW(dir), tileSides[dir]);
        }
        tileSides = newDict;
        if (animateRotation)
            StartCoroutine(RotateMe());
        else
            transform.rotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * 90);
    }

    private Vector2Int rotateDirectionCW(Vector2Int direction)
    {
        return new Vector2Int(direction.y, -direction.x);
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

    [ClientRpc]
    public void RpcUpdateParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
    }

    #endregion



    public void setDoor(Vector2Int direction)
    {
        tileSides[direction].Door.SetActive(true);
        tileSides[direction].Solidwall.SetActive(false);
    }

    public void setWall(Vector2Int direction)
    {
        tileSides[direction].Door.SetActive(false);
        tileSides[direction].Solidwall.SetActive(true);
    }

    public bool isSideADoor(Vector2Int dir)
    {
        return tileSides[dir].isDoor();
    }

    
}
