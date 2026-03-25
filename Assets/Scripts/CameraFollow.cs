using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Drag your player here
    public float smoothSpeed = 0.125f; // Higher = faster follow
    public Vector3 offset;        // Typically (0, 0, -10) for 2D

    // FixedUpdate is better for camera following physics-based players
    void FixedUpdate()
    {
        if (target != null)
        {
            // Calculate where the camera wants to be
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly transition from current position to desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Apply the position
            transform.position = smoothedPosition;
        }
    }
}