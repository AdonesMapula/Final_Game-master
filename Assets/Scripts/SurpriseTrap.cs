using UnityEngine;

public class SurpriseTrap : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    public float gravityWhenDropped = 3f;

    private Vector3 startPos;
    private Quaternion startRot;
    private bool activated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        ResetTrap();
    }

    public void TriggerTrap()
    {
        if (activated) return;

        activated = true;

        sr.enabled = true;
        col.enabled = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityWhenDropped;
    }

    public void ResetTrap()
    {
        activated = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        transform.position = startPos;
        transform.rotation = startRot;

        sr.enabled = false;
        col.enabled = false;
    }
}