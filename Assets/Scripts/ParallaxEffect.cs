using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float lengthX, lengthY;
    private Vector3 startPos;
    public GameObject cam;
    
    // X is horizontal multiplier, Y is vertical multiplier
    // 0 = follows camera exactly, 1 = static background
    public Vector2 parallaxEffect; 

    void Start()
{
    // Auto-assignment: This saves you from dragging the camera every time
    if (cam == null)
    {
        cam = Camera.main.gameObject; 
    }

    startPos = transform.position;
    
    // Bounds check to ensure the sprite repeats correctly
    lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
    lengthY = GetComponent<SpriteRenderer>().bounds.size.y;
}

    void FixedUpdate()
    {
        // Calculate relative distance moved from the camera's perspective
        Vector3 distance = new Vector3(
            cam.transform.position.x * parallaxEffect.x,
            cam.transform.position.y * parallaxEffect.y,
            0
        );

        transform.position = new Vector3(startPos.x + distance.x, startPos.y + distance.y, transform.position.z);

        // --- Optional: Infinite Looping Logic ---
        Vector3 temp = new Vector3(
            cam.transform.position.x * (1 - parallaxEffect.x),
            cam.transform.position.y * (1 - parallaxEffect.y),
            0
        );

        // Horizontal Loop
        if (temp.x > startPos.x + lengthX) startPos.x += lengthX;
        else if (temp.x < startPos.x - lengthX) startPos.x -= lengthX;

        // Vertical Loop
        if (temp.y > startPos.y + lengthY) startPos.y += lengthY;
        else if (temp.y < startPos.y - lengthY) startPos.y -= lengthY;
    }
}