using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileCard : ISelectableCard//, IPointerDownHandler
{

    public delegate void OnClick(TileLayout layout, GameObject cardGO);
    public event OnClick OnClickDelegate;

    public delegate void OnClickAway();
    public event OnClickAway OnClickAwayDelegate;

    public TileLayout layout;
    //private bool interactable = true;
    //public GameObject highlight;

    //private bool clicked = false;
    //public Image image;
    //public Color deactiveColor = Color.grey;
    //public Color startingColor;

    //public override void DeselectCard()
    //{
    //    highlight.SetActive(false);
    //}

    //public void setInteractable(bool val)
    //{
    //    interactable = val;
    //    if (val)
    //        image.color = startingColor;
    //    else
    //        image.color = deactiveColor;
    //}

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (interactable)
    //    {
    //        OnClickDelegate(layout, gameObject);
    //        clicked = true;
    //    }
            
    //}

    //public override void SelectCard()
    //{
    //    highlight.SetActive(true);
    //}

    //private void Awake()
    //{
    //    startingColor = image.color;
    //}

    protected override void onClickAwayCall()
    {
        OnClickAwayDelegate();
    }

    protected override void onClickCall()
    {
        OnClickDelegate(layout, gameObject);
    }

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0) && highlight.activeSelf)
    //    {
    //        if (!clicked)
    //            OnClickAwayDelegate();
    //        else
    //            clicked = false;
    //    }
    //}
}
