using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorDropdown : Dropdown
{
    protected override void Start()
    {
        base.Start();
        
    }

    protected override void Awake()
    {
        base.Awake();
        //options.Clear();
        
        //for(int i = 0; i < ResourceContainer.Instance.playerColors.Length; i++)
        //{
        //    options.Add(new OptionData(ResourceContainer.Instance.playerColor))
        //}
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        //Transform dlist = transform.Find("Dropdown List");
        //if (dlist == null)
        //    return;

        //List<Image> colorImages = new List<Image>();
        //foreach (Image img in dlist.GetComponentsInChildren<Image>())
        //{
        //    if (img.gameObject.name.Equals("Color Image"))
        //    {
        //        colorImages.Add(img);
        //    }
        //}
        //for(int i = 0; i < options.Count; i++)
        //{
        //    if (i > colorImages.Count)
        //        return;

        //    colorImages[i].color = ResourceContainer.Instance.playerColors[i];
        //}
    }
}
