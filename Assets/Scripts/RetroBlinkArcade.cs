using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RetroBlinkArcade : MonoBehaviour
{
    public float visibleTime = 0.6f;
    public float hiddenTime = 0.3f;

    private Graphic uiGraphic;
    private TMP_Text tmpText;
    private SpriteRenderer spriteRenderer;

    private float timer;
    private bool isVisible = true;

    void Start()
    {
        uiGraphic = GetComponent<Graphic>();
        tmpText = GetComponent<TMP_Text>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        SetVisible(true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isVisible && timer >= visibleTime)
        {
            timer = 0f;
            isVisible = false;
            SetVisible(false);
        }
        else if (!isVisible && timer >= hiddenTime)
        {
            timer = 0f;
            isVisible = true;
            SetVisible(true);
        }
    }

    void SetVisible(bool visible)
    {
        if (uiGraphic != null)
            uiGraphic.enabled = visible;

        if (tmpText != null)
            tmpText.enabled = visible;

        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }
}