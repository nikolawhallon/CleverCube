using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController3D : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;

    public Orientation orientation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        orientation.rotationY += mouseX;
        orientation.rotationX -= mouseY;
        orientation.rotationX = Mathf.Clamp(orientation.rotationX, -90.0f, 90.0f);

        transform.rotation = Quaternion.Euler(orientation.rotationX, orientation.rotationY, 0);
    }
}
