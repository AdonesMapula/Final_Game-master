using UnityEngine;
using System.Collections;

public class PngMarqueeUI : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer targetRenderer; // PNG sprite renderer
    public Transform moveTarget;          // object to jitter (defaults to this transform)

    [Header("Flicker")]
    public bool playOnStart = true;
    public bool useUnscaledTime = true;
    [Range(0f, 1f)] public float minAlpha = 0.25f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float minInterval = 0.03f;
    public float maxInterval = 0.12f;

    [Header("Glitch Move")]
    public bool enableJitter = true;
    public float jitterX = 0.05f;         // horizontal glitch strength
    public float jitterY = 0.03f;         // vertical glitch strength
    public float jitterChance = 0.6f;     // chance each tick to jitter
    public bool snapBackEachTick = true;  // reset to original pos before applying new jitter

    private Coroutine glitchRoutine;
    private Vector3 baseLocalPos;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (moveTarget == null)
            moveTarget = transform;

        baseLocalPos = moveTarget.localPosition;
    }

    void OnEnable()
    {
        if (playOnStart)
            StartGlitch();
    }

    void OnDisable()
    {
        StopGlitch(true);
    }

    public void StartGlitch()
    {
        if (targetRenderer == null) return;

        if (glitchRoutine != null)
            StopCoroutine(glitchRoutine);

        baseLocalPos = moveTarget.localPosition;
        glitchRoutine = StartCoroutine(GlitchRoutine());
    }

    public void StopGlitch(bool resetState = true)
    {
        if (glitchRoutine != null)
        {
            StopCoroutine(glitchRoutine);
            glitchRoutine = null;
        }

        if (resetState)
        {
            if (targetRenderer != null)
            {
                Color c = targetRenderer.color;
                c.a = maxAlpha;
                targetRenderer.color = c;
            }

            if (moveTarget != null)
                moveTarget.localPosition = baseLocalPos;
        }
    }

    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            // Flicker alpha
            Color c = targetRenderer.color;
            c.a = Random.Range(minAlpha, maxAlpha);
            targetRenderer.color = c;

            // Glitch movement
            if (enableJitter && moveTarget != null)
            {
                if (snapBackEachTick)
                    moveTarget.localPosition = baseLocalPos;

                if (Random.value <= jitterChance)
                {
                    float x = Random.Range(-jitterX, jitterX);
                    float y = Random.Range(-jitterY, jitterY);
                    moveTarget.localPosition = baseLocalPos + new Vector3(x, y, 0f);
                }
            }

            float wait = Random.Range(minInterval, maxInterval);
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);
        }
    }
}