using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float speed;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float movementMagnitude = Mathf.Clamp01(movementDirection.magnitude) * speed;

        movementDirection.Normalize();

        characterController.SimpleMove(movementDirection * movementMagnitude);
    }
}


