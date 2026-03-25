using UnityEngine;

public class RandomMovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float blockSize = 0.32f;      // Your tile/block size
    [SerializeField] private float moveSpeed = 5f;         // How fast it moves
    [SerializeField] private float detectionRange = 2f;    // How close player needs to be
    
    [Header("Boundaries (Optional)")]
    [SerializeField] private int maxBlocksFromStart = 3;   // Max blocks left/right from starting position
    
    private Vector3 targetPosition;
    private Vector3 startingPosition;                      // Remember where platform started
    private bool isMoving = false;
    private bool hasMovedThisJump = false;
    private Transform player;
    private Rigidbody2D playerRb;
    
    void Start()
    {
        startingPosition = transform.position;
        targetPosition = transform.position;
        
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
    }
    
    void Update()
    {
        DetectPlayerJump();
        
        // Smoothly move platform to target position
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            
            // Check if reached target
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
    
    void DetectPlayerJump()
    {
        if (player == null || playerRb == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            bool isJumping = playerRb.linearVelocity.y > 0.1f;
            bool isBelowPlatform = player.position.y < transform.position.y;
            
            if (isJumping && isBelowPlatform && !hasMovedThisJump && !isMoving)
            {
                MoveOneBlock();
                hasMovedThisJump = true;
            }
        }
        
        // Reset when player stops jumping or moves away
        if (playerRb.linearVelocity.y <= 0 || distanceToPlayer > detectionRange)
        {
            hasMovedThisJump = false;
        }
    }
    
    void MoveOneBlock()
    {
        // Randomly choose left (-1) or right (1)
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;
        
        // Calculate new position (exactly 1 block)
        float newX = transform.position.x + (direction * blockSize);
        
        // Calculate boundaries based on starting position
        float minX = startingPosition.x - (maxBlocksFromStart * blockSize);
        float maxX = startingPosition.x + (maxBlocksFromStart * blockSize);
        
        // Check if new position is within bounds
        if (newX < minX || newX > maxX)
        {
            // Try opposite direction
            newX = transform.position.x + (-direction * blockSize);
        }
        
        // Final boundary check
        newX = Mathf.Clamp(newX, minX, maxX);
        
        targetPosition = new Vector3(newX, transform.position.y, transform.position.z);
        isMoving = true;
    }
    
    // Visualize in editor
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Movement boundaries
        Gizmos.color = Color.cyan;
        Vector3 startPos = Application.isPlaying ? startingPosition : transform.position;
        float minX = startPos.x - (maxBlocksFromStart * blockSize);
        float maxX = startPos.x + (maxBlocksFromStart * blockSize);
        
        // Draw boundary lines
        Vector3 leftBound = new Vector3(minX, transform.position.y, 0);
        Vector3 rightBound = new Vector3(maxX, transform.position.y, 0);
        Gizmos.DrawLine(leftBound + Vector3.up * 0.5f, leftBound + Vector3.down * 0.5f);
        Gizmos.DrawLine(rightBound + Vector3.up * 0.5f, rightBound + Vector3.down * 0.5f);
        Gizmos.DrawLine(leftBound, rightBound);
        
        // Draw block grid
        Gizmos.color = Color.gray;
        for (int i = -maxBlocksFromStart; i <= maxBlocksFromStart; i++)
        {
            Vector3 blockPos = new Vector3(startPos.x + (i * blockSize), transform.position.y, 0);
            Gizmos.DrawWireCube(blockPos, new Vector3(blockSize, blockSize, 0));
        }
    }
}