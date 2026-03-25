using System.Collections;
using UnityEngine;

public class SpriteSpawner : MonoBehaviour
{
    [Tooltip("Drag the different Sprite Prefabs you want to randomly clone here.")]
    public GameObject[] spritePrefabs; // Notice the [] brackets! This makes it a list.
    
    [Tooltip("Time in seconds between each spawn.")]
    public float spawnInterval = 3f;

    void Start()
    {
        // Start the continuous spawning loop when the game begins
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // The while (true) loop runs forever while the script is active
        while (true)
        {
            // Wait for the specified amount of seconds
            yield return new WaitForSeconds(spawnInterval);

            // Safety check: Only try to spawn if we actually put something in the list!
            if (spritePrefabs.Length > 0)
            {
                // Pick a random index from 0 up to (but not including) the length of the list
                int randomIndex = Random.Range(0, spritePrefabs.Length);

                // Clone the randomly chosen sprite from the list
                Instantiate(spritePrefabs[randomIndex], transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Your spawner doesn't have any prefabs assigned to it!");
            }
        }
    }
}