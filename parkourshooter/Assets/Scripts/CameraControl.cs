using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float sensX, sensY;
    public bool canLook = true;
    public Transform playerOrientation, cameraOrientation;
    float xRot, yRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Move camera to player
        cameraOrientation.position = playerOrientation.position;
        // Get Mouse input since last frame * sensitivity
        float mouseInputX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseInputY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        if(canLook == true)
        {
            // Apply mouse input to rotations
            yRot += mouseInputX;
            xRot -= mouseInputY;
            // Restrict vertical rotation
            xRot = Mathf.Clamp(xRot, -90f, 90f);
            // Set Camera facing
            cameraOrientation.rotation = Quaternion.Euler(xRot, yRot, 0);
            // Set Player facing
            playerOrientation.rotation = Quaternion.Euler(0, yRot, 0);
        }
    }
}
