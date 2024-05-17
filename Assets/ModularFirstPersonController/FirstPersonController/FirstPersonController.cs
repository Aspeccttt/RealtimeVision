﻿// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for UI interactions
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    #region Ilario Cutajar section

    public GameObject infoPanelPrefab;
    public float infoPanelDistance = 2f;
    public LayerMask interactableLayer;
    public Transform infoPanelParent;
    public Transform dragAnchor;
    private LineRenderer currentLineRenderer;


    #region Variables
    private List<Transform> orientableUITransforms;
    public string targetCamera = "MainCamera";
    public Transform cameraTransform;

    private GameObject selectedPanel = null;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPanelPosition;
    #endregion

    private DatabaseManager dbManager;
    private Dictionary<GameObject, (LineRenderer lineRenderer, Transform dataPoint)> infoPanels = new Dictionary<GameObject, (LineRenderer, Transform)>();


    private void initializeSideVariables()
    {
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

        uiArrayCounter();
    }

    private void uiArrayCounter()
    {
        orientableUITransforms = new List<Transform>();
        GameObject[] orientableUIs = GameObject.FindGameObjectsWithTag("OrientableUI");
        foreach (GameObject ui in orientableUIs)
        {
            orientableUITransforms.Add(ui.transform);
        }
    }

    private void orientingUiToUser()
    {
        for (int i = orientableUITransforms.Count - 1; i >= 0; i--) // Iterate backwards through the list
        {
            if (orientableUITransforms[i] == null)
            {
                orientableUITransforms.RemoveAt(i); // Remove the null reference from the list
            }
            else
            {
                Transform uiTransform = orientableUITransforms[i];
                uiTransform.LookAt(uiTransform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
            }
        }
    }
    #endregion

    private Rigidbody rb;

    #region Camera Movement Variables

    public Camera playerCamera;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion
    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    private void Awake()
    {
        initializeSideVariables();

        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        dbManager = GameManager.Instance.GetComponent<DatabaseManager>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion
    }

    float camRotation;

    private void Update()
    {
        #region Camera

        // Control camera movement
        if (cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        orientingUiToUser();

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if (holdToZoom && !isSprinting)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if (enableSprint)
        {
            if (isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if (!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if (isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar 
            if (useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }

            if (Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }

        #endregion

        CheckGround();

        if (enableHeadBob)
        {
            HeadBob();
        }

        bool hasHit = false; // This boolean will track whether an object was hit by the raycast


        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition // Use the current mouse position
            };

            List<RaycastResult> uiHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, uiHits);
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            foreach (RaycastResult result in uiHits)
            {
                if (LayerMask.LayerToName(result.gameObject.layer) == "UI")
                {
                    if (result.gameObject.CompareTag("CloseButton"))
                    {
                        Debug.Log("Close button was clicked.");
                        Transform panel = result.gameObject.transform.parent;
                        while (panel != null && !panel.CompareTag("OrientableUI"))
                        {
                            panel = panel.parent;
                        }
                        if (panel != null)
                        {
                            Destroy(panel.gameObject);
                            infoPanels.Remove(panel.gameObject);
                        }
                        return;
                    }

                    if (result.gameObject.CompareTag("DraggablePanel"))
                    {
                        Debug.Log("Initiated Draggable Panel Part");
                        Transform panel = result.gameObject.transform.parent;
                        while (panel != null && !panel.CompareTag("OrientableUI"))
                        {
                            panel = panel.parent;
                        }

                        if (panel != null)
                        {
                            Debug.Log("Dragging panel: " + panel.gameObject.name);
                            selectedPanel = panel.transform.gameObject;
                            Vector3 panelScreenPosition = playerCamera.WorldToScreenPoint(selectedPanel.transform.position);
                            offset = selectedPanel.transform.position - playerCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, panelScreenPosition.z));
                            isDragging = true;

                            if (infoPanels.TryGetValue(selectedPanel, out var infoPanelData))
                            {
                                currentLineRenderer = infoPanelData.lineRenderer;
                            }
                        }
                        return;
                    }
                }
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                hasHit = true;
                if (hit.collider.CompareTag("DataPoint"))
                {
                    Debug.Log("Hit a DataPoint");
                    Color dataPointColor = hit.collider.GetComponent<Renderer>()?.material.color ?? Color.white;
                    string dataName = hit.collider.gameObject.name;
                    dbManager.LogDatapointClick(dataName);

                    ShowInfoPanel(hit.point, dataPointColor, dataName, GameManager.Instance.GetComponent<CSVPlotter>().columnXName, GameManager.Instance.GetComponent<CSVPlotter>().columnYName, GameManager.Instance.GetComponent<CSVPlotter>().columnZName);
                }
            }
        }

        if (isDragging)
        {
            if (dragAnchor != null && selectedPanel != null)
            {
                selectedPanel.transform.position = dragAnchor.transform.position;

                if (currentLineRenderer != null)
                {
                    Vector3[] positions = new Vector3[2];
                    currentLineRenderer.GetPositions(positions);
                    positions[1] = selectedPanel.transform.position; // Update the second position
                    currentLineRenderer.SetPositions(positions);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                selectedPanel = null;
                currentLineRenderer = null;
            }
        }

        foreach (var kvp in infoPanels)
        {
            var (lineRenderer, dataPoint) = kvp.Value;
            if (lineRenderer != null && dataPoint != null)
            {
                Vector3[] positions = new Vector3[2];
                lineRenderer.GetPositions(positions);
                positions[0] = dataPoint.position;
                positions[1] = kvp.Key.transform.position;
                lineRenderer.SetPositions(positions);
            }
        }
    }

    private void ShowInfoPanel(Vector3 hitPoint, Color dataPointColor, string dataName, string CX, string CY, string CZ)
    {
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.y = 0; // Adjust if you want the panel to align differently
        Vector3 panelPosition = playerCamera.transform.position + forwardDirection.normalized * infoPanelDistance;

        panelPosition.y = Mathf.Max(hitPoint.y, panelPosition.y);

        Quaternion panelRotation = Quaternion.LookRotation(panelPosition - playerCamera.transform.position);

        GameObject infoPanel = Instantiate(infoPanelPrefab, panelPosition, panelRotation, infoPanelParent);
        infoPanel.transform.LookAt(playerCamera.transform.position);
        infoPanel.SetActive(true);

        Image backgroundImage = infoPanel.GetComponentInChildren<Image>();
        backgroundImage.color = dataPointColor;

        string[] parts = dataName.Split(' '); // This assumes dataName format is "x y z"

        if (infoPanel.transform.parent.tag == "Histogram")
        {
            if (parts.Length >= 3)
            {
                TMP_Text[] infoTexts = infoPanel.GetComponentsInChildren<TMP_Text>();
                // Now assigning the parts of the dataName to the respective TMP_Text components
                infoTexts[1].text = CX; // ColumnX name (you might want to fetch actual name instead of hardcoding)
                infoTexts[2].text = parts[0]; // ColumnX Data
                infoTexts[3].text = CY; // ColumnY name
                infoTexts[4].text = parts[1]; // ColumnY Data
                infoTexts[5].text = CZ; // ColumnZ name
                infoTexts[6].text = parts[6]; // ColumnZ Data
            }
        }
        else
        {
            if (parts.Length >= 3)
            {
                TMP_Text[] infoTexts = infoPanel.GetComponentsInChildren<TMP_Text>();
                // Now assigning the parts of the dataName to the respective TMP_Text components
                infoTexts[1].text = CX; // ColumnX name (you might want to fetch actual name instead of hardcoding)
                infoTexts[2].text = parts[0]; // ColumnX Data
                infoTexts[3].text = CY; // ColumnY name
                infoTexts[4].text = parts[1]; // ColumnY Data
                infoTexts[5].text = CZ; // ColumnZ name
                infoTexts[6].text = parts[2]; // ColumnZ Data
            }
        }

        // Create a LineRenderer component dynamically and set the colour as the same as the datapoint.
        currentLineRenderer = infoPanel.AddComponent<LineRenderer>();
        currentLineRenderer.startWidth = 0.02f;
        currentLineRenderer.endWidth = 0.00002f;
        currentLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        currentLineRenderer.material.color = dataPointColor;
        currentLineRenderer.positionCount = 2;
        currentLineRenderer.SetPositions(new Vector3[] { hitPoint, panelPosition });

        infoPanels[infoPanel] = (currentLineRenderer, null); // Store the LineRenderer and the panel without dataPoint

        uiArrayCounter();
    }

    public void DespawnAllInfoPanels()
    {
        foreach (var kvp in infoPanels)
        {
            Destroy(kvp.Key);
        }
        infoPanels.Clear();
    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations shile sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        Crouch();
                    }

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if (isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            // Calculates HeadBob speed during sprint
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    // Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
    public class FirstPersonControllerEditor : Editor
    {
        FirstPersonController fpc;
        SerializedObject SerFPC;

        private void OnEnable()
        {
            fpc = (FirstPersonController)target;
            SerFPC = new SerializedObject(fpc);
        }

        public override void OnInspectorGUI()
        {
            SerFPC.Update();

            EditorGUILayout.Space();
            GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Label("Asset Store Script by: Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
            GUILayout.Label("Modified by: Ilario Cutajar.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
            GUILayout.Label("version 1.1", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
            EditorGUILayout.Space();

            #region Camera Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera, typeof(Camera), true);
            fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View", "The camera’s view angle. Changes the player camera directly."), fpc.fov, fpc.zoomFOV, 179f);
            fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."), fpc.cameraCanMove);

            GUI.enabled = fpc.cameraCanMove;
            fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpc.invertCamera);
            fpc.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."), fpc.mouseSensitivity, .1f, 10f);
            fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40, 90);
            GUI.enabled = true;

            fpc.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

            fpc.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpc.crosshair);

            // Only displays crosshair options if crosshair is enabled
            if (fpc.crosshair)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair."));
                fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                fpc.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpc.crosshairColor);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            #region Camera Zoom Setup

            GUILayout.Label("Zoom", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

            fpc.enableZoom = EditorGUILayout.ToggleLeft(new GUIContent("Enable Zoom", "Determines if the player is able to zoom in while playing."), fpc.enableZoom);

            GUI.enabled = fpc.enableZoom;
            fpc.holdToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Hold to Zoom", "Requires the player to hold the zoom key instead if pressing to zoom and unzoom."), fpc.holdToZoom);
            fpc.zoomKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key", "Determines what key is used to zoom."), fpc.zoomKey);
            fpc.zoomFOV = EditorGUILayout.Slider(new GUIContent("Zoom FOV", "Determines the field of view the camera zooms to."), fpc.zoomFOV, .1f, fpc.fov);
            fpc.zoomStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while zooming in."), fpc.zoomStepTime, .1f, 10f);
            GUI.enabled = true;

            #endregion

            #endregion

            #region Movement Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."), fpc.playerCanMove);

            GUI.enabled = fpc.playerCanMove;
            fpc.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed", "Determines how fast the player will move while walking."), fpc.walkSpeed, .1f, fpc.sprintSpeed);
            GUI.enabled = true;

            EditorGUILayout.Space();

            #region Sprint

            GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

            fpc.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpc.enableSprint);

            GUI.enabled = fpc.enableSprint;
            fpc.unlimitedSprint = EditorGUILayout.ToggleLeft(new GUIContent("Unlimited Sprint", "Determines if 'Sprint Duration' is enabled. Turning this on will allow for unlimited sprint."), fpc.unlimitedSprint);
            fpc.sprintKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "Determines what key is used to sprint."), fpc.sprintKey);
            fpc.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."), fpc.sprintSpeed, fpc.walkSpeed, 20f);

            //GUI.enabled = !fpc.unlimitedSprint;
            fpc.sprintDuration = EditorGUILayout.Slider(new GUIContent("Sprint Duration", "Determines how long the player can sprint while unlimited sprint is disabled."), fpc.sprintDuration, 1f, 20f);
            fpc.sprintCooldown = EditorGUILayout.Slider(new GUIContent("Sprint Cooldown", "Determines how long the recovery time is when the player runs out of sprint."), fpc.sprintCooldown, .1f, fpc.sprintDuration);
            //GUI.enabled = true;

            fpc.sprintFOV = EditorGUILayout.Slider(new GUIContent("Sprint FOV", "Determines the field of view the camera changes to while sprinting."), fpc.sprintFOV, fpc.fov, 179f);
            fpc.sprintFOVStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while sprinting."), fpc.sprintFOVStepTime, .1f, 20f);

            fpc.useSprintBar = EditorGUILayout.ToggleLeft(new GUIContent("Use Sprint Bar", "Determines if the default sprint bar will appear on screen."), fpc.useSprintBar);

            // Only displays sprint bar options if sprint bar is enabled
            if (fpc.useSprintBar)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                fpc.hideBarWhenFull = EditorGUILayout.ToggleLeft(new GUIContent("Hide Full Bar", "Hides the sprint bar when sprint duration is full, and fades the bar in when sprinting. Disabling this will leave the bar on screen at all times when the sprint bar is enabled."), fpc.hideBarWhenFull);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Bar BG", "Object to be used as sprint bar background."));
                fpc.sprintBarBG = (Image)EditorGUILayout.ObjectField(fpc.sprintBarBG, typeof(Image), true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Bar", "Object to be used as sprint bar foreground."));
                fpc.sprintBar = (Image)EditorGUILayout.ObjectField(fpc.sprintBar, typeof(Image), true);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                fpc.sprintBarWidthPercent = EditorGUILayout.Slider(new GUIContent("Bar Width", "Determines the width of the sprint bar."), fpc.sprintBarWidthPercent, .1f, .5f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                fpc.sprintBarHeightPercent = EditorGUILayout.Slider(new GUIContent("Bar Height", "Determines the height of the sprint bar."), fpc.sprintBarHeightPercent, .001f, .025f);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            #endregion

            #region Jump

            GUILayout.Label("Jump", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

            fpc.enableJump = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump", "Determines if the player is allowed to jump."), fpc.enableJump);

            GUI.enabled = fpc.enableJump;
            fpc.jumpKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "Determines what key is used to jump."), fpc.jumpKey);
            fpc.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "Determines how high the player will jump."), fpc.jumpPower, .1f, 20f);
            GUI.enabled = true;

            EditorGUILayout.Space();

            #endregion

            #region Crouch

            GUILayout.Label("Crouch", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

            fpc.enableCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Enable Crouch", "Determines if the player is allowed to crouch."), fpc.enableCrouch);

            GUI.enabled = fpc.enableCrouch;
            fpc.holdToCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Hold To Crouch", "Requires the player to hold the crouch key instead if pressing to crouch and uncrouch."), fpc.holdToCrouch);
            fpc.crouchKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "Determines what key is used to crouch."), fpc.crouchKey);
            fpc.crouchHeight = EditorGUILayout.Slider(new GUIContent("Crouch Height", "Determines the y scale of the player object when crouched."), fpc.crouchHeight, .1f, 1);
            fpc.speedReduction = EditorGUILayout.Slider(new GUIContent("Speed Reduction", "Determines the percent 'Walk Speed' is reduced by. 1 being no reduction, and .5 being half."), fpc.speedReduction, .1f, 1);
            GUI.enabled = true;

            #endregion

            #endregion

            #region Head Bob

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Head Bob Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.enableHeadBob = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob", "Determines if the camera will bob while the player is walking."), fpc.enableHeadBob);


            GUI.enabled = fpc.enableHeadBob;
            fpc.joint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Joint", "Joint object position is moved while head bob is active."), fpc.joint, typeof(Transform), true);
            fpc.bobSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Determines how often a bob rotation is completed."), fpc.bobSpeed, 1, 20);
            fpc.bobAmount = EditorGUILayout.Vector3Field(new GUIContent("Bob Amount", "Determines the amount the joint moves in both directions on every axes."), fpc.bobAmount);
            GUI.enabled = true;

            #endregion

            #region Interactable Objects Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Interactable Objects Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
            GUILayout.Label("Section by: Ilario Cutajar.", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
            EditorGUILayout.Space();

            // Here we add the custom fields for the FirstPersonController script
            fpc.infoPanelParent = (Transform)EditorGUILayout.ObjectField(new GUIContent("Info Panel Parent", "Parent GameObject for info panels."), fpc.infoPanelParent, typeof(Transform), true);
            fpc.infoPanelPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Info Panel Prefab", "Prefab for the information panel displayed upon interacting."), fpc.infoPanelPrefab, typeof(GameObject), false);
            fpc.infoPanelDistance = EditorGUILayout.FloatField(new GUIContent("Info Panel Distance", "Distance from the camera to show the info panel."), fpc.infoPanelDistance);
            fpc.interactableLayer = EditorGUILayout.LayerField(new GUIContent("Interactable Layer", "Layer your data points are on."), fpc.interactableLayer);
            fpc.dragAnchor = (Transform)EditorGUILayout.ObjectField(new GUIContent("Drag Anchor", "Parent transform."), fpc.dragAnchor, typeof(Transform), true);
            #endregion

            //Sets any changes from the prefab
            if (GUI.changed)
            {
                EditorUtility.SetDirty(fpc);
                Undo.RecordObject(fpc, "FPC Change");
                SerFPC.ApplyModifiedProperties();
            }
        }

    }
}
#endif