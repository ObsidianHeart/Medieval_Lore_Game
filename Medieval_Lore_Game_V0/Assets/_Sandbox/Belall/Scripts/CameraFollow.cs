using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // The Player
    public Vector3 offset;              // Position gap (e.g., x:0, y:3, z:-5)

    public float fixedXRotation = 20f;  // <--- NEW: The strict angle you want (20)

    [Range(0, 1)]
    public float smoothSpeed = 0.5f;    // How fast it snaps to position

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. HANDLE POSITION (Same as before) ---
        // Calculate where the camera should be based on the player's facing direction
        Vector3 desiredPosition = target.TransformPoint(offset);

        // Smoothly move there
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);


        // --- 2. HANDLE ROTATION (New logic) ---
        // Instead of "Looking At" the player, we construct a specific rotation:
        // X = 20 (Fixed)
        // Y = target.eulerAngles.y (Match the player's turning)
        // Z = 0 (No tilting sideways)
        Quaternion desiredRotation = Quaternion.Euler(fixedXRotation, target.eulerAngles.y, 0);

        // Smoothly rotate to that angle
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
    }
}