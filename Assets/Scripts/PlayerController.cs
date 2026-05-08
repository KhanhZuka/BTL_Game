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
    float moveInputX;

    // Jump feel 
    [Header("Jump Feel")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    float coyoteTimeCounter;
    float jumpBufferCounter;

    // Ground check 
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    bool isGrounded;

    // Health 
    public int maxHealth = 10;
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
    }

    void Update()
    {
        moveX = MoveAction.ReadValue<Vector2>().x;
        //moveInputX = MoveAction.ReadValue<Vector2>().x;

        CheckGround();
        Flip();

        // Cho phép nhảy trong một khoảng thời gian ngắn sau khi rời đất
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

        if (isAttacking)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.normalizedTime >= 1f)
            {
                isAttacking = false;
            }
        }

        UpdateAnimator();
        HandleInvincible();
    }
    void FixedUpdate()
    {
        rigidbody2d.linearVelocity = new Vector2(moveX * speed, rigidbody2d.linearVelocity.y);

        if (isDashing)
        {
            DashMovement();
            return; // chặn movement khác
        }
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
        //animator.SetBool("IsMovingInput", Mathf.Abs(moveInputX) > 0.1f);
        //if (Mathf.Abs(moveInputX) > 0.1f)
        //{
        //    animator.SetInteger("Facing", (int)Mathf.Sign(moveInputX));
        //}
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
        if (amount < 0)
        {
            if (isInvincible) return;
            isInvincible = true;
            damageCooldown = timeInvincible;
            animator.SetTrigger("Hit");
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
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

        // Bật lại physics
        rigidbody2d.simulated = true;

        // Reset trạng thái
        isDead = false;

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
        if (isAttacking) return;

        Vector2 moveInput = MoveAction.ReadValue<Vector2>();

        // đứng đất + nhấn W
        if (isGrounded && moveInput.y > 0.5f)
        {
            AttackUp();
        }


        // đang trên không (có buff nhảy cao) + nhấn attack
        if (!isGrounded && jumpForce >= 20)
        {
            AttackDown();
        }

        // Attack thường
        if (isGrounded)
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
        Debug.Log("EndAttack called");
        isAttacking = false;
    }

    // Cho phép tấn công lại sau khi animation kết thúc
    public void EnableAttack()
    {
        isAttacking = false;
    }

    // DealDamage
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll( attackPoint.position, attackRadius, enemyLayer );

        foreach (Collider2D hit in hits)
        {
            EnemyPatrol enemy = hit.GetComponent<EnemyPatrol>();
            if (enemy != null)
            {
                enemy.Dead();
            }
        }
    }

    //
    void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

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
        GetComponent<Rigidbody2D>().gravityScale = 0;

        isInvincible = true;

        animator.SetTrigger("Dash");
    }
    void DashMovement()
    {
        dashTime -= Time.deltaTime;

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

        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * dashForce, 0);

        if (dashTime <= 0)
        {
            EndDash();
        }
    }
    void EndDash()
    {
        isDashing = false;

        // bật lại gravity
        GetComponent<Rigidbody2D>().gravityScale = 3;
        isInvincible = false;
    }

    // DEBUG
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}