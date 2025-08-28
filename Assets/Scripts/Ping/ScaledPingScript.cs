using UnityEngine;

public class ScaledPingScript : MonoBehaviour
{
    [Header("Scaling Settings")]
    public float PingScaleFactor = 1f;
    public float PingMinDist = 1f;
    public float PingMaxScale = 10f;
    public float PingMinScale = 0.5f;

    [Header("Target & Display")]
    public Transform Tgt;                   // Player to follow
    public TextMesh PingDistText;

    [Header("Visual Offset")]
    public Vector3 LocalOffset = Vector3.zero;

    void Update()
    {
        if (Tgt == null) return; // Don't move if no target

        // Follow player
        transform.position = Tgt.position + LocalOffset;

        // Distance for scaling and text
        float dist = Vector3.Distance(transform.position, Tgt.position);

        if (PingDistText != null)
            PingDistText.text = Mathf.Round(dist).ToString() + " M";

        // Scale with distance
        float scale = (dist / Mathf.Max(PingMinDist, 0.01f)) * PingScaleFactor;
        scale = Mathf.Clamp(scale, PingMinScale, PingMaxScale);
        transform.localScale = Vector3.one * scale;

        // Face camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180f, 0); // correct for single-sided mesh
        }
    }
}
