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
  
    protected override void onClickCall()
    {
        OnClickDelegate?.Invoke(this, card);
    }

    protected override void onClickAwayCall()
    {
        OnClickAwayDelegate?.Invoke();
    }
}
