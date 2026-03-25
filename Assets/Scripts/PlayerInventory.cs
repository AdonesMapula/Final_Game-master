using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public int keysCollected = 0;
    public int maxKeys = 5;

    [Header("Key Bar - assign the parent GameObjects")]
    [Tooltip("Drag the parent with ON/filled key sprites here")]
    public Transform keysOnParent;

    [Tooltip("Drag the parent with OFF/empty key sprites here")]
    public Transform keysOffParent;

    private SpriteRenderer[] srOn;
    private SpriteRenderer[] srOff;

    private void Awake()
    {
        // Auto-find parents by name if not assigned
        if (keysOnParent == null)
        {
            GameObject found = GameObject.Find("key_0");
            if (found != null) keysOnParent = found.transform;
        }

        if (keysOffParent == null)
        {
            GameObject found = GameObject.Find("key_off");
            if (found != null) keysOffParent = found.transform;
        }

        srOn = GetDirectChildRenderers(keysOnParent);
        srOff = GetDirectChildRenderers(keysOffParent);

        int barLength = Mathf.Max(srOn.Length, srOff.Length);
        if (barLength > 0)
            maxKeys = Mathf.Min(maxKeys, barLength);

        Debug.Log($"[KeyBar] ON segments: {srOn.Length} | OFF segments: {srOff.Length} | maxKeys set to: {maxKeys}");
    }

    private SpriteRenderer[] GetDirectChildRenderers(Transform parent)
    {
        if (parent == null) return new SpriteRenderer[0];

        var list = new System.Collections.Generic.List<SpriteRenderer>();

        foreach (Transform child in parent)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
                list.Add(sr);
        }

        return list.ToArray();
    }

    private void Start()
    {
        UpdateKeyBar();
    }

    public void AddKey()
    {
        if (keysCollected >= maxKeys)
            return;

        keysCollected++;
        Debug.Log("Nakakuha ug key! Total keys: " + keysCollected);
        UpdateKeyBar();
    }

    private void UpdateKeyBar()
    {
        int total = Mathf.Max(srOn.Length, srOff.Length);

        for (int i = 0; i < total; i++)
        {
            bool filled = i < keysCollected;

            if (i < srOn.Length && srOn[i] != null)
            {
                Color c = srOn[i].color;
                c.a = filled ? 1f : 0f;
                srOn[i].color = c;
            }

            if (i < srOff.Length && srOff[i] != null)
            {
                Color c = srOff[i].color;
                c.a = filled ? 0f : 1f;
                srOff[i].color = c;
            }
        }
    }
}