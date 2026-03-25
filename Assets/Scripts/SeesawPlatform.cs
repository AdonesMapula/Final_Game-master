using UnityEngine;

public class SeesawPlatform360 : MonoBehaviour
{
    public float torqueStrength = 100f;
    public float maxAngularVelocity = 400f;

    private Rigidbody2D rb;
    private Transform playerOnTop;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (playerOnTop != null)
        {
            float offset = playerOnTop.position.x - transform.position.x;

            // apply strong torque depending on where the player stands
            rb.AddTorque(-offset * torqueStrength);

            // optional clamp so it doesn't go too crazy
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTop = collision.transform;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTop = null;
        }
    }
}