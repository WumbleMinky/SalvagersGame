using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenteredLayoutController : MonoBehaviour
{
    public float padding = 20f;
    public float spacing = 20f;

    private RectTransform rectTransform;
    private Dictionary<RectTransform, float> childAspectRatios = new Dictionary<RectTransform, float>();

    public void clear()
    {
        childAspectRatios.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        transform.DetachChildren();
    }

    public void addItem(GameObject go)
    {
        RectTransform itemRect = go.GetComponent<RectTransform>();
        if (itemRect == null)
            return;

        childAspectRatios.Add(itemRect, itemRect.rect.height / itemRect.rect.width);
        go.transform.SetParent(transform);
        adjustChildren();
    }

    public void adjustChildren()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        List<Rect> childRects = new List<Rect>();
        float childWidth = 0;
        for(int i = 0; i < transform.childCount; i++)
        {
            RectTransform childRT = transform.GetChild(i).GetComponent<RectTransform>();
            Rect r = new Rect();
            r.height = rectTransform.rect.height - (padding * 2);
            r.width = r.height / childAspectRatios[childRT];
            childRects.Add(r);
            childWidth += r.width;
        }

        childWidth += (spacing * (transform.childCount - 1));

        if (childWidth + (padding * 2) > rectTransform.rect.width)
        {
            //READJUST WIDTHS if too wide
        }

        float placementX = rectTransform.rect.center.x - (childWidth / 2f) + (childRects[0].width/2);

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform childRT = transform.GetChild(i).GetComponent<RectTransform>();
            childRT.anchoredPosition = new Vector2(placementX, 0);
            childRT.sizeDelta = new Vector2(childRects[i].width, childRects[i].height);
            placementX += spacing + childRects[i].width;
        }
    }
}
