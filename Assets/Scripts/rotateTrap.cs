using UnityEngine;

public class rotateTrap : MonoBehaviour
{
    public float rotationSpeed = -200f;

    void Update()
    {
        // Keep the object perfectly circular
        float scale = transform.localScale.x;
        transform.localScale = new Vector3(scale, scale, 1f);

        // Rotate the trap
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}   