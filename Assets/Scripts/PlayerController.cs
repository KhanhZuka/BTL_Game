using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    // Input
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction AttackAction;
    public InputAction DashAction;

    // Movement 
    public float speed = 6f;
    public float jumpForce = 8f;
    float facingDirection = 1f; // 1 = phải, -1 = trái
    Vector2 attackDirection = Vector2.right;

    Rigidbody2D rigidbody2d;
    float moveX;
    //float moveInputX;

    // Jump feel 
    [Header("Jump Feel")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    float coyoteTimeCounter;
    float jumpBufferCounter;

    // Trọng lực gốc (Dùng để trả lại trạng thái sau khi Dash)
    float defaultGravity;

    // Ground check 
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    bool isGrounded;

    // Health 
    [Header("Health")]
    public int maxHealth = 100;
    int currentHealth;
    public int health => currentHealth;

    // Invincible 
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float damageCooldown;

    // Animation 
    Animator animator;
    SpriteRenderer spriteRenderer;

    // Death and Respawn 
    bool isDead;
    Vector2 respawnPoint;

    // Attack 
    [Header("Attack")]
    public int baseDamage = 1;
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;
    bool isAttacking;

    // Dash
    [Header("Dash")]
    public float dashForce = 13f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    bool isDashing;
    float dashTime;
    float dashCooldownTimer;

    //Item 
    float damageMultiplier = 1f;
    float originalSpeed;
    bool isShieldActive = false;
    float originalJumpForce;

    // Fireball
    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireCooldown = 5f;
    float fireTimer;
    public SkillCooldownUI skillUI;

    [Header("Charge Attack")]
    public float maxChargeTime = 2f;

    [Header("Key")]
    public bool hasKey = false;

    [Header("Moving Platform")]
    public Vector2 platformVelocity;

    public int soXu = 0;
    public int soQuaiDead = 0;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip appearSound;

    // Sound_Attack
    public AudioClip[] attackSounds;
    public AudioClip attackUpSound;
    public AudioClip attackDownSound;

    // Sound_Movement
    public AudioClip[] footstepSounds;

    // Sound_Jump
    public AudioClip jumpSound;
    public AudioClip[] landingSounds;

    // Sound_Others
    public AudioClip dashSound;
    public AudioClip hitSound;
    public AudioClip fireSound;
    public AudioClip deadSound;

    // footstep timer
    float stepTimer;
    public float stepInterval = 0.4f;

    // landing check
    bool wasGrounded;

    private void Awake()
    {
        instance = this;
    }
    // EVENTS
    void Start()
    {
        audioSource.PlayOneShot(appearSound);
        MoveAction.Enable();
        JumpAction.Enable();

        rigidbody2d = GetComponent<Rigidbody2D>();
        defaultGravity = rigidbody2d.gravityScale; // Lưu trọng lực gốc

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
        respawnPoint = transform.position;

        InventoryManager.Instance.items.Clear();
    }
    void Update()
    {
        if (isDead) return;

        moveX = MoveAction.ReadValue<Vector2>().x;

        CheckGround();
        Flip();

        //LANDING 
        if (!wasGrounded && isGrounded)
        {
            audioSource.PlayOneShot(GetRandomClip(landingSounds));
        }
        wasGrounded = isGrounded;


        // Cho phép nhảy trong một khoảng thời gian ngắn sau khi rời đất (Coyote Time)
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer (ý định nhảy)
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            audioSource.PlayOneShot(jumpSound);
            rigidbody2d.linearVelocity = new Vector2(rigidbody2d.linearVelocity.x, jumpForce);
            // reset để tránh double jump ngoài ý muốn
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }

        // giảm cooldown dash
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing) return;
        if (isAttacking)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f)
            {
                isAttacking = false;
            }
        }

        // nhấn E để tấn công
        fireTimer -= Time.deltaTime;
        if (Keyboard.current.eKey.wasPressedThisFrame && fireTimer <= 0 && !isAttacking)
        {
            FireAttack();
            fireTimer = fireCooldown;
        }

        // flip FirePoint theo hướng player
        Vector3 fpPos = firePoint.localPosition;

        fpPos.x = Mathf.Abs(fpPos.x) * facingDirection;
        firePoint.localPosition = fpPos;

        // cập nhật hướng nhìn
        if (moveX > 0)
        {
            facingDirection = 1f;
        }
        else if (moveX < 0)
        {
            facingDirection = -1f;
        }

        // FOOTSTEP 
        if (isGrounded && Mathf.Abs(moveX) > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                audioSource.PlayOneShot(GetRandomClip(footstepSounds));
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }

        UpdateAnimator();
        HandleInvincible();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            DashMovement();
            return; // chặn movement khác
        }

       rigidbody2d.linearVelocity = new Vector2((moveX * speed) + platformVelocity.x, rigidbody2d.linearVelocity.y);
    }
   void OnEnable()
{
    MoveAction.Enable(); 
    
    JumpAction.Enable();
    JumpAction.performed += OnJump;
    
    AttackAction.Enable();
    AttackAction.performed += OnAttack;
    
    DashAction.Enable();
    DashAction.performed += OnDash;
}

