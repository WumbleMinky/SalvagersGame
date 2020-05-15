using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ISelectableCard : MonoBehaviour, IPointerDownHandler
{
    protected bool interactable = true;
    protected bool clicked = false;
    public GameObject highlight;
    public Image image;
    public Color deactiveColor = Color.grey;
    public Color startingColor;

    protected abstract void onClickCall();
    protected abstract void onClickAwayCall();

    public void Awake()
    {
        startingColor = image.color;
    }

    private void Update()
    {
        if (interactable && Input.GetMouseButtonDown(0) && highlight.activeSelf)
        {
            if (!clicked)
                onClickAwayCall();
            else
                clicked = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (interactable)
        {
            onClickCall();
            clicked = true;
        }
    }

    public void SelectCard()
    {
        highlight.SetActive(true);
    }

    public void DeselectCard()
    {
        highlight.SetActive(false);
    }

    public void setInteractable(bool val)
    {
        interactable = val;
        if (val)
            image.color = startingColor;
        else
            image.color = deactiveColor;
    }
}
