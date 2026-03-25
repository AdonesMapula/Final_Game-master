using UnityEngine;

public class OrcEnemy : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float sightRange = 1.2f; 
    public float attackRange = 0.3f; 

    [Header("Movement Speeds")]
    public float walkSpeed = 0.3f; 
    public float runSpeed = 1.0f; 

    [Header("Combat")]
    public float attackCooldown = 1.5f; 
    private float nextAttackTime = 0f;  

    [Header("Patrol Limits (Invisible Fence)")]
    public Transform leftLimit;  
    public Transform rightLimit; 
    private Transform currentTarget;

    [Header("Enemy Interaction")]
    public GameObject angryBubble; 
    public string enemyTag = "Orc_Enemy"; 

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentTarget = rightLimit;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (player == null || leftLimit == null || rightLimit == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        bool playerInSight = distanceToPlayer <= sightRange;
        bool playerInsideTerritory = player.position.x >= leftLimit.position.x && player.position.x <= rightLimit.position.x;

        // --- THE FIX: Turn on the bubble for the Player OR other Orcs ---
        bool sawAnotherEnemy = CheckForOtherEnemies();

        if (angryBubble != null)
        {
            angryBubble.SetActive(playerInSight || sawAnotherEnemy);
        }
        // ----------------------------------------------------------------

        if (distanceToPlayer <= attackRange)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown; 
            }
            else
            {
                anim.SetBool("IsAttacking", false);
            }
        }
        else if (playerInSight && playerInsideTerritory)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetInteger("MoveState", 1); 

        Vector2 targetPos = new Vector2(currentTarget.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);

        spriteRenderer.flipX = currentTarget.position.x < transform.position.x;

        if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.2f)
        {
            if (currentTarget == leftLimit)
            {
                currentTarget = rightLimit;
            }
            else
            {
                currentTarget = leftLimit;
            }
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetInteger("MoveState", 2); 

        float clampedX = Mathf.Clamp(player.position.x, leftLimit.position.x, rightLimit.position.x);
        Vector2 targetPos = new Vector2(clampedX, transform.position.y);
        
        transform.position = Vector2.MoveTowards(transform.position, targetPos, runSpeed * Time.deltaTime);

        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    void AttackPlayer()
    {
        anim.SetInteger("MoveState", 0); 
        anim.SetBool("IsAttacking", true); 
    }

    // Notice we changed this to return a "bool" (true or false)
    private bool CheckForOtherEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, sightRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(enemyTag) && hit.gameObject != this.gameObject)
            {
                return true; // Found one!
            }
        }
        return false; // Didn't find any
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        if (leftLimit != null && rightLimit != null) 
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector2(leftLimit.position.x, transform.position.y - 1), new Vector2(leftLimit.position.x, transform.position.y + 1));
            Gizmos.DrawLine(new Vector2(rightLimit.position.x, transform.position.y - 1), new Vector2(rightLimit.position.x, transform.position.y + 1));
        }
    }
}