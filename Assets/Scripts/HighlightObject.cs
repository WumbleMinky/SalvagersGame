using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    public Color hightlightColor;
    public bool useMouseOver = true;
    Renderer[] myRenderers;
    Color[] startColors;
    bool triggerHighlight = false;
    bool triggerRemoveHightlight = false;
    bool isHighlighted = false;

    private void Start()
    {
        myRenderers = GetComponentsInChildren<Renderer>();
        startColors = new Color[myRenderers.Length];
    }

    private void Update()
    {
        if (triggerHighlight)
        {
            if (!isHighlighted)
            {
                int i = 0;
                foreach (Renderer childRenderer in myRenderers)
                {
                    startColors[i] = childRenderer.material.color;
                    childRenderer.material.color = hightlightColor;
                    i++;
                }
                isHighlighted = true;
            }
            triggerHighlight = false;
        }

        if (triggerRemoveHightlight)
        {
            if (isHighlighted)
            {
                int i = 0;
                foreach (Renderer childRenderer in myRenderers)
                {
                    childRenderer.material.color = startColors[i];
                    i++;
                }
                isHighlighted = false;
            }
            triggerRemoveHightlight = false;
        }
    }

    public void hightlight()
    {
        if (isHighlighted)
            return;  //already highlighted
        triggerHighlight = true;
    }

    public void unhighlight()
    {
        if (!isHighlighted)
            return; // already not hightlighted
        triggerRemoveHightlight = true;
    }
}
