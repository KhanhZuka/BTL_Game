using UnityEngine;
using UnityEngine.UI;

public class BerserkEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointC;
    public Transform pointD;
    public float speed = 2f;

    [Header("Player")]
    public Transform player;
    public float chaseDistance = 4f;
    private bool isAttacking = false;
    public float attackCooldown = 1.2f;
    private float nextAttackTime = 0f;
    public int damage = 15;

    [Header("Jump Wall")]
    public float jumpForce = 10f;
    public float jumpForwardForce = 7f;
    public Transform wallCheck;
    public Transform groundCheck;
    public float checkRadius = 0.35f;
    public LayerMask groundLayer;

    [Header("Health")]
    public Image fillHealth;
    public Image frameHealth;
    private int maxHealthBerserk = 30;
    private int healthBerserk = 30;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private Transform target;

    private bool isChasing;
    private bool isGrounded;
    private bool isJumpingWall;

    public Transform attackPoint;
    public float attackRadius = 0.2f;
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip berserkSound;

    private float nextSoundTime;
    public float SoundInterval = 3f;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        target = pointD;
        healthBerserk = maxHealthBerserk;
        UpdateHealthBar();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        CheckGround();

        float distanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);

        isChasing = distanceToPlayer < chaseDistance && PlayerInPatrolArea();

        bool playerInAttackRange = Physics2D.OverlapCircle(
           attackPoint.position,
           attackRadius,
           playerLayer
        );

        if (playerInAttackRange)
        {
            AttackPlayer();
            return;
        }

        if (isAttacking)
        {
            rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);
            return;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        CheckWallAndJump();
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );
    }

    void PlayBerserkSoundWhenNear()
    {
        if (Time.time >= nextSoundTime)
        {
            if (audioSource != null && berserkSound != null)
            {
                audioSource.PlayOneShot(berserkSound);
            }

            nextSoundTime = Time.time + SoundInterval;
        }
    }

    void Patrol()
    {
        float directionX = target.position.x - transform.position.x;

        if (directionX > 0)
        {
            Move(1);
        }
        else if (directionX < 0)
        {
            Move(-1);
        }

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
        {
            if (target == pointC)
            {
                target = pointD;
            }
            else
            {
                target = pointC;
            }
        }
    }

    void ChasePlayer()
    {
        float directionX = player.position.x - transform.position.x;

        if (directionX > 0)
        {
            Move(1);
        }
        else if (directionX < 0)
        {
            Move(-1);
        }
    }

    void Move(float direction)
    {
        rigidbody2D.linearVelocity = new Vector2(
            direction * speed,
            rigidbody2D.linearVelocity.y
        );

        Flip(direction);
    }

    void Flip(float direction)
    {
        if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void CheckWallAndJump()
    {
        float direction;

        if (isChasing)
        {
            direction = player.position.x > transform.position.x ? 1f : -1f;
        }
        else
        {
            direction = target.position.x > transform.position.x ? 1f : -1f;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            1f,
            groundLayer
        );

        if (hit.collider != null && isGrounded && !isJumpingWall)
        {
            isJumpingWall = true;

            rigidbody2D.linearVelocity = new Vector2(
                direction * jumpForwardForce,
                jumpForce
            );
        }

        if (isGrounded && hit.collider == null)
        {
            isJumpingWall = false;
        }
    }

    bool PlayerInPatrolArea()
    {
        float minX = Mathf.Min(pointC.position.x, pointD.position.x);
        float maxX = Mathf.Max(pointC.position.x, pointD.position.x);

        return player.position.x >= minX && player.position.x <= maxX;
    }

    void AttackPlayer()
    {
        rigidbody2D.linearVelocity = new Vector2(0, rigidbody2D.linearVelocity.y);

        float directionX = player.position.x - transform.position.x;

        if (directionX > 0)
        {
            Flip(1);
        }
        else if (directionX < 0)
        {
            Flip(-1);
        }
        PlayBerserkSoundWhenNear();
        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        Invoke(nameof(DamagePlayer), 0.4f);
        Invoke(nameof(ResetAttack), 1f);
    }

    void DamagePlayer()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        if (hitPlayer != null)
        {
            PlayerOne playerOne = hitPlayer.GetComponent<PlayerOne>();

            if (playerOne != null && playerOne.health > 0)
            {
                playerOne.ChangeHealth(-damage);
            }
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    public void ChangeHealthBerserk(int amount)
    {

        healthBerserk -= amount;
        healthBerserk = Mathf.Clamp(healthBerserk, 0, maxHealthBerserk);
        UpdateHealthBar();

        if (healthBerserk <= 0)
        {
            animator.SetTrigger("Dead");
            PlayerOne.instance.soQuaiDead++;
        }
    }

    void UpdateHealthBar()
    {
        if (fillHealth != null)
        {
            fillHealth.fillAmount = (float)healthBerserk / maxHealthBerserk;
        }
    }

    void DestroyEnemyBerserk()
    {
        Destroy(gameObject);
    }
}

