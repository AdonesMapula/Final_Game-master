using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGamePortal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string creditsSceneName = "EndCredits";

    [Header("Warning Sprite")]
    public Sprite warningSprite;
    public float warningDisplayTime = 3f;

    [Header("Sprite Settings")]
    public float spriteScale = 1f;
    public int sortingOrder = 999;
    public Vector2 screenOffset = Vector2.zero;

    [Header("Animation")]
    public bool pulseAnimation = true;
    public float pulseSpeed = 5f;
    public float pulseIntensity = 0.1f;
    public bool fadeEffect = true;
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 0.5f;

    [Header("Audio (Optional)")]
    public AudioClip warningSound;
    public AudioClip successSound;

    private GameObject warningObject;
    private SpriteRenderer warningSpriteRenderer;
    private AudioSource audioSource;
    private Camera mainCamera;
    private bool isShowingWarning = false;
    private Vector3 originalScale;

    private void Start()
    {
        mainCamera = Camera.main;
        CreateWarningObject();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (warningSound != null || successSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Debug.Log("[EndGamePortal] Initialized. Sprite assigned: " + (warningSprite != null));
    }

    private void CreateWarningObject()
    {
        if (warningSprite == null)
        {
            Debug.LogError("[EndGamePortal] NO WARNING SPRITE ASSIGNED!");
            return;
        }

        warningObject = new GameObject("WarningSprite");
        warningSpriteRenderer = warningObject.AddComponent<SpriteRenderer>();
        warningSpriteRenderer.sprite = warningSprite;
        warningSpriteRenderer.sortingOrder = sortingOrder;

        warningObject.transform.localScale = Vector3.one * spriteScale;
        originalScale = warningObject.transform.localScale;

        warningObject.SetActive(false);

        Debug.Log("[EndGamePortal] Warning sprite object created!");
    }

    private void Update()
    {
        if (!isShowingWarning || warningObject == null)
            return;

        UpdateWarningPosition();

        if (pulseAnimation)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            warningObject.transform.localScale = originalScale * scale;
        }
    }

    private void UpdateWarningPosition()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            float zPosition = 0f;

            warningObject.transform.position = new Vector3(
                cameraPos.x + screenOffset.x,
                cameraPos.y + screenOffset.y,
                zPosition
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[EndGamePortal] Trigger entered by: " + other.name + " | Tag: " + other.tag);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[EndGamePortal] Not player, ignoring.");
            return;
        }

        // UPDATED: Get PlayerController instead of PlayerInventory
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            Debug.LogError("[EndGamePortal] PlayerController NOT FOUND on player!");
            return;
        }

        // UPDATED: Access keysCollected and maxKeys from PlayerController
        int currentKeys = player.keysCollected;
        int requiredKeys = player.maxKeys;

        Debug.Log($"[EndGamePortal] Keys: {currentKeys}/{requiredKeys}");

        if (currentKeys >= requiredKeys)
        {
            Debug.Log("[EndGamePortal] SUCCESS! All keys collected! Loading credits...");

            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }

            // Optional: Small delay to let sound play
            // StartCoroutine(LoadCreditsAfterDelay(0.5f));
            
            SceneManager.LoadScene(creditsSceneName);
        }
        else
        {
            Debug.Log($"[EndGamePortal] Kuwang ug yawi! {currentKeys}/{requiredKeys}");
            ShowWarning();
        }
    }

    private System.Collections.IEnumerator LoadCreditsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(creditsSceneName);
    }

    private void ShowWarning()
    {
        if (isShowingWarning)
        {
            Debug.Log("[EndGamePortal] Already showing warning, skipping.");
            return;
        }

        if (warningObject == null)
        {
            Debug.LogError("[EndGamePortal] Warning object is NULL!");
            return;
        }

        isShowingWarning = true;

        warningObject.SetActive(true);
        UpdateWarningPosition();

        Debug.Log("[EndGamePortal] Warning sprite ACTIVATED at position: " + warningObject.transform.position);

        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }

        if (fadeEffect)
        {
            StartCoroutine(FadeWarning());
        }
        else
        {
            Invoke(nameof(HideWarning), warningDisplayTime);
        }
    }

    private System.Collections.IEnumerator FadeWarning()
    {
        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInTime));
        yield return new WaitForSeconds(warningDisplayTime);
        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutTime));
        HideWarning();
    }

    private System.Collections.IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);

            if (warningSpriteRenderer != null)
            {
                Color c = warningSpriteRenderer.color;
                c.a = alpha;
                warningSpriteRenderer.color = c;
            }

            yield return null;
        }

        if (warningSpriteRenderer != null)
        {
            Color c = warningSpriteRenderer.color;
            c.a = to;
            warningSpriteRenderer.color = c;
        }
    }

    private void HideWarning()
    {
        Debug.Log("[EndGamePortal] Hiding warning sprite.");

        if (warningObject != null)
        {
            warningObject.SetActive(false);
            warningObject.transform.localScale = originalScale;

            if (warningSpriteRenderer != null)
            {
                Color c = warningSpriteRenderer.color;
                c.a = 1f;
                warningSpriteRenderer.color = c;
            }
        }

        isShowingWarning = false;
    }

    private void OnDestroy()
    {
        if (warningObject != null)
        {
            Destroy(warningObject);
        }
    }
}   