using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Global Variables
    // UI
    private List<Transform> orientableUITransforms;
    public string targetCamera = "MainCamera";
    public Transform cameraTransform;

    // Movement
    public float speed = 6.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100f;
    public float zoomSpeed = 4f;
    public float minFov = 15f; // Minimum field of view for zooming in
    public float maxFov = 90f; // Maximum field of view for zooming out

    private CharacterController controller;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float xRotation = 0f;

    private Camera playerCamera; // Add a reference for the player's camera

    // Camera
    public Vector3 cameraOffset;
    public float cameraFollowSpeed = 10f;
    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component not found on " + gameObject.name);
        }

        setupGlobalVariables();
    }

    void Update()
    {
        if (controller == null) return; // Ensure we have a controller before proceeding
        movementHandler();
        HandleCameraAndPlayerRotation(); // This method will now continuously update rotation
        followCamera(); // Ensure camera follows the player smoothly
    }

    private void setupGlobalVariables()
    {
        Cursor.lockState = CursorLockMode.Locked;

        GameObject cameraGameObject = GameObject.FindGameObjectWithTag(targetCamera);
        if (cameraGameObject != null)
        {
            cameraTransform = cameraGameObject.transform;
            playerCamera = cameraGameObject.GetComponent<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("Camera component not found on camera GameObject");
            }
        }
        else
        {
            Debug.LogError("No camera GameObject with tag " + targetCamera + " found.");
        }

        orientableUITransforms = new List<Transform>();
        GameObject[] orientableUIs = GameObject.FindGameObjectsWithTag("OrientableUI");
        foreach (GameObject ui in orientableUIs)
        {
            orientableUITransforms.Add(ui.transform);
        }
    }

    private void orientingUiToUser()
    {
        if (cameraTransform == null) return;

        foreach (Transform uiTransform in orientableUITransforms)
        {
            uiTransform.LookAt(uiTransform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
        }
    }

    private void movementHandler()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * Time.deltaTime * speed);

        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        if (playerCamera != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            playerCamera.fieldOfView -= scroll * zoomSpeed;
            playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, minFov, maxFov);
        }
        else
        {
            Debug.LogWarning("Player camera is not set for zoom functionality.");
        }
    }

    private void HandleCameraAndPlayerRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Apply the rotation for looking up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player based on horizontal mouse movement
        transform.Rotate(Vector3.up * mouseX);

        // Update the player's forward direction to align with the camera's forward direction
        // This part ensures the player moves in the direction they are facing
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0; // Keep the player's forward direction strictly horizontal
        transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * cameraFollowSpeed);
    }
    private void followCamera()
    {
        if (cameraTransform == null) return;

        // Update camera position smoothly to follow the player while maintaining the offset
        Vector3 newPosition = transform.position + transform.TransformDirection(cameraOffset);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPosition, cameraFollowSpeed * Time.deltaTime);
        cameraTransform.LookAt(transform.position); // Ensure the camera always looks at the player
    }
}
