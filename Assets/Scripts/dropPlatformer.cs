using UnityEngine;

public class dropPlatformer : MonoBehaviour
{
    public Rigidbody2D trap;
    public float dropGravity = 5f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool triggered = false;

    void Start()
    {
        startPosition = trap.transform.position;
        startRotation = trap.transform.rotation;

        trap.bodyType = RigidbodyType2D.Kinematic;
        trap.gravityScale = 0f;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            trap.gravityScale = 5f;
        }
    }
    public void ResetTrap()
    {
        triggered = false;

        trap.linearVelocity = Vector2.zero;
        trap.angularVelocity = 0f;
        trap.bodyType = RigidbodyType2D.Kinematic;
        trap.gravityScale = 0f;
        trap.transform.position = startPosition;
        trap.transform.rotation = startRotation;
    }
}