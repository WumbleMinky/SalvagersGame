using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class SpaceTile : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public delegate void SpaceClicked(Vector2Int coords);
    public static event SpaceClicked SpaceClickedDelegate;

    [SyncVar]
    public Vector2Int gridPos = Vector2Int.zero;

    public List<GameObject> tokenPositions;
    public List<Token> tokens = new List<Token>();

    private bool spaceWalking = false;

    public void removeToken(Token token)
    {
        tokens.Remove(token);
    }

    public void addToken(Token token)
    {
        if (!tokens.Contains(token))
            tokens.Add(token);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            GameBoard board = GameObject.FindObjectOfType<GameBoard>();
            if (board == null)
                return;
            transform.SetParent(board.gameObject.transform);
            board.clientAddSpaceTile(this);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (!isServer)
        {
            GameBoard board = GameObject.FindObjectOfType<GameBoard>();
            board?.clientRemoveSpaceTile(this);
        }
    }

    public void setSpaceWalking(bool enable)
    {
        spaceWalking = enable;
        GetComponent<BoxCollider>().enabled = enable;
        UIReference.Instance.ObjectSelector.deselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (spaceWalking)
            SpaceClickedDelegate?.Invoke(gridPos);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spaceWalking)
            UIReference.Instance.ObjectSelector.selectObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spaceWalking)
            UIReference.Instance.ObjectSelector.deselect();
    }
}
