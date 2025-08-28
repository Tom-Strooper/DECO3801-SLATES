using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PingMenuScript : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject menuParent;      // The whole circular menu parent (enable/disable)
    public Transform cursor;           // Rotating cursor around the circle
    public Transform sectorParent;     // Parent holding the 3 sectors
    public Camera playerCamera;        // Player's camera for raycasting
    public Transform pingParent;       // Where to spawn ping objects under

    [Header("Ping Prefabs")]
    public GameObject pingWarningPrefab;
    public GameObject pingDefaultPrefab;
    public GameObject pingLookHerePrefab;

    [Header("Ping Settings")]
    public Transform player;           // Player reference for ScaledPingScript
    public float cursorRadius = 50f;   // Radius of cursor from center

    private bool menuActive = false;
    private int selectedSector = -1;

    private GameObject activePing;     // keep track of the currently spawned ping

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
            {
                SelectPing();
            }
        }
    }

    void HandleMenuToggle()
    {
        // Middle mouse pressed → open menu
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            menuActive = true;
            if (menuParent != null) menuParent.SetActive(true);

            // TODO: disable camera controller here
            // Example: playerCamera.GetComponent<CameraController>().enabled = false;
        }

        // Middle mouse released → close menu
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            menuActive = false;
            if (menuParent != null) menuParent.SetActive(false);

            // TODO: re-enable camera controller
            // Example: playerCamera.GetComponent<CameraController>().enabled = true;
        }
    }

    void HighlightSector()
    {
        if (sectorParent == null) return;

        for (int i = 0; i < sectorParent.childCount; i++)
        {
            // Enable only the active sector
            sectorParent.GetChild(i).gameObject.SetActive(i == selectedSector);
        }
    }

    void UpdateCursor()
    {
        if (cursor == null) return;

        // Get mouse position in screen space
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Mouse.current.position.ReadValue() - screenCenter;

        // Get angle
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Position cursor on circular menu
        Vector3 cursorPos = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0
        ) * cursorRadius;

        cursor.localPosition = cursorPos;

        // Determine selected sector (3 slices of 120° each)
        selectedSector = Mathf.FloorToInt(angle / 120f) % 3;

        // Highlight the correct sector
        HighlightSector();
    }

    void SelectPing()
    {
        if (selectedSector < 0) return;

        GameObject prefabToSpawn = null;

        switch (selectedSector)
        {
            case 0: prefabToSpawn = pingWarningPrefab; break;
            case 1: prefabToSpawn = pingDefaultPrefab; break;
            case 2: prefabToSpawn = pingLookHerePrefab; break;
        }

        if (prefabToSpawn != null)
        {
            // Remove old ping if it exists
            if (activePing != null)
                Destroy(activePing);

            // Cast ray from camera to where player is pointing
            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            Vector3 spawnPos = player.position + playerCamera.transform.forward * 5f;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                spawnPos = hit.point;
            }

            // Spawn new ping
            activePing = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, pingParent);

            // Assign ScaledPingScript target
            ScaledPingScript pingScript = activePing.GetComponent<ScaledPingScript>();
            if (pingScript != null)
                pingScript.Tgt = player;

            // Close menu after placing ping
            menuActive = false;
            if (menuParent != null) menuParent.SetActive(false);

            // TODO: re-enable camera controller
            // Example: playerCamera.GetComponent<CameraController>().enabled = true;
        }
    }
}
