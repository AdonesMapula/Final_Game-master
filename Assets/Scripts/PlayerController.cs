using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 1.75f;
    public float jumpForce = 3.5f;

    [Header("Custom Controls")]
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.05f;

    [Header("Health")]
    public int maxHealth = 10;
    public float damageCooldown = 1.5f;

    [Header("Heart UI Parents")]
    public Transform heartsOnParent;
    public Transform heartsOffParent;

    [Header("Keys")]
    public int keysCollected = 0;
    public int maxKeys = 5;

    [Header("Key UI Parents")]
    public Transform keysOnParent;
    public Transform keysOffParent;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;
    public float hitAnimationTime = 1.5f;

    [Header("Game Over Overlay")]
    public float gameOverDelay = 1.5f;
    public string gameOverSceneName = "GameOver";

    [Header("Stats")]
    public int deathCount = 0;

    [Header("UI")]
    public Text deathCountText;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isDead;
    private bool canTakeDamage = true;
    private int currentHealth;

    private Vector3 startPosition;
    private RigidbodyType2D originalBodyType;
    private float defaultGravity;

    private SpriteRenderer[] srOn = new SpriteRenderer[0];
    private SpriteRenderer[] srOff = new SpriteRenderer[0];
    private SpriteRenderer[] keySrOn = new SpriteRenderer[0];
    private SpriteRenderer[] keySrOff = new SpriteRenderer[0];

    private void Awake()
    {
        AutoAssignUIParents();
        CacheUIRenderers();
        SyncStatLimitsToUI();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            originalBodyType = rb.bodyType;
            defaultGravity = rb.gravityScale;
        }

        startPosition = transform.position;

        LoadPersistentState();

        UpdateHealthBar();
        UpdateKeyBar();
        UpdateDeathUI();

        if (coll != null)
        {
            PhysicsMaterial2D zeroFriction = new PhysicsMaterial2D("ZeroFriction")
            {
                friction = 0f,
                bounciness = 0f
            };
            coll.sharedMaterial = zeroFriction;
        }
    }

    private void Update()
    {
        if (isDead || rb == null) return;

        HandleMovement();
        HandleJump();
    }

    private void LoadPersistentState()
    {
        if (GlobalPlayerState.Instance == null)
        {
            currentHealth = maxHealth;
            keysCollected = 0;
            deathCount = 0;
            return;
        }

        if (GlobalPlayerState.Instance.currentHealth < 0)
            GlobalPlayerState.Instance.currentHealth = maxHealth;

        currentHealth = Mathf.Clamp(GlobalPlayerState.Instance.currentHealth, 0, maxHealth);
        keysCollected = Mathf.Clamp(GlobalPlayerState.Instance.keysCollected, 0, maxKeys);
        deathCount = GlobalPlayerState.Instance.deathCount;
    }

    private void SavePersistentState()
    {
        if (GlobalPlayerState.Instance == null)
            return;

        GlobalPlayerState.Instance.currentHealth = currentHealth;
        GlobalPlayerState.Instance.keysCollected = keysCollected;
        GlobalPlayerState.Instance.deathCount = deathCount;
    }

    private void AutoAssignUIParents()
    {
        if (heartsOnParent == null)
            heartsOnParent = FindObjectByPossibleNames("Hearts On", "heart_0", "Heart On", "HeartsOn");

        if (heartsOffParent == null)
            heartsOffParent = FindObjectByPossibleNames("Hearts Off", "off", "Heart Off", "HeartsOff");

        if (keysOnParent == null)
            keysOnParent = FindObjectByPossibleNames("Keys On", "key_0", "Key On", "KeysOn");

        if (keysOffParent == null)
            keysOffParent = FindObjectByPossibleNames("Keys Off", "key_off", "Key Off", "KeysOff");
    }

    private Transform FindObjectByPossibleNames(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
                return found.transform;
        }
        return null;
    }

    private void CacheUIRenderers()
    {
        srOn = GetSortedChildRenderersRecursive(heartsOnParent);
        srOff = GetSortedChildRenderersRecursive(heartsOffParent);
        keySrOn = GetSortedChildRenderersRecursive(keysOnParent);
        keySrOff = GetSortedChildRenderersRecursive(keysOffParent);
    }

    private SpriteRenderer[] GetSortedChildRenderersRecursive(Transform parent)
    {
        if (parent == null) return new SpriteRenderer[0];

        List<SpriteRenderer> renderers = parent
            .GetComponentsInChildren<SpriteRenderer>(true)
            .Where(sr => sr.transform != parent) // exclude parent sprite if any
            .ToList();

        return renderers
            .OrderBy(sr => sr.transform.localPosition.x)
            .ThenByDescending(sr => sr.transform.localPosition.y)
            .ToArray();
    }

    private void SyncStatLimitsToUI()
    {
        int healthBarLength = Mathf.Max(srOn.Length, srOff.Length);
        if (healthBarLength > 0)
            maxHealth = healthBarLength;

        int keyBarLength = Mathf.Max(keySrOn.Length, keySrOff.Length);
        if (keyBarLength > 0)
            maxKeys = keyBarLength;
    }

    private void HandleMovement()
    {
        float xVelocity = 0f;

        if (Input.GetKey(moveLeftKey))
        {
            xVelocity = -moveSpeed;
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (Input.GetKey(moveRightKey))
        {
            xVelocity = moveSpeed;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);

        if (anim != null)
            anim.SetBool("Player_run", xVelocity != 0f && isGrounded);

        if (xVelocity != 0f && isGrounded && AudioManager.Instance != null && !AudioManager.Instance.sfxSource.isPlaying)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.walk);
    }

    private void HandleJump()
    {
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (anim != null)
        {
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Player_jump");
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.jump);
        }
    }

    public void AddKey()
    {
        if (keysCollected >= maxKeys) return;

        keysCollected++;
        UpdateKeyBar();
        SavePersistentState();
    }

    public void Die() => TakeDamage(1);

    public void TakeDamage(int amount)
    {
        if (isDead || !canTakeDamage) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        SavePersistentState();

        // Game over only at ZERO
        if (currentHealth == 0)
        {
            deathCount++;
            UpdateDeathUI();
            SavePersistentState();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRandomDeathSound();

            StartCoroutine(GameOverRoutine());
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRandomDeathSound();

            StartCoroutine(RespawnAfterHitRoutine());
        }
    }

    private void UpdateHealthBar()
    {
        UpdateSpriteBar(srOn, srOff, currentHealth);
    }

    private void UpdateKeyBar()
    {
        UpdateSpriteBar(keySrOn, keySrOff, keysCollected);
    }

    private void UpdateSpriteBar(SpriteRenderer[] onSlots, SpriteRenderer[] offSlots, int filledCount)
    {
        int total = Mathf.Max(onSlots.Length, offSlots.Length);

        for (int i = 0; i < total; i++)
        {
            bool filled = i < filledCount;

            if (i < onSlots.Length && onSlots[i] != null)
                SetSpriteAlpha(onSlots[i], filled ? 1f : 0f);

            if (i < offSlots.Length && offSlots[i] != null)
                SetSpriteAlpha(offSlots[i], filled ? 0f : 1f);
        }
    }

    private void SetSpriteAlpha(SpriteRenderer sr, float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    private void UpdateDeathUI()
    {
        if (deathCountText != null)
            deathCountText.text = "Deaths: " + deathCount;
    }

    private IEnumerator RespawnAfterHitRoutine()
    {
        canTakeDamage = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Player_jump");
            anim.SetBool("Player_run", false);
            anim.SetTrigger("Player_death");
        }

        yield return new WaitForSeconds(hitAnimationTime);

        ResetAllTraps();

        if (coll != null) coll.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = respawnPoint != null ? respawnPoint.position : startPosition;

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = defaultGravity;
        }

        if (coll != null) coll.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        ResetAnimationToIdle();

        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    private void ResetAnimationToIdle()
    {
        if (anim == null) return;

        anim.ResetTrigger("Player_death");
        anim.ResetTrigger("Player_jump");
        anim.Play("Player_Idle", 0, 0f);
        anim.SetBool("Player_run", false);
        anim.SetBool("isGrounded", true);
        anim.SetFloat("yVelocity", 0f);
    }

    private IEnumerator GameOverRoutine()
    {
        isDead = true;
        canTakeDamage = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (anim != null)
            anim.SetTrigger("Player_death");

        yield return new WaitForSeconds(gameOverDelay);

        UpdateHealthBar();

        Scene gameOverScene = SceneManager.GetSceneByName(gameOverSceneName);
        if (!gameOverScene.isLoaded)
            yield return SceneManager.LoadSceneAsync(gameOverSceneName, LoadSceneMode.Additive);

        yield return null; // wait 1 frame so objects in loaded scene exist

        GameObject camPoint = GameObject.Find("GameOverCameraPoint");
        Camera cam = Camera.main;

        if (cam != null && camPoint != null)
        {
            Vector3 pos = cam.transform.position;
            cam.transform.position = new Vector3(
                camPoint.transform.position.x,
                camPoint.transform.position.y,
                pos.z
            );
        }

        Time.timeScale = 0f;
    }

    private void ResetAllTraps()
    {
        foreach (var trap in FindObjectsOfType<TrapTrigger>()) trap.ResetTrap();
        foreach (var trap in FindObjectsOfType<dropPlatformer>()) trap.ResetTrap();
        foreach (var trap in FindObjectsOfType<SurpriseTrap>()) trap.ResetTrap();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
            TakeDamage(1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
            TakeDamage(1);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}