using Unity.VisualScripting;
using UnityEngine;

public class BatEnemy : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public Transform player;

    public float speed = 2.0f;

    public float chaseDistance = 5.0f;

    Rigidbody2D rigidbody2D;

    Transform target;

    bool isChasing = false;

    private int healthBat = 20;
    public static BatEnemy instance;
    public float attackDistance = 1.2f;

    Animator animator;

    bool isAttacking = false;
    bool canDamage = true;
    bool playerInAttackRange = false;
    PlayerOne playerTarget;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        target = pointB;
    }

    void FixedUpdate()
    {
        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        // Nếu player vào vùng phát hiện
        if (distanceToPlayer < chaseDistance)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        // Nếu đủ gần thì attack
        if (distanceToPlayer < attackDistance)
        {
            AttackPlayer();
        }
        else if (isChasing)
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
        Vector2 direction =
            (target.position - transform.position).normalized;

        rigidbody2D.linearVelocity =
            direction * speed;

        Flip(direction.x);

        // Đổi điểm tuần tra
        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            if (target == pointA)
            {
                target = pointB;
            }
            else
            {
                target = pointA;
            }
        }
    }

    void ChasePlayer()
    {
        Vector2 direction =
            (player.position - transform.position).normalized;

        rigidbody2D.linearVelocity =
            direction * speed;

        Flip(direction.x);
    }

    void Flip(float directionX)
    {
        if (directionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (directionX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void ChangeHealthBat(int amount)
    {
        animator.SetTrigger("Hurt");
        healthBat -= amount;

        if (healthBat <= 0)
        {
            animator.SetTrigger("Dead");
        }
    }

    void DestroyEnemyBat()
    {
        Destroy(gameObject);
    }

    void AttackPlayer()
    {
        rigidbody2D.linearVelocity = Vector2.zero;

        if (!isAttacking)
        {
            isAttacking = true;
            canDamage = true;

            animator.SetTrigger("Attack");

            // Đợi animation đánh chạy một đoạn rồi mới trừ máu
            Invoke(nameof(DamagePlayer), 1f);

            Invoke(nameof(ResetAttack), 1f);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
        canDamage = true;
    }

    void DamagePlayer()
    {
        if (playerTarget != null && playerTarget.health > 0 && playerInAttackRange && canDamage)
        {
            playerTarget.ChangeHealth(-5);
            canDamage = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerOne player = collision.GetComponent<PlayerOne>();

        if (player != null)
        {
            playerInAttackRange = false;
            playerTarget = null;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerOne player = collision.GetComponent<PlayerOne>();

        if (player != null)
        {
            playerTarget = player;
            playerInAttackRange = true;
        }
    }
}
    
