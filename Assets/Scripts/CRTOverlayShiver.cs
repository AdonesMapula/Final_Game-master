using UnityEngine;
using UnityEngine.UI;

public class CRTOverlayShiver : MonoBehaviour
{
    [Header("Shiver Movement")]
    public float jitterAmount = 1.5f;     // how many pixels/units it shakes
    public float jitterSpeed = 30f;       // how often it changes direction

    [Header("Scale Wobble")]
    public float scaleWobbleAmount = 0.01f; // small scale change
    public float scaleWobbleSpeed = 8f;

    [Header("Alpha Flicker")]
    public bool useAlphaFlicker = true;
    public float flickerAmount = 0.08f;   // how much opacity changes
    public float flickerSpeed = 20f;

    private Vector3 originalPos;
    private Vector3 originalScale;

    private Image uiImage;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        originalPos = transform.localPosition;
        originalScale = transform.localScale;

        uiImage = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiImage != null)
            originalColor = uiImage.color;
        else if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Random jitter
        float x = (Mathf.PerlinNoise(Time.time * jitterSpeed, 0f) - 0.5f) * 2f * jitterAmount;
        float y = (Mathf.PerlinNoise(0f, Time.time * jitterSpeed) - 0.5f) * 2f * jitterAmount;

        transform.localPosition = originalPos + new Vector3(x, y, 0f);

        // Slight scale wobble
        float scaleOffset = Mathf.Sin(Time.time * scaleWobbleSpeed) * scaleWobbleAmount;
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, 0f);

        // Alpha flicker
        if (useAlphaFlicker)
        {
            float flicker = 1f - (Mathf.PerlinNoise(Time.time * flickerSpeed, 1f) * flickerAmount);

            if (uiImage != null)
            {
                Color c = originalColor;
                c.a = originalColor.a * flicker;
                uiImage.color = c;
            }
            else if (spriteRenderer != null)
            {
                Color c = originalColor;
                c.a = originalColor.a * flicker;
                spriteRenderer.color = c;
            }
        }
    }
}