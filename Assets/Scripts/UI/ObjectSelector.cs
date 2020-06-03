using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelector : MonoBehaviour
{
    GameObject selectedObject;
    public Image selectImage;
    public float minWidth = 25;
    public float minHeight = 25;
    Camera mainCamera;
    RectTransform myRect;
    List<Renderer> renderers;
    Bounds selectedBounds;
    Vector3[] corners = new Vector3[8];
    float min_x;
    float min_y;
    float max_x;
    float max_y;

    void Start()
    {
        mainCamera = Camera.main;
        myRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObject == null || !selectImage.gameObject.activeSelf)
            return;
        
        corners[0] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x + selectedBounds.extents.x, selectedBounds.center.y + selectedBounds.extents.y, selectedBounds.center.z + selectedBounds.extents.z));
        corners[1] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x + selectedBounds.extents.x, selectedBounds.center.y + selectedBounds.extents.y, selectedBounds.center.z - selectedBounds.extents.z));
        corners[2] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x + selectedBounds.extents.x, selectedBounds.center.y - selectedBounds.extents.y, selectedBounds.center.z + selectedBounds.extents.z));
        corners[3] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x + selectedBounds.extents.x, selectedBounds.center.y - selectedBounds.extents.y, selectedBounds.center.z - selectedBounds.extents.z));
        corners[4] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x - selectedBounds.extents.x, selectedBounds.center.y + selectedBounds.extents.y, selectedBounds.center.z + selectedBounds.extents.z));
        corners[5] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x - selectedBounds.extents.x, selectedBounds.center.y + selectedBounds.extents.y, selectedBounds.center.z - selectedBounds.extents.z));
        corners[6] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x - selectedBounds.extents.x, selectedBounds.center.y - selectedBounds.extents.y, selectedBounds.center.z + selectedBounds.extents.z));
        corners[7] = mainCamera.WorldToScreenPoint(new Vector3(selectedBounds.center.x - selectedBounds.extents.x, selectedBounds.center.y - selectedBounds.extents.y, selectedBounds.center.z - selectedBounds.extents.z));

        min_x = corners[0].x;
        min_y = corners[0].y;
        max_x = corners[0].x;
        max_y = corners[0].y;

        for(int i = 1; i < 8; i++)
        {
            min_x = Math.Min(min_x, corners[i].x);
            min_y = Math.Min(min_y, corners[i].y);
            max_x = Math.Max(max_x, corners[i].x);
            max_y = Math.Max(max_y, corners[i].y);
        }
        if (max_x - min_x < minWidth)
        {
            float diff = minWidth - (max_x - min_x);
            min_x -= diff / 2;
            max_x += diff / 2;
        }
        if (max_y - min_y < minHeight)
        {
            float diff = minHeight - (max_y - min_y);
            min_y -= diff / 2;
            max_y += diff / 2;
        }
        myRect.position = new Vector2(min_x, min_y);
        myRect.sizeDelta = new Vector2(max_x - min_x , max_y - min_y);
    }

    public void selectObject(GameObject obj)
    {
        selectedObject = obj;
        selectImage.gameObject.SetActive(true);
        renderers = new List<Renderer>();
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
            renderers.Add(r);
        foreach(Renderer rend in obj.GetComponentsInChildren<Renderer>())
        {
            renderers.Add(rend);
        }
        selectedBounds = renderers[0].bounds;
        foreach (Renderer rend in renderers)
        {
            selectedBounds.Encapsulate(rend.bounds);
        }
    }

    public void deselect()
    {
        selectImage.gameObject.SetActive(false);
        selectedObject = null;
    }
}
