using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EdgeTiltPlatform2D : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float platformHalfWidth = 2f; // World units (half of platform width)

    [Header("Edge Tilt Only")]
    [Range(0f, 1f)]
    [SerializeField] private float edgeStart = 0.7f;   // Tilt starts at 70% toward edge
    [SerializeField] private float maxTiltAngle = 20f; // Degrees at very edge
    [SerializeField] private float tiltSpeed = 140f;   // Degrees/sec

    private Rigidbody2D rb;
    private Transform standingPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Recommended Rigidbody2D:
        // Body Type = Kinematic, Interpolate = Interpolate
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & playerLayer.value) == 0) return;

        // Only count as "standing" if player is on top
        for (int i = 0; i < col.contactCount; i++)
        {
            if (col.GetContact(i).normal.y < -0.5f)
            {
                standingPlayer = col.transform;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (standingPlayer == col.transform)
            standingPlayer = null;
    }

    private void FixedUpdate()
    {
        float targetAngle = 0f;

        if (standingPlayer != null)
        {
            float localX = transform.InverseTransformPoint(standingPlayer.position).x;
            float n = Mathf.Clamp(localX / Mathf.Max(0.001f, platformHalfWidth), -1f, 1f);
            float absN = Mathf.Abs(n);

            // No tilt in center zone; only tilt near edges
            if (absN >= edgeStart)
            {
                // Map [edgeStart..1] -> [0..1]
                float edgeT = Mathf.InverseLerp(edgeStart, 1f, absN);
                float signedTilt = Mathf.Sign(n) * edgeT * maxTiltAngle;

                // Negative sign if you want right side to go down when player is right
                targetAngle = -signedTilt;
            }
        }

        float next = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, tiltSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(next);
    }
}