using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandLayoutController : MonoBehaviour
{
    public Canvas canvas;
    public float padding = 20f;
    public float spacing = 20f;
    //public float minWidth = 500;
    //public float maxWidth = 700;

    private float panelAspectRatio = 0;
    RectTransform rectTransform;
    Dictionary<RectTransform, float> aspectRatios = new Dictionary<RectTransform, float>(); // Keep all the original apsect ratios for the children. Height / Wdith
    List<RectTransform> myChildRects = new List<RectTransform>();

    public void addItem(GameObject item)
    {
        RectTransform itemRT = item.GetComponent<RectTransform>();
        if (itemRT == null)
            return;
        aspectRatios.Add(itemRT, itemRT.rect.height / itemRT.rect.width);
        myChildRects.Add(itemRT);
        item.transform.SetParent(transform);
        adjustChildren();
    }

    private void adjustChildren()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (panelAspectRatio == 0)
            panelAspectRatio = rectTransform.rect.height / rectTransform.rect.width;
        List<Rect> childRects = new List<Rect>();
        float totalWidth = 0.0f;
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
                totalWidth += padding + r.width;
            }
            else
            {
                r.x = childRects[i-1].x + childRects[i-1].width + spacing;
                totalWidth += r.width + spacing;
            }
            childRects.Add(r);
            childRT.anchoredPosition = new Vector2(r.x, r.y);
            childRT.sizeDelta = new Vector2(r.width, r.height);
        }
        totalWidth += padding;
        if (totalWidth > rectTransform.rect.width)
        {
            float diff = (totalWidth - rectTransform.rect.width) / transform.childCount;
            for(int i = 0; i < transform.childCount; i++)
            {
                RectTransform childRT = transform.GetChild(i).GetComponent<RectTransform>();
                Rect r = new Rect();
                r.width = childRT.rect.width - diff;
                r.height = r.width * aspectRatios[childRT];
                r.y = padding;
                if (i == 0)
                    r.x = padding;
                else
                    r.x = childRects[i - 1].x + childRects[i - 1].width + spacing;
                childRects[i] = r;
                childRT.anchoredPosition = new Vector2(r.x, r.y);
                childRT.sizeDelta = new Vector2(r.width, r.height);
            }
        }
    }
}
