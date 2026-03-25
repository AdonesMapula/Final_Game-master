using UnityEngine;

public class CloneDestroyer : MonoBehaviour
{
    [Tooltip("Type the exact name of the Layer your clones are on.")]
    public string cloneLayerName = "CloneLayer";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Unity stores layers as numbers (0-31). 
        // LayerMask.NameToLayer converts our string name into that specific number.
        if (other.gameObject.layer == LayerMask.NameToLayer(cloneLayerName))
        {
            // Destroy the specific object that entered the trigger
            Destroy(other.gameObject);
        }
    }
}