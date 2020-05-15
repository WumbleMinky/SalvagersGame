using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    Transform cameraTransform;
    Quaternion originalRotation;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = cameraTransform.rotation* originalRotation;
    }
}
