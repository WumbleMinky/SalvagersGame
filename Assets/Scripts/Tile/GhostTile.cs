using UnityEngine;
using UnityEngine.EventSystems;

public class GhostTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public delegate void OnMouseOver();
    public static event OnMouseOver onMouseOverDelegate;

    public delegate void OnMouseExit();
    public static event OnMouseExit onMouseExitDelegate;

    public Color hightlightColor;
    Renderer[] myRenderers;
    Color[] startColors;
    PlayerData player;

    public void OnPointerClick(PointerEventData eventData)
    {
        CardSelection.Instance.placeTile(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int i = 0;
        foreach (Renderer childRenderer in myRenderers)
        {
            startColors[i] = childRenderer.material.color;
            childRenderer.material.color = hightlightColor;
            i++;
        }
        onMouseOverDelegate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        int i = 0;
        foreach (Renderer childRenderer in myRenderers)
        {
            childRenderer.material.color = startColors[i];
            i++;
        }
        onMouseExitDelegate();
    }

    void Start()
    {
        myRenderers = GetComponentsInChildren<Renderer>();
        startColors = new Color[myRenderers.Length];
    }
}
