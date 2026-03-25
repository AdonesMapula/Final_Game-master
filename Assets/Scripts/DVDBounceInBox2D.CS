using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DVDBounceInBox2D : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float speed = 4f;
    public Vector2 startDirection = new Vector2(1f, 1f);
    public bool randomColorOnBounce = true;
    public bool useUnscaledTime = true; // works even when Time.timeScale = 0

    [Header("Boundary")]
    public BoxCollider2D boundaryBox;
    public float innerPadding = 0f;

    private SpriteRenderer sr;
    private Vector2 dir;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        dir = startDirection.normalized;
        if (dir == Vector2.zero) dir = Vector2.one.normalized;
    }

    void Update()
    {
        if (boundaryBox == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        Vector3 pos = transform.position;
        pos += (Vector3)(dir * speed * dt);

        Bounds b = boundaryBox.bounds;

        float minX = b.min.x + sr.bounds.extents.x + innerPadding;
        float maxX = b.max.x - sr.bounds.extents.x - innerPadding;
        float minY = b.min.y + sr.bounds.extents.y + innerPadding;
        float maxY = b.max.y - sr.bounds.extents.y - innerPadding;

        bool bounced = false;

        if (pos.x <= minX)
        {
            pos.x = minX;
            dir.x *= -1f;
            bounced = true;
        }
        else if (pos.x >= maxX)
        {
            pos.x = maxX;
            dir.x *= -1f;
            bounced = true;
        }

        if (pos.y <= minY)
        {
            pos.y = minY;
            dir.y *= -1f;
            bounced = true;
        }
        else if (pos.y >= maxY)
        {
            pos.y = maxY;
            dir.y *= -1f;
            bounced = true;
        }

        transform.position = pos;

        if (bounced && randomColorOnBounce)
        {
            sr.color = new Color(Random.value, Random.value, Random.value, 1f);
        }
    }
}