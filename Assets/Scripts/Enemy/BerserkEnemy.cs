using UnityEngine;

public class BerserkEnemy : MonoBehaviour
{
    public Transform pointC;
    public Transform pointD;

    public Transform player;

    public float speed = 2.0f;

    public float chaseDistance = 5.0f;

    Rigidbody2D rigidbody2D;

    Transform target;

    bool isChasing = false;

    public float jumpForce = 10f;
    public float jumpForwardForce = 7f;
    bool isJumpingWall = false;
    public Transform wallCheck;
    public Transform groundCheck;
    public float checkRadius = 0.35f;
    public LayerMask groundLayer;

    bool isGrounded;

    private int health = 20;
    public static BerserkEnemy instance;
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
        target = pointD;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        checkRadius,
        groundLayer
        );

        Patrol();
        CheckWallAndJump();

        //// Nếu player vào vùng phát hiện
        //if (distanceToPlayer < chaseDistance)
        //{
        //    isChasing = true;
        //}
        //else
        //{
        //    isChasing = false;
        //}

        // Nếu đủ gần thì attack
        //if (distanceToPlayer < attackDistance)
        //{
        //   // AttackPlayer();
        //}
        //else if (isChasing)
        //{
        //    //ChasePlayer();
        //}
        //else
        //{
        //    Patrol();
        //}
    }

    void Patrol()
    {
        float directionX = target.position.x - transform.position.x;

        if (directionX > 0)
        {
            rigidbody2D.linearVelocity =
                new Vector2(speed, rigidbody2D.linearVelocity.y);

            Flip(1);
        }
        else if (directionX < 0)
        {
            rigidbody2D.linearVelocity =
                new Vector2(-speed, rigidbody2D.linearVelocity.y);

            Flip(-1);
        }

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
        {
            if (target == pointC)
                target = pointD;
            else
                target = pointC;
        }
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

    void CheckWallAndJump()
    {
        float direction = target.position.x > transform.position.x ? 1f : -1f;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            1.0f,
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

    //void ChasePlayer()
    //{
    //    Vector2 direction =
    //        (player.position - transform.position).normalized;

    //    rigidbody2D.linearVelocity =
    //        direction * speed;

    //    Flip(direction.x);
    //}



    //public void ChangeHealth(int amout)
    //{
    //    health -= amout;
    //    if (health <= 0) Destroy(gameObject);
    //}

    //void AttackPlayer()
    //{
    //    rigidbody2D.linearVelocity = Vector2.zero;

    //    if (!isAttacking)
    //    {
    //        isAttacking = true;
    //        canDamage = true;

    //        animator.SetTrigger("Attack");

    //        // Đợi animation đánh chạy một đoạn rồi mới trừ máu
    //        Invoke(nameof(DamagePlayer), 1f);

    //        Invoke(nameof(ResetAttack), 1f);
    //    }
    //}

    //void ResetAttack()
    //{
    //    isAttacking = false;
    //    canDamage = true;
    //}

    //void DamagePlayer()
    //{
    //    if (playerTarget != null && playerTarget.health > 0 && playerInAttackRange && canDamage)
    //    {
    //        playerTarget.ChangeHealth(-5);
    //        canDamage = false;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    PlayerOne player = collision.GetComponent<PlayerOne>();

    //    if (player != null)
    //    {
    //        playerInAttackRange = false;
    //        playerTarget = null;
    //    }
    //}

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    PlayerOne player = collision.GetComponent<PlayerOne>();

    //    if (player != null)
    //    {
    //        playerTarget = player;
    //        playerInAttackRange = true;
    //    }
    //}
}
