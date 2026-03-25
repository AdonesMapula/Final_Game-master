using UnityEngine;

public class GlobalPlayerState : MonoBehaviour
{
    public static GlobalPlayerState Instance;

    public int currentHealth = -1;
    public int keysCollected = 0;
    public int deathCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetState(int startingHealth)
    {
        currentHealth = startingHealth;
        keysCollected = 0;
        deathCount = 0;
    }
}