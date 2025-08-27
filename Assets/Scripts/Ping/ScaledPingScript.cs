using UnityEngine;

public class ScaledPingScript : MonoBehaviour
{
    [Header("Scaling Settings")]
    public float PingScaleFactor = 1f;     // base scale multiplier
    public float PingMinDist = 1f;         // distance below which scale is clamped
    public float PingMaxScale = 10f;       // maximum scale to prevent huge objects
    public float PingMinScale = 0.5f;      // minimum scale to prevent disappearing

    [Header("Target & Display")]
    public Transform Tgt;                   // player or camera to face
    public TextMesh PingDistText;           // optional text to show distance

    [Header("Visual Offset")]
    public Vector3 LocalOffset = new Vector3(0, 1f, 0); // lifts ping above parent pivot

    private float dist;

    void Update()
    {
        if (Tgt == null) return;

        // Move ping slightly above parent pivot
        transform.localPosition = LocalOffset;

        // Distance to player
        dist = Vector3.Distance(transform.position, Tgt.position);

        // Update distance text if assigned
        if (PingDistText != null)
            PingDistText.text = Mathf.Round(dist).ToString() + " M";

        // Scale ping based on distance, safely clamped
        float scale = (dist / Mathf.Max(PingMinDist, 0.01f)) * PingScaleFactor;
        scale = Mathf.Clamp(scale, PingMinScale, PingMaxScale);
        transform.localScale = Vector3.one * scale;

        // Rotate to always face player
        transform.LookAt(Tgt);

        // Flip if mesh is backwards (single-sided plane)
        transform.Rotate(0, 180f, 0); 
    }
}
