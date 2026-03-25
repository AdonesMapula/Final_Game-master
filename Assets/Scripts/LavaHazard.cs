using UnityEngine;
using UnityEngine.SceneManagement; // Required to restart the level

public class LavaHazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the lava has the "Player" tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player fell into lava!");
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        // Gets the index of the current active scene and reloads it
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}