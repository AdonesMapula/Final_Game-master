using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Settings")]
    public string targetSceneName; // Type the name of the scene to load here

    // This runs when something enters the Portal's "hitbox"
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that touched the portal is the Player
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}