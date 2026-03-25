using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // This works for solid traps (like spikes on the floor)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    // This works for overlapping traps (like your swinging hammer)
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckHit(other.gameObject);
    }

    // A shared method to keep our code clean
    private void CheckHit(GameObject target)
    {
        // Check if the object we hit is tagged as the Player
        if (target.CompareTag("Player"))
        {
            // Grab the PlayerController and trigger the death sequence
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}