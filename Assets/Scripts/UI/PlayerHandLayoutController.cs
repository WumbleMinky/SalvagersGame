using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandLayoutController : MonoBehaviour
{

    public float padding = 20f;
    public float spacing = 20f;

    RectTransform rectTransform;
    Dictionary<RectTransform, float> aspectRatios = new Dictionary<RectTransform, float>(); // Keep all the original apsect ratios for the children. Height / Wdith

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addItem(GameObject item)
    {
        RectTransform itemRT = item.GetComponent<RectTransform>();
        if (itemRT == null)
            return;
        aspectRatios.Add(itemRT, itemRT.rect.height / itemRT.rect.width);
        item.transform.SetParent(transform);
        adjustChildren();
    }

    private void adjustChildren()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        List<Rect> childRects = new List<Rect>();
        for(int i  = 0; i < transform.childCount; i++)
        {
            RectTransform childRT = transform.GetChild(i).GetComponent<RectTransform>();
            Rect r = new Rect();
            r.height = rectTransform.rect.height - (padding * 2);
            r.width = r.height / aspectRatios[childRT];
            r.y = padding;
            if (i == 0)
            {
                r.x = padding;
            }
            else
            {
                r.x = childRects[i-1].x + childRects[i-1].width + spacing;
            }
            childRects.Add(r);
            childRT.anchoredPosition = new Vector2(r.x, r.y);
            childRT.sizeDelta = new Vector2(r.width, r.height);
        }
    }
}
