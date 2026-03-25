using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PngRetroViceColorGlow : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer targetRenderer; // PNG sprite renderer

    [Header("Playback")]
    public bool playOnStart = true;
    public bool useUnscaledTime = true;
    public float colorChangeInterval = 1.5f;

    [Header("Retro Vice Color")]
    [Range(0f, 1f)] public float minBrightness = 0.82f;
    [Range(0f, 1f)] public float maxBrightness = 1f;
    [Range(0f, 1f)] public float minSaturation = 0.72f;
    [Range(0f, 1f)] public float maxSaturation = 1f;

    [Header("Glow Outline")]
    public bool enableGlowOutline = true;
    public bool glowFollowsCurrentColor = true;
    public Color glowTint = new Color(0.8f, 1f, 1f, 0.4f);
    [Range(0.001f, 0.08f)] public float outlineSize = 0.018f;
    [Range(0f, 1f)] public float glowAlpha = 0.45f;

    private Coroutine colorRoutine;
    private readonly List<SpriteRenderer> glowRenderers = new List<SpriteRenderer>();

    // Neon vice anchors: magenta, pink, violet, cyan, electric blue.
    private readonly float[] viceHueAnchors = { 0.86f, 0.92f, 0.78f, 0.51f, 0.58f };

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        BuildGlowOutline();
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

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        ApplyColor(GetRetroViceColor(1f));
        colorRoutine = StartCoroutine(GlitchRoutine());
    }

    public void StopGlitch(bool resetState = true)
    {
        if (colorRoutine != null)
        {
            StopCoroutine(colorRoutine);
            colorRoutine = null;
        }

        if (resetState)
            ApplyColor(Color.white);
    }

    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            ApplyColor(GetRetroViceColor(1f));

            if (useUnscaledTime) yield return new WaitForSecondsRealtime(colorChangeInterval);
            else yield return new WaitForSeconds(colorChangeInterval);
        }
    }

    private Color GetRetroViceColor(float alpha)
    {
        // Pick a vice anchor, then jitter around it for slight variety.
        float anchor = viceHueAnchors[Random.Range(0, viceHueAnchors.Length)];
        float hue = anchor + Random.Range(-0.025f, 0.025f);
        if (hue < 0f) hue += 1f;
        if (hue > 1f) hue -= 1f;

        float saturation = Random.Range(minSaturation, maxSaturation);
        float value = Random.Range(minBrightness, maxBrightness);
        Color vice = Color.HSVToRGB(hue, saturation, value);
        vice.a = alpha;
        return vice;
    }

    private void ApplyColor(Color mainColor)
    {
        if (targetRenderer == null) return;

        targetRenderer.color = mainColor;

        if (!enableGlowOutline) return;

        if (glowRenderers.Count == 0)
            BuildGlowOutline();

        Color outlineColor = glowFollowsCurrentColor ? mainColor : glowTint;
        outlineColor.a = glowAlpha;

        foreach (var gr in glowRenderers)
        {
            if (gr == null) continue;
            gr.sprite = targetRenderer.sprite;
            gr.flipX = targetRenderer.flipX;
            gr.flipY = targetRenderer.flipY;
            gr.color = outlineColor;
            gr.enabled = targetRenderer.enabled;
        }
    }

    private void BuildGlowOutline()
    {
        if (targetRenderer == null) return;

        foreach (var gr in glowRenderers)
            if (gr != null) Destroy(gr.gameObject);
        glowRenderers.Clear();

        Vector3[] offsets =
        {
            new Vector3(outlineSize, 0f, 0f),
            new Vector3(-outlineSize, 0f, 0f),
            new Vector3(0f, outlineSize, 0f),
            new Vector3(0f, -outlineSize, 0f),
            new Vector3(outlineSize, outlineSize, 0f),
            new Vector3(outlineSize, -outlineSize, 0f),
            new Vector3(-outlineSize, outlineSize, 0f),
            new Vector3(-outlineSize, -outlineSize, 0f)
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            GameObject glow = new GameObject("GlowOutline_" + i);
            glow.transform.SetParent(targetRenderer.transform, false);
            glow.transform.localPosition = offsets[i];

            SpriteRenderer gr = glow.AddComponent<SpriteRenderer>();
            gr.sprite = targetRenderer.sprite;
            gr.sortingLayerID = targetRenderer.sortingLayerID;
            gr.sortingOrder = targetRenderer.sortingOrder - 1;
            gr.color = glowTint;

            glowRenderers.Add(gr);
        }
    }
}