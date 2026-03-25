using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public Rigidbody2D trap;
    public float dropGravity = 1.5f;

    private bool triggered = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = trap.transform.position;
        startRotation = trap.transform.rotation;

        ResetTrap();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            trap.bodyType = RigidbodyType2D.Dynamic;
            trap.gravityScale = dropGravity;
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