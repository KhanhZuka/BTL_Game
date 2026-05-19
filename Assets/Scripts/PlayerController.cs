using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Input
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction AttackAction;
    public InputAction DashAction;

    // Movement 
    public float speed = 6f;
    public float jumpForce = 8f;

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
    [Header("Health & Combat")]
    public int maxHealth = 10000;
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
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;
    public float attackDamage = 20f; // Sát thương mỗi đòn đánh

    bool isAttacking;

    // Dash
    [Header("Dash")]
    public float dashForce = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    bool isDashing;
    float dashTime;
    float dashCooldownTimer;

    // EVENTS
    void Start()
    {
        MoveAction.Enable();
        JumpAction.Enable();

        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
        respawnPoint = transform.position;
        
        // Lưu lại lực hút trái đất mặc định của Player trên Inspector
        defaultGravity = rigidbody2d.gravityScale; 
    }

    void Update()
    {
        if (isDead) return;

        moveX = MoveAction.ReadValue<Vector2>().x;

        CheckGround();
        Flip();

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
            rigidbody2d.linearVelocity = new Vector2(rigidbody2d.linearVelocity.x, jumpForce);

            // reset để tránh double jump ngoài ý muốn
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }

        // giảm cooldown dash
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing) return;

        // ĐÃ XÓA đoạn kiểm tra normalizedTime ở đây để tránh lỗi ngắt hoạt ảnh

        UpdateAnimator();
        HandleInvincible();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isDashing)
        {
            DashMovement();
            return; // chặn movement khác
        }

        rigidbody2d.linearVelocity = new Vector2(moveX * speed, rigidbody2d.linearVelocity.y);
    }

    void OnEnable()
    {
        JumpAction.performed += OnJump;
        AttackAction.Enable();
        AttackAction.performed += OnAttack;
        DashAction.Enable();
        DashAction.performed += OnDash;
    }

    void OnDisable()
    {
        JumpAction.performed -= OnJump;
        AttackAction.performed -= OnAttack;
        AttackAction.Disable();
        DashAction.performed -= OnDash;
        DashAction.Disable();
    }

    // METHODS
    void OnJump(InputAction.CallbackContext ctx)
    {
        // Ghi nhận ý định nhảy
        jumpBufferCounter = jumpBufferTime;
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
            if (isInvincible) return;
            isInvincible = true;
            damageCooldown = timeInvincible;
            animator.SetTrigger("Hit");
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log("HP: " + currentHealth + "/" + maxHealth);

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

        // Dừng physics
        rigidbody2d.linearVelocity = Vector2.zero;
        rigidbody2d.simulated = false;

        // Tắt input
        MoveAction.Disable();
        JumpAction.Disable();

        // Animation chết
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Respawn sau 1 khoảng thời gian
        Invoke(nameof(Respawn), 1.2f);
    }

    void Respawn()
    {
        // Đưa player về điểm respawn
        transform.position = respawnPoint;

        // Hồi lại máu (nếu game bạn reset máu khi hồi sinh)
        currentHealth = maxHealth;

        // Bật lại physics
        rigidbody2d.simulated = true;

        // Reset trạng thái
        isDead = false;
        isInvincible = false;
        isAttacking = false;

        // Reset velocity
        rigidbody2d.linearVelocity = Vector2.zero;

        // Bật lại input
        MoveAction.Enable();
        JumpAction.Enable();
    }

    public void SetCheckpoint(Vector2 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }

    // ATTACK 
    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isAttacking || isDead) return;

        Vector2 moveInput = MoveAction.ReadValue<Vector2>();

        // Đứng đất + nhấn W (Lên)
        if (isGrounded && moveInput.y > 0.5f)
        {
            AttackUp();
        }
        // Đang trên không + nhảy cao (xuống đập)
        else if (!isGrounded && jumpForce >= 20)
        {
            AttackDown();
        }
        // Attack thường
        else if (isGrounded)
        {
            NormalAttack();
        }
    }

    void NormalAttack()
    {
        if (isAttacking) return;
        isAttacking = true;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
    }

    void AttackUp()
    {
        if (isAttacking) return;
        isAttacking = true;

        animator.ResetTrigger("AttackUp");
        animator.SetTrigger("AttackUp");
    }

    void AttackDown()
    {
        if (isAttacking) return;
        isAttacking = true;

        animator.ResetTrigger("AttackDown");
        animator.SetTrigger("AttackDown");

        rigidbody2d.linearVelocity = new Vector2(rigidbody2d.linearVelocity.x, -15f);
    }

    public void EndAttack()
    {
        Debug.Log("EndAttack called via Animation Event");
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
                enemy.Die();
            }
        }
    }

    // DASH
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