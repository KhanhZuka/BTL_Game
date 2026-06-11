using UnityEngine;

public class WarriorEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointI;
    public Transform pointK;
    public float speed = 2f;

    [Header("Player")]
    public Transform player;
    public float chaseDistance = 4f;
    private bool isAttacking = false;
    public float attackCooldown = 1.2f;
    private float nextAttackTime = 0f;
    public int damage = 25;

    [Header("Health")]
    public int healthWarrior = 20;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private Transform target;

    private bool isChasing;
    private bool isGrounded;
    private bool isJumpingWall;

    public Transform attackPoint;
    public float attackRadius = 0.2f;
    public LayerMask playerLayer;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        target = pointK;
    }

    void FixedUpdate()
    {
        if (player == null) return;

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
            if (target == pointI)
            {
                target = pointK;
            }
            else
            {
                target = pointI;
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


    bool PlayerInPatrolArea()
    {
        float minX = Mathf.Min(pointI.position.x, pointK.position.x);
        float maxX = Mathf.Max(pointI.position.x, pointK.position.x);

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

        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        animator.ResetTrigger("AttackWarrior");
        animator.SetTrigger("AttackWarrior");

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

    public void ChangeHealthWarrior(int amount)
    {
        healthWarrior -= amount;
        if (healthWarrior <= 0)
        {
            animator.SetTrigger("DeadWarrior");
            PlayerOne.instance.soQuaiDead++;
        }
    }

    void DestroyEnemyWarrior()
    {
        Destroy(gameObject);
    }
}
