using UnityEngine;

public class LavaDrop : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            GetComponent<Rigidbody2D>().gravityScale = 0;

            anim.SetTrigger("Splash");

            Destroy(gameObject, 0.5f);
        }
    }
}