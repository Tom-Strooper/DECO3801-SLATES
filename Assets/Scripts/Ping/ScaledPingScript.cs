using UnityEngine;
using TMPro;

public class ScaledPingScript : MonoBehaviour
{
    [Header("Scaling Settings")]
    public float PingScaleFactor = 1f;
    public float PingMinDist = 1f;
    public float PingMaxScale = 3f;
    public float PingMinScale = 0.2f;

    [Header("Target & Display")]
    public Transform Tgt;                   // Player to calculate distance from
    public TextMesh PingDistText;           // Use TMP if you like

    [Header("Ping Type Icons")]
    public GameObject WarningIcon;
    public GameObject DefaultIcon;
    public GameObject LookHereIcon;

    [Header("Rotation Settings")]
    public bool alwaysFacePlayer = true;    // Toggle to always face player
    public bool smoothRotation = true;      // Smooth rotation vs instant
    public float rotationSpeed = 5f;        // Speed of smooth rotation

    private Vector3 spawnPos;               // Fixed spawn position

    public void Init(Transform player, Vector3 position, int pingType)
    {
        Tgt = player;
        spawnPos = position;

        // Set active icon
        WarningIcon.SetActive(pingType == 0);
        DefaultIcon.SetActive(pingType == 1);
        LookHereIcon.SetActive(pingType == 2);

        transform.position = spawnPos; // set initial position
        
        // Initial rotation to face player
        if (alwaysFacePlayer && Tgt != null)
        {
            Vector3 directionToPlayer = (Tgt.position - spawnPos).normalized;
            transform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        }
    }

    void Update()
    {
        if (Tgt == null) return;

        // Ping stays at spawn position
        transform.position = spawnPos;

        // Distance for scaling and text
        float dist = Vector3.Distance(spawnPos, Tgt.position);

        if (PingDistText != null)
        {
            PingDistText.text = $"{Mathf.RoundToInt(dist)}m";

            // 🔹 Ensure text is never mirrored
            Vector3 ls = PingDistText.transform.localScale;
            PingDistText.transform.localScale = new Vector3(Mathf.Abs(ls.x), ls.y, ls.z);
        }

        // Scale with distance
        float scale = (dist / Mathf.Max(PingMinDist, 0.01f)) * PingScaleFactor;
        scale = Mathf.Clamp(scale, PingMinScale, PingMaxScale);
        transform.localScale = Vector3.one * scale;

        // Face player instead of camera
        if (alwaysFacePlayer)
        {
            Vector3 directionToPlayer = (Tgt.position - spawnPos).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            
            if (smoothRotation)
            {
                // Smooth rotation towards player
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // Instant rotation towards player
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // Original behavior - face camera
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180f, 0); // correct for single-sided mesh
            }
        }
    }
}
