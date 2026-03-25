using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 7f;

    [Header("Detection Settings")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Skill 1 Settings (Stationary Slash)")]
    public GameObject slashSkillEffect; 
    public float skillDuration = 0.5f;  
    public float skillCooldown = 2.0f;  
    public KeyCode skillKey = KeyCode.E; 

    // --- NEW: JUGGERNAUT SPIN SETTINGS ---
    [Header("Skill 2 Settings (Spin)")]
    public float spinDuration = 3.0f;   // How long the spin lasts
    public float spinCooldown = 5.0f;   // Cooldown time
    public KeyCode spinKey = KeyCode.X; // Hotkey is 'X'
    
    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private Vector3 initialScale;

    // State Variables
    private bool isSkillActive = false; // Tracks the "E" skill (stops movement)
    private bool isSpinning = false;    // Tracks the "X" skill (allows movement)
    
    private float lastSkillTime = -100f; 
    private float lastSpinTime = -100f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;
        
        if (slashSkillEffect != null) 
            slashSkillEffect.SetActive(false);
    }

    void Update()
    {
        CheckGround(); 
        
        Move();
        Jump();
        AttackInput(); 
        SkillInput();  // "E" Skill
        SpinInput();   // <--- NEW: "X" Spin Skill
    }

    void Move()
    {
        // 1. If we are using the stationary "E" skill, stop moving.
        if (isSkillActive) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            anim.SetFloat("Speed", 0);
            return; 
        }

        // 2. Normal movement (Works even if isSpinning is true!)
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (isGrounded)
        {
            // Only update running animation if we AREN'T spinning
            // (This prevents the Run animation from overriding the Spin animation)
            if (!isSpinning)
            {
                anim.SetFloat("Speed", Mathf.Abs(moveX));
            }
        }
        else
        {
             // If spinning in air, keep spin animation, otherwise set speed 0
            if (!isSpinning) anim.SetFloat("Speed", 0); 
        }

        // Flip Character
        if (moveX > 0) transform.localScale = initialScale;
        else if (moveX < 0) transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
    }

    void Jump()
    {
        if (isSkillActive) return; // Cannot jump during "E" skill

        // You CAN jump while spinning (optional: remove !isSpinning check if you want to block it)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // Only show jump animation if not spinning
            if (!isSpinning) anim.SetBool("isJumping", true);
        }
    }

    void AttackInput()
    {
        if (isSkillActive || isSpinning) return; // Cannot attack while casting either skill

        if (Input.GetButtonDown("Fire1") && isGrounded)
        {
            int randomAttack = Random.Range(1, 4);
            anim.SetInteger("AttackIndex", randomAttack);
            anim.SetTrigger("Attack");
        }
    }

    void SkillInput()
    {
        if (Input.GetKeyDown(skillKey) && Time.time >= lastSkillTime + skillCooldown && isGrounded)
        {
            if (!isSkillActive && !isSpinning) 
            {
                StartCoroutine(CastSkill());
            }
        }
    }

    // --- NEW FUNCTION: HANDLES THE SPIN ---
void SpinInput()
    {
        // 1. Check if the key is actually being pressed
        if (Input.GetKeyDown(spinKey))
        {
        

            // 2. Check the conditions
            bool cooldownReady = Time.time >= lastSpinTime + spinCooldown;
            bool notActive = !isSpinning && !isSkillActive;

           
            if (cooldownReady && notActive)
            {
                StartCoroutine(CastSpin());
            }
            else
            {
            }
        }
    }

    IEnumerator CastSpin()
    {
        isSpinning = true;
        lastSpinTime = Time.time;

        // Trigger the animation in Unity Animator
        anim.SetTrigger("Spin"); 

        // Wait for the duration
        yield return new WaitForSeconds(spinDuration);

        // End Spin
        isSpinning = false;
        
        // Force update animation state back to idle/run
        anim.Play("Idle"); 
    }

    IEnumerator CastSkill()
    {
        isSkillActive = true;
        lastSkillTime = Time.time;

        anim.SetTrigger("SkillCast"); 

        if (slashSkillEffect != null) slashSkillEffect.SetActive(true);

        yield return new WaitForSeconds(skillDuration);

        if (slashSkillEffect != null) slashSkillEffect.SetActive(false);

        isSkillActive = false;
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded)
        {
            if(!isSpinning) anim.SetBool("isJumping", false);
        }
        else
        {
            if(!isSpinning) anim.SetBool("isJumping", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}