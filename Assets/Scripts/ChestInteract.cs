using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    private Animator anim;
    private bool isOpen = false;

    [Header("Loot Settings")]
    public GameObject keyPrefab;
    public Transform spawnPoint;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isOpen)
            return;

        isOpen = true;

        if (anim != null)
            anim.SetTrigger("Open");

        if (keyPrefab != null && spawnPoint != null)
        {
            Instantiate(keyPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}