using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PingMenuScript : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject menuParent;      // The whole circular menu parent (enable/disable)
    public Transform cursor;           // Rotating cursor around the circle
    public Camera playerCamera;        // Player's camera for raycasting
    public Transform pingParent;       // Where to spawn ping objects under

    [Header("Ping Prefab")]
    public GameObject pingPrefab;      // The unified PingObjectParent prefab

    [Header("Ping Settings")]
    public Transform player;           // Player reference for distance calculations
    public float cursorRadius = 45f;   // Radius of cursor from center

    [Header("Camera Controller")]
    public MonoBehaviour cameraController; // Reference to your camera controller script

    private bool menuActive = false;
    private int selectedSector = -1;
    private GameObject activePing;     // Track currently spawned ping

    // Store original cursor state
    private CursorLockMode originalLockState;
    private bool originalCursorVisible;

    void Start()
    {
        if (menuParent != null)
            menuParent.SetActive(false);
    }

    void Update()
    {
        HandleMenuToggle();

        if (menuActive)
        {
            UpdateCursor();

            // Left click confirm
            if (Mouse.current.leftButton.wasPressedThisFrame)
                SelectPing();
        }
    }

    void HandleMenuToggle()
    {
        // Middle mouse pressed → open menu
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            OpenMenu();
        }

        // Middle mouse released → close menu
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            CloseMenu();
        }
    }

    void OpenMenu()
    {
        menuActive = true;
        if (menuParent != null) menuParent.SetActive(true);

        // Store original cursor state
        originalLockState = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // Unlock cursor and make it visible for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable camera controller
        if (cameraController != null)
            cameraController.enabled = false;
    }

    void CloseMenu()
    {
        menuActive = false;
        if (menuParent != null) menuParent.SetActive(false);

        // Restore original cursor state
        Cursor.lockState = originalLockState;
        Cursor.visible = originalCursorVisible;

        // Re-enable camera controller
        if (cameraController != null)
            cameraController.enabled = true;
    }

    void UpdateCursor()
    {
        if (cursor == null) return;

        // Get mouse position relative to screen center
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Mouse.current.position.ReadValue() - screenCenter;

        // Only update if mouse has moved significantly from center
        if (mousePos.magnitude < 10f) return;

        // Calculate angle
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Move cursor along circular menu
        Vector3 cursorPos = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0
        ) * cursorRadius;

        cursor.localPosition = cursorPos;

        // Determine selected sector (0 = warning, 1 = default, 2 = look here)
        selectedSector = Mathf.FloorToInt(angle / 120f) % 3;
    }

    void SelectPing()
    {
        if (selectedSector < 0 || pingPrefab == null)
        {
            Debug.LogWarning("Cannot select ping: selectedSector=" + selectedSector + ", pingPrefab=" + (pingPrefab != null ? "assigned" : "null"));
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is null! Assign it in the inspector.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player transform is null! Assign it in the inspector.");
            return;
        }

        // Remove old ping if exists
        if (activePing != null)
            Destroy(activePing);

        // Determine spawn position using screen center for raycast
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        Vector3 spawnPos = player.position + playerCamera.transform.forward * 5f;

        if (Physics.Raycast(ray, out hit, 100f))
            spawnPos = hit.point;

        // Calculate rotation to face the player
        Vector3 directionToPlayer = (player.position - spawnPos).normalized;
        Quaternion facePlayerRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

        // Spawn the PingObjectParent prefab with rotation facing player
        activePing = Instantiate(pingPrefab, spawnPos, facePlayerRotation, pingParent);

        if (activePing == null)
        {
            Debug.LogError("Failed to instantiate ping prefab!");
            return;
        }

        // Try multiple ways to find the ScaledPingScript
        ScaledPingScript sps = null;

        // Method 1: Try to find "ScaledPing" child object
        Transform scaledPingChild = activePing.transform.Find("ScaledPing");
        if (scaledPingChild != null)
        {
            sps = scaledPingChild.GetComponent<ScaledPingScript>();
        }

        // Method 2: If not found, try getting it directly from the instantiated object
        if (sps == null)
        {
            sps = activePing.GetComponent<ScaledPingScript>();
        }

        // Method 3: If still not found, try getting it from any child
        if (sps == null)
        {
            sps = activePing.GetComponentInChildren<ScaledPingScript>();
        }

        // Initialize if found
        if (sps != null)
        {
            sps.Init(player, spawnPos, selectedSector);
        }
        else
        {
            Debug.LogError("ScaledPingScript not found on ping prefab! Make sure your prefab has the ScaledPingScript component.");
        }

        // Close menu
        CloseMenu();
    }


    // Alternative update method using mouse delta instead of absolute position
    void UpdateCursorAlternative()
    {
        if (cursor == null) return;

        // Get mouse delta movement
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // Only update if there's significant mouse movement
        if (mouseDelta.magnitude < 0.1f) return;

        // Get current cursor position and add delta
        Vector3 currentPos = cursor.localPosition;
        Vector2 currentPos2D = new Vector2(currentPos.x, currentPos.y);
        
        // Add scaled mouse delta
        currentPos2D += mouseDelta * 0.5f; // Adjust multiplier as needed
        
        // Normalize to cursor radius
        if (currentPos2D.magnitude > 0.1f)
        {
            currentPos2D = currentPos2D.normalized * cursorRadius;
            
            // Update cursor position
            cursor.localPosition = new Vector3(currentPos2D.x, currentPos2D.y, 0);
            
            // Calculate sector based on angle
            float angle = Mathf.Atan2(currentPos2D.y, currentPos2D.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            selectedSector = Mathf.FloorToInt(angle / 120f) % 3;
        }
    }
}