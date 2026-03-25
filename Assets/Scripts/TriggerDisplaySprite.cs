using UnityEngine;

public class TriggerDisplaySprite : MonoBehaviour
{
    [Header("Sprite to Display")]
    [Tooltip("Drag your sprite here directly")]
    public Sprite displaySprite;

    [Header("Display Settings")]
    public float spriteScale = 1f;
    public int sortingOrder = 100;
    public Vector2 offset = new Vector2(0f, 1.5f); // Position above trigger object

    [Header("Animation Type")]
    public bool useBob = true;
    public bool usePulse = true;

    [Header("Bob Settings (Up & Down Movement)")]
    public float bobSpeed = 3f;
    public float bobHeight = 0.25f;

    [Header("Pulse Settings (Scale In & Out)")]
    public float pulseSpeed = 4f;
    public float pulseIntensity = 0.15f;

    [Header("Display Duration")]
    [Tooltip("0 = stays forever while in trigger")]
    public float displayDuration = 0f;
    public bool hideOnExit = true;

    [Header("Audio (Optional)")]
    public AudioClip showSound;
    public AudioClip hideSound;

    // Private variables
    private GameObject spriteObject;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isShowing = false;
    private Vector3 originalScale;
    private Vector3 basePosition;
    private float showTime;

    private void Start()
    {
        CreateSpriteObject();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (showSound != null || hideSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void CreateSpriteObject()
    {
        if (displaySprite == null)
        {
            Debug.LogWarning("[TriggerDisplaySprite] No sprite assigned on: " + gameObject.name);
            return;
        }

        spriteObject = new GameObject("DisplaySprite");
        spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = displaySprite;
        spriteRenderer.sortingOrder = sortingOrder;

        spriteObject.transform.localScale = Vector3.one * spriteScale;
        originalScale = spriteObject.transform.localScale;

        // Position relative to this object
        basePosition = transform.position + (Vector3)offset;
        spriteObject.transform.position = basePosition;

        spriteObject.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing || spriteObject == null)
            return;

        // Update base position (in case object moves)
        basePosition = transform.position + (Vector3)offset;

        // Apply animations
        Vector3 newPosition = basePosition;
        Vector3 newScale = originalScale;

        // Bob animation (up and down)
        if (useBob)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            newPosition.y += bobOffset;
        }

        // Pulse animation (scale)
        if (usePulse)
        {
            float pulseScale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            newScale = originalScale * pulseScale;
        }

        spriteObject.transform.position = newPosition;
        spriteObject.transform.localScale = newScale;

        // Check duration
        if (displayDuration > 0f && Time.time - showTime >= displayDuration)
        {
            HideSprite();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        ShowSprite();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hideOnExit)
        {
            HideSprite();
        }
    }

    public void ShowSprite()
    {
        if (spriteObject == null)
            return;

        isShowing = true;
        showTime = Time.time;
        spriteObject.SetActive(true);

        if (audioSource != null && showSound != null)
        {
            audioSource.PlayOneShot(showSound);
        }
    }

    public void HideSprite()
    {
        if (spriteObject == null)
            return;

        isShowing = false;
        spriteObject.SetActive(false);
        spriteObject.transform.localScale = originalScale;

        if (audioSource != null && hideSound != null)
        {
            audioSource.PlayOneShot(hideSound);
        }
    }

    private void OnDestroy()
    {
        if (spriteObject != null)
        {
            Destroy(spriteObject);
        }
    }

    // Visual helper in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 spritePos = transform.position + (Vector3)offset;
        Gizmos.DrawWireSphere(spritePos, 0.3f);
        Gizmos.DrawLine(transform.position, spritePos);
    }
}