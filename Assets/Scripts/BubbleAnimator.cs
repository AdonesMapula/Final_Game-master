using UnityEngine;
using System.Collections;

public class BubbleAnimator : MonoBehaviour
{
    private Vector3 originalScale;

    // Awake runs the very first time the game starts
    void Awake()
    {
        // Memorize the exact weird size you set in the Inspector so we don't ruin it!
        originalScale = transform.localScale; 
    }

    // OnEnable runs automatically EVERY TIME this object gets turned on (.SetActive(true))
    void OnEnable()
    {
        // Stop any leftover animations just in case it was toggled super fast
        StopAllCoroutines(); 
        // Start the bouncy pop-in animation
        StartCoroutine(PopIn());
    }

    /// <summary>
    /// Call this method from your other scripts when you want to close the bubble!
    /// Example: myBubbleAnimator.HideBubble();
    /// </summary>
    public void HideBubble()
    {
        StopAllCoroutines();
        StartCoroutine(PopOut());
    }

    private IEnumerator PopIn()
    {
        // 1. Start completely invisible (Size 0)
        transform.localScale = Vector3.zero;
        
        float timer = 0f;
        float popUpSpeed = 0.15f; // Takes 0.15 seconds to pop up
        
        // Calculate a size slightly bigger than normal for the "bounce" effect
        Vector3 stretchedScale = originalScale * 1.3f;

        // 2. Quickly blow up like a balloon to the stretched size
        while (timer < popUpSpeed)
        {
            timer += Time.deltaTime;
            // Lerp smoothly transitions between two sizes over time
            transform.localScale = Vector3.Lerp(Vector3.zero, stretchedScale, timer / popUpSpeed);
            yield return null; // Wait for the next frame
        }

        // 3. Settle back down to the normal size
        timer = 0f;
        float settleDownSpeed = 0.1f; // Takes 0.1 seconds to settle

        while (timer < settleDownSpeed)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(stretchedScale, originalScale, timer / settleDownSpeed);
            yield return null;
        }

        // 4. Force it to be exactly the original size at the end just in case
        transform.localScale = originalScale;
    }

    private IEnumerator PopOut()
    {
        float timer = 0f;
        
        // We do a tiny "anticipation" bulge before shrinking. 
        // It makes the animation feel much more alive!
        float anticipationSpeed = 0.05f; 
        Vector3 stretchedScale = originalScale * 1.1f;

        // 1. Bulge out just a tiny bit
        while (timer < anticipationSpeed)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, stretchedScale, timer / anticipationSpeed);
            yield return null;
        }

        // 2. Quickly shrink down to nothing
        timer = 0f;
        float shrinkSpeed = 0.20f;

        while (timer < shrinkSpeed)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(stretchedScale, Vector3.zero, timer / shrinkSpeed);
            yield return null;
        }

        // 3. Force it to size 0 at the end
        transform.localScale = Vector3.zero;

        // 4. Now that the animation is totally finished, turn the GameObject off!
        gameObject.SetActive(false);
    }
}