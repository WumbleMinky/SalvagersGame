using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class TileWall : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    GameTile parentTile;
    public Vector2Int direction;
    public bool isExterior = false;
    public bool hasDock = false;
    public GameObject doorGO;
    public GameObject wallGO;
    private Collider myCollider;
    private Color startingColor;

    private void Start()
    {
        parentTile = transform.GetComponentInParent<GameTile>();
        myCollider = GetComponent<Collider>();
        startingColor = GetComponentInChildren<Renderer>().material.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (myCollider.enabled && isExterior)
        {
            parentTile.wallClicked(this);
        }
    }

    public void changeColor(Color color)
    {
        foreach (Renderer rend in getActive().GetComponentsInChildren<Renderer>())
        {
            rend.material.color = color;
        }
    }

    public void resetColor()
    {
        changeColor(startingColor);
    }

    public void enablePointerDown()
    {
        myCollider.enabled = true;
    }

    public void disablePointerDown()
    {
        myCollider.enabled = false;
        UIReference.Instance.ObjectSelector.deselect();
    }

    public void setDirection(Vector2Int dir)
    {
        direction = dir;
    }

    public void activateDoor()
    {
        doorGO.SetActive(true);
        wallGO.SetActive(false);
    }

    public void activateWall()
    {
        doorGO.SetActive(false);
        wallGO.SetActive(true);
    }

    public bool isDoor()
    {
        return doorGO.activeSelf;
    }

    public GameObject getActive()
    {
        if (isDoor())
            return doorGO;
        return wallGO;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myCollider.enabled)
            UIReference.Instance.ObjectSelector.selectObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (myCollider.enabled)
            UIReference.Instance.ObjectSelector.deselect();
    }
}