void OnDisable()
{
    MoveAction.Disable(); 
    
    JumpAction.performed -= OnJump;
    JumpAction.Disable(); 
    
    AttackAction.performed -= OnAttack;
    AttackAction.Disable();
    
    DashAction.performed -= OnDash;
    DashAction.Disable();
}
    // METHODS
    void OnJump(InputAction.CallbackContext ctx)
    {
        jumpBufferCounter = jumpBufferTime; // Ghi nhận ý định nhảy
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Flip()
    {
        if (moveX > 0)
            spriteRenderer.flipX = false;
        else if (moveX < 0)
            spriteRenderer.flipX = true;
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveX));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rigidbody2d.linearVelocity.y);
    }

    // Invincible
    void HandleInvincible()
    {
        if (!isInvincible) return;
        damageCooldown -= Time.deltaTime;
        if (damageCooldown < 0)
        {
            isInvincible = false;
        }
    }

    // Health
    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        if (amount < 0)
        {
            if (isShieldActive)
            {
                Debug.Log("Shield block");
                return;
            }

            if (isInvincible) return;
            isInvincible = true;
            damageCooldown = timeInvincible;
            audioSource.PlayOneShot(hitSound);
            animator.SetTrigger("Hit");
        }
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        Debug.Log("Health: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Death and Respawn
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // mất 1 mạng
        HealthUIManager.Instance.LoseLife();

        if (HealthUIManager.Instance.IsGameOver())
            {
                GameOver();
                return;
            }

        // dừng physics
        rigidbody2d.linearVelocity = Vector2.zero;
        rigidbody2d.simulated = false;

        audioSource.PlayOneShot(deadSound);
        animator.SetTrigger("Die");

        Invoke(nameof(Respawn), 1.2f);
    }

    void Respawn()
    { 
        transform.position = respawnPoint; // Đưa player về điểm respawn
        audioSource.PlayOneShot(appearSound);
        rigidbody2d.simulated = true; // Bật lại physics        
        isDead = false; // Reset trạng thái
        rigidbody2d.linearVelocity = Vector2.zero; // Reset velocity
        MoveAction.Enable(); // Bật lại input
        JumpAction.Enable();
        
        // đầy máu khi sống lại
        currentHealth = maxHealth;
        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public void SetCheckpoint(Vector2 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }
    void GameOver()
    {
        Debug.Log("GAME OVER");

        // tắt player
        rigidbody2d.simulated = false;
        Debug.Log("Game Over");
        Time.timeScale = 1f;
        GameData.lastMap = SceneManager.GetActiveScene().name;
        GameData.backToLosePanel = true;
        SceneManager.LoadScene("UIScene");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // load scene lại
    }

    // ATTACK 
    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;
        Vector2 moveInput = MoveAction.ReadValue<Vector2>();

        // xác định hướng attack
        if (moveInput.y > 0.5f)
        {
            attackDirection = Vector2.up;
        }
        else if (moveInput.y < -0.5f)
        {
            attackDirection = Vector2.down;
        }
        else
        {
            attackDirection = new Vector2(facingDirection, 0);
        }

        // cập nhật attackPoint 
        attackPoint.localPosition = attackDirection * 0.5f;

        if (moveInput.y > 0.5f)
        {
            AttackUp(); 
        }
        else if (moveInput.y < -0.5f && !isGrounded)
        {
            AttackDown(); 
        }
        else
        {
            NormalAttack(); 
        }
    }

    void NormalAttack()
    {
        if (isAttacking) return;
        isAttacking = true;
        // audioSource.PlayOneShot(GetRandomClip(attackSounds));

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
    }

    void AttackUp()
    {
        if (isAttacking) return;
        isAttacking = true;
        audioSource.PlayOneShot(attackUpSound);

        animator.ResetTrigger("AttackUp");
        animator.SetTrigger("AttackUp");
    }

    void AttackDown()
    {
        if (isAttacking) return;
        isAttacking = true;
        audioSource.PlayOneShot(attackDownSound);

        animator.ResetTrigger("AttackDown");
        animator.SetTrigger("AttackDown");

        rigidbody2d.linearVelocity = new Vector2(rigidbody2d.linearVelocity.x, -15f);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    // Cho phép tấn công lại sau khi animation kết thúc
    public void EnableAttack()
    {
        isAttacking = false;
    }

    // DealDamage (Được gọi từ Animation Event)
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        

        foreach (Collider2D hit in hits)
        {
            EnemySystemController enemy = hit.GetComponent<EnemySystemController>();
            if (enemy != null)
            {
                int finalDamage = (int)(baseDamage * damageMultiplier);
                enemy.TakeDamage(finalDamage);
            }
        }
    }
    // Dash
    void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || isDead) return;

        if (isDashing) return;
        if (dashCooldownTimer > 0) return;

        StartDash();
    }
    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;

        // tắt gravity khi dash
        rigidbody2d.gravityScale = 0;

        isInvincible = true;
        audioSource.PlayOneShot(dashSound);

        animator.SetTrigger("Dash");
    }

    void DashMovement()
    {
        // Dùng fixedDeltaTime vì đang ở trong FixedUpdate
        dashTime -= Time.fixedDeltaTime; 

        float direction;
        if (Mathf.Abs(rigidbody2d.linearVelocity.x) > 0.1f)
        {
            // đang di chuyển dùng velocity
            direction = Mathf.Sign(rigidbody2d.linearVelocity.x);
        }
        else
        {
            // đứng yên dùng hướng nhìn
            direction = spriteRenderer.flipX ? -1 : 1;
        }

        rigidbody2d.linearVelocity = new Vector2(direction * dashForce, 0);

        if (dashTime <= 0)
        {
            EndDash();
        }
    }

    void EndDash()
    {
        isDashing = false;

        // Bật lại gravity như ban đầu
        rigidbody2d.gravityScale = defaultGravity; 
        
        isInvincible = false;
    }

    // Item Effects
    public IEnumerator DamageBuff(float multiplier, float duration)
    {
        damageMultiplier = multiplier;

        yield return new WaitForSeconds(duration);

        damageMultiplier = 1f;
    }
    public IEnumerator SpeedBuff(float newSpeed, float duration)
    {
        originalSpeed = speed;
        speed = newSpeed;

        yield return new WaitForSeconds(duration);

        speed = originalSpeed;
    }
    public IEnumerator ShieldBuff(float duration)
    {
        isShieldActive = true;

        yield return new WaitForSeconds(duration);

        isShieldActive = false;
    }

    public IEnumerator HighJumpBuff(float newJump, float duration)
    {
        originalJumpForce = jumpForce;
        jumpForce = newJump;

        yield return new WaitForSeconds(duration);

        jumpForce = originalJumpForce;
    }
    public IEnumerator FreezeEnemies(float duration)
    {
        EnemySystemController[] enemies = Object.FindObjectsByType<EnemySystemController>(FindObjectsSortMode.None);

        foreach (var e in enemies){}
            //e.Freeze(true);

        yield return new WaitForSeconds(duration);

        foreach (var e in enemies){}
            //e.Freeze(false);
    }

    // Fireball Attack
    void FireAttack()
    {
        audioSource.PlayOneShot(fireSound);
        animator.SetTrigger("FireAttack");
    }
    public void SpawnFireball()
    {
        GameObject fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        Fireball fireball = fb.GetComponent<Fireball>();
        if (fireball == null) return;

        fireball.SetDirection(new Vector2(facingDirection, 0));
        fireball.SetDamageMultiplier(damageMultiplier); // truyền multiplier sang fireball
        SpriteRenderer sr = fb.GetComponent<SpriteRenderer>(); // chỉnh sprite đúng hướng
        sr.flipX = facingDirection > 0;

        skillUI.StartCooldown(fireCooldown);
    }

    AudioClip GetRandomClip(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }
    public void PlayAttackSound()
    {
        if (audioSource != null && attackSounds.Length > 0)
        {
            audioSource.pitch = Random.Range(0.95f, 1.1f);
            audioSource.PlayOneShot(GetRandomClip(attackSounds));
        }
    }

    // DEBUG
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}