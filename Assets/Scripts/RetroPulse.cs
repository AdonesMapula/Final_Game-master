using UnityEngine;

public class RetroPulse : MonoBehaviour
{
    public float growAmount = 1.2f;
    public float pulseSpeed = 8f;
    public int steps = 6;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        t = Mathf.Round(t * steps) / steps; // makes movement choppy

        float currentScale = Mathf.Lerp(1f, growAmount, t);
        transform.localScale = originalScale * currentScale;
    }
}