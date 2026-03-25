using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Pickup Delay")]
    public float pickupDelay = 3f;

    private bool canBeCollected = false;

    private void Start()
    {
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void EnablePickup()
    {
        canBeCollected = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!canBeCollected || !collision.CompareTag("Player"))
            return;

        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null && player.keysCollected < player.maxKeys)
        {
            player.AddKey();
            Destroy(gameObject);
        }
    }
}