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

    private int health = 20;
    public static BatEnemy instance;
    public float attackDistance = 1.2f;

    Animator animator;

    bool isAttacking = false;
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

    public void ChangeHealth(int amout)
    {
        health -= amout;
        if(health <= 0) Destroy(gameObject);
    }

    void AttackPlayer()
    {
        rigidbody2D.linearVelocity = Vector2.zero;

        if (!isAttacking)
        {
            isAttacking = true;

            animator.SetTrigger("Attack");

            Invoke(nameof(ResetAttack), 0.5f);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}