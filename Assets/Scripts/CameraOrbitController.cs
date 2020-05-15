using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitController: MonoBehaviour
{
    public Transform myCamera;
    public Transform verticalRotationHolder;
    public float scrollSpeed = 10f;
    public float rotationSpeed = 10f;
    public float moveSpeed = 10f;
    

    public float minVertRotation = 10f;
    public float maxVertRotation = 80f;

    public float minSrollDistance = 5;

    private float actualMinVert;
    private float actualMaxVert;

    private float xRot;

    private void Start()
    {
        //Calculate the actual vertical rotations based on the camera's start place.
        actualMaxVert = maxVertRotation - myCamera.localEulerAngles.x;
        actualMinVert = minVertRotation - myCamera.localEulerAngles.x;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float horiz = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float vert = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        if (scroll != 0)
        {
            Vector3 camPos = myCamera.position + myCamera.forward * scroll * scrollSpeed;
            if (camPos.magnitude > 5)
                myCamera.position = camPos;
        }

        if (Input.GetMouseButtonDown(2))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetMouseButton(2))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            transform.rotation *= Quaternion.Euler(Vector3.up * x * rotationSpeed);
            xRot = Mathf.Clamp(xRot - y * rotationSpeed, actualMinVert, actualMaxVert);
            verticalRotationHolder.localEulerAngles= new Vector3(xRot, 0, 0);
        }

        if (Input.GetMouseButtonUp(2))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (horiz != 0 || vert != 0)
        {
            transform.position += transform.right * horiz + transform.forward * vert;
        }
    }
}
