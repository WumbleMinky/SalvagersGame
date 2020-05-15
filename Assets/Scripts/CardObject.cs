using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardObject : ISelectableCard//, IPointerDownHandler
{

    public delegate void OnClick(CardObject cardObj, Card card);
    public event OnClick OnClickDelegate;

    public delegate void OnClickAway();
    public event OnClickAway OnClickAwayDelegate;

    public Card card;
    public Image cardImage;
    //public GameObject highlight;

    //public bool interactable = true;
    //private bool clicked = false;

    //public override void DeselectCard()
    //{
    //    highlight.SetActive(false);
    //}

    //public override void SelectCard()
    //{
    //    highlight.SetActive(true);
    //}

    //public void setInteractable(bool val)
    //{
    //    interactable = val;
    //}

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (!interactable)
    //        return;
    //    OnClickDelegate?.Invoke(this, card);
    //    clicked = true;
    //}

    //private void Update()
    //{
    //    if (interactable && Input.GetMouseButtonDown(0) && highlight.activeSelf)
    //    {
    //        if (!clicked && OnClickAwayDelegate != null)
    //            OnClickAwayDelegate();
    //        else
    //            clicked = false;
    //    }
    //}

    protected override void onClickCall()
    {
        OnClickDelegate?.Invoke(this, card);
    }

    protected override void onClickAwayCall()
    {
        OnClickAwayDelegate?.Invoke();
    }
}
