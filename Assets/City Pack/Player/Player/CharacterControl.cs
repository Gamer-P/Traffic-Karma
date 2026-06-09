using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public float speed = 6.0f;
    public float mouseSensitivity = 100f;

    float xRotation = 0f;

    private Transform cam;
    private CharacterController charController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        charController = GetComponent<CharacterController>();
        cam = transform.Find("Camera");
    }

    void Update()
    {
        CameraMovement();
        PlayerMovement();
    }

    void PlayerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * 1.4f : speed;

        charController.SimpleMove(Vector3.ClampMagnitude(move, 1.0f) * currentSpeed);
    }

    void CameraMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (UP/DOWN)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 70f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation (LEFT/RIGHT)
        transform.Rotate(Vector3.up * mouseX);
    }
}