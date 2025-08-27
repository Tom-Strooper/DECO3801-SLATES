using UnityEngine;
using UnityEngine.InputSystem;

public class PingMenuScript : MonoBehaviour
{
    public GameObject PingMenuRootObject;
    public Transform CursorRootObject;

    private Vector3 lastMousePosition;
    private float currentAngle = 0f; // track cursor rotation

    void Start()
    {
        // Ensure the menu is closed by default
        if (PingMenuRootObject != null)
            PingMenuRootObject.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Open menu on middle mouse button down
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            if (PingMenuRootObject != null)
                PingMenuRootObject.SetActive(true);

            lastMousePosition = Mouse.current.position.ReadValue();
        }

        // Close menu on middle mouse button up
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            if (PingMenuRootObject != null)
                PingMenuRootObject.SetActive(false);
        }

        // Track mouse movement while holding middle mouse button
        if (Mouse.current.middleButton.isPressed && CursorRootObject != null)
        {
            Vector3 currentMousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;

            // Adjust angle: use vertical drag to rotate clockwise
            currentAngle += mouseDelta.y * 0.5f; // tweak sensitivity
            // Wrap angle to 0-360
            currentAngle = currentAngle % 360f;
            if (currentAngle < 0) currentAngle += 360f;

            // Calculate smooth rotation
            float smoothAngle = Mathf.MoveTowardsAngle(CursorRootObject.eulerAngles.z, currentAngle, mouseDelta.magnitude * Time.deltaTime * 250);
            CursorRootObject.eulerAngles = new Vector3(0, 0, smoothAngle);

            lastMousePosition = currentMousePosition;
        }
    }
}
