using UnityEngine;

public class RangedEnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public Transform player;

    [Header("--- Enemy Stats ---")]
    public float hp = 80f; 

    [Header("--- Movement Speeds ---")]
    public float walkSpeed = 1.5f; 
    public float chaseSpeed = 2.5f; 

    [Header("--- AI Ranges & Territory ---")]
    // Đã chuyển toàn bộ thành khoảng cách ngang (Trục X)
    public float detectionRange = 12f; 
    public float attackRange = 7f;     
    
    public float verticalDetectionLimit = 1.5f; // Vẫn giữ giới hạn chiều cao để không bắn bậy
    public float territoryRadius = 10f;       
    private Vector2 startPos; 

    [Header("--- Combat ---")]
    public float attackCooldown = 2.5f; 
    private float attackTimer = 0f;

    private bool isDead = false;
    private bool isPatrolling = false;
    private float patrolTimer = 0f;
    private float patrolDirection = 1f; 

    [Header("--- Ranged Setup ---")]
    public Transform firePoint; 
    public GameObject projectilePrefab; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        attackTimer = attackCooldown; 
        startPos = transform.position; 
    }

    void Update()
    {
        if (isDead) return;

        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttacking = stateInfo.IsName("Attack");

        if (isAttacking || stateInfo.IsName("Hit"))
        {
            StopMoving();
            return;
        }

        if (attackTimer < 0.1f) return;

        // KIỂM TRA MỚI: Chỉ tính khoảng cách ngang (X) giữa Quái và Player
        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);
        float directionX = player.position.x - transform.position.x;
        
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
        float playerDistanceFromHomeX = Mathf.Abs(player.position.x - startPos.x);

        bool isPlayerInTerritory = playerDistanceFromHomeX <= territoryRadius;
        bool isPlayerAtSameLevel = verticalDistance <= verticalDetectionLimit;

        // QUÁI ĐÁNH XA (THEO HÀNG NGANG)
        
        if (isPlayerAtSameLevel && distanceToPlayerX <= attackRange)
        {
            // 1. TRONG TẦM BẮN NGANG -> ĐỨNG LẠI BẮN
            StopMoving();
            FlipTowards(directionX);

            if (attackTimer >= attackCooldown)
            {
                animator.SetTrigger("Attack");
                animator.SetBool("isWalk", false);
                attackTimer = 0f; 
            }
        }
        else if (isPlayerAtSameLevel && distanceToPlayerX <= detectionRange && isPlayerInTerritory)
        {
            // 2. TRONG TẦM NHÌN NGANG NHƯNG CHƯA TỚI TẦM BẮN -> ĐI LÙA
            animator.SetBool("isWalk", true);

            float dirNormal = Mathf.Sign(directionX);
            rb.linearVelocity = new Vector2(dirNormal * chaseSpeed, rb.linearVelocity.y);
            FlipTowards(directionX);
        }
        else 
        {
            // 3. MẤT DẤU -> QUAY VỀ TUẦN TRA
            animator.SetBool("isWalk", false);
            PatrolLogic(); 
        }
    }

    private void PatrolLogic()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            patrolTimer = Random.Range(3f, 5f);
            int action = Random.Range(0, 10); 

            if (action < 7) 
            {
                isPatrolling = true;
                animator.SetBool("isWalk", true);
                
                float myDistanceFromHomeX = Mathf.Abs(transform.position.x - startPos.x);
                
                if (myDistanceFromHomeX > territoryRadius)
                {
                    float dirToHome = startPos.x - transform.position.x;
                    patrolDirection = Mathf.Sign(dirToHome);
                }
                else
                {
                    patrolDirection *= -1f; 
                }
                
                Vector3 currentScale = transform.localScale;
                currentScale.x = patrolDirection == 1f ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);
                transform.localScale = currentScale;
            }
            else 
            {
                StopMoving();
                animator.SetBool("isWalk", false);
                animator.Play("RangedEnemy"); 
            }
        }

        if (isPatrolling && animator.GetBool("isWalk"))
        {
            rb.linearVelocity = new Vector2(patrolDirection * walkSpeed, rb.linearVelocity.y);
        }
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isPatrolling = false;
    }

    private void FlipTowards(float directionX)
    {
        Vector3 currentScale = transform.localScale;

        if (directionX > 0.1f) 
        {
            currentScale.x = -Mathf.Abs(currentScale.x); 
        }
        else if (directionX < -0.1f) 
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }

        transform.localScale = currentScale;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        hp -= damageAmount;

        if (hp <= 0)
        {
            Die();
            return;
        }

        animator.Play("Hit"); 
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.Play("Dead"); 
        StopMoving();
        rb.gravityScale = 0;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 drawPos = Application.isPlaying ? startPos : (Vector2)transform.position;
        
        // 1. Vẽ lãnh thổ (Màu vàng)
        Gizmos.color = Color.yellow;
        Vector3 leftBound = new Vector3(drawPos.x - territoryRadius, drawPos.y, 0);
        Vector3 rightBound = new Vector3(drawPos.x + territoryRadius, drawPos.y, 0);
        Gizmos.DrawLine(leftBound, rightBound);
        Gizmos.DrawLine(leftBound + Vector3.up * 0.5f, leftBound - Vector3.up * 0.5f);
        Gizmos.DrawLine(rightBound + Vector3.up * 0.5f, rightBound - Vector3.up * 0.5f);
        
        // 2. Vẽ giới hạn chiều cao (Màu xanh)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * verticalDetectionLimit);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.up * verticalDetectionLimit);
        
        // 3. VẼ TẦM BẮN MỚI THEO HÀNG NGANG (Màu đỏ)
        Gizmos.color = Color.red;
        Vector3 attackLeft = new Vector3(transform.position.x - attackRange, transform.position.y, 0);
        Vector3 attackRight = new Vector3(transform.position.x + attackRange, transform.position.y, 0);
        Gizmos.DrawLine(attackLeft, attackRight);
        Gizmos.DrawLine(attackLeft + Vector3.up * 0.3f, attackLeft - Vector3.up * 0.3f);
        Gizmos.DrawLine(attackRight + Vector3.up * 0.3f, attackRight - Vector3.up * 0.3f);
    }
}