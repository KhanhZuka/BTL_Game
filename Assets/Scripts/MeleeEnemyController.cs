using UnityEngine;

public class MeleeEnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public Transform player;

    [Header("--- Enemy Stats ---")]
    public float hp = 100f;

    [Header("--- Movement Speeds ---")]
    public float walkSpeed = 1.5f; 
    public float runSpeed = 4.5f;  

    [Header("--- AI Ranges & Territory ---")]
    public float detectionRange = 10f; 
    public float attackRange = 1.5f;   
    public float loseSightRange = 10f; 
    
    [Header("--- Vertical Limits ---")]
    // BIẾN : Player cao hơn quái bao nhiêu thì quái sẽ ngừng đuổi (ví dụ: 3 mét)
    public float verticalDetectionLimit = 2f; 

    [Header("--- Territory ---")]
    public float territoryRadius = 10f; 
    private Vector2 startPos; 

    [Header("--- Combat ---")]
    public float attackCooldown = 2f; 
    private float attackTimer = 0f;

    private bool isDead = false;
    private bool isAlerted = false; 
    private bool isPatrolling = false;
    private float patrolTimer = 0f;
    private float patrolDirection = 1f; 

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

        if (isAttacking || stateInfo.IsName("Alert") || stateInfo.IsName("Hit") || stateInfo.IsName("Blink"))
        {
            StopMoving();
            if (isAttacking) animator.SetBool("isAttack", false);
            return;
        }

        // 1. Tính toán các loại khoảng cách
        float distanceToPlayer = Vector2.Distance(player.position, transform.position);
        float directionX = player.position.x - transform.position.x;
        
        // Khoảng cách chiều dọc (Y)
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
        
        // Khoảng cách chiều ngang (X) so với nhà
        float playerDistanceFromHomeX = Mathf.Abs(player.position.x - startPos.x);

        // 2. Kiểm tra điều kiện Player có "hợp lệ" để đuổi không
        bool isPlayerInTerritory = playerDistanceFromHomeX <= territoryRadius;
        bool isPlayerAtSameLevel = verticalDistance <= verticalDetectionLimit;

        // ==========================================
        // BỘ NÃO AI CỦA QUÁI
        // ==========================================
        
        // Chỉ tấn công và rượt đuổi nếu Player KHÔNG quá cao/thấp
        if (isPlayerAtSameLevel && distanceToPlayer <= attackRange)
        {
            StopMoving();
            FlipTowards(directionX);

            if (attackTimer >= attackCooldown)
            {
                animator.SetBool("isAttack", true);
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", false);
                attackTimer = 0f; 
            }
        }
        else if (isPlayerAtSameLevel && distanceToPlayer <= detectionRange && isPlayerInTerritory)
        {
            // PLAYER Ở TRONG TẦM MẮT VÀ CÙNG ĐỘ CAO CHO PHÉP
            if (!isAlerted)
            {
                StopMoving();
                FlipTowards(directionX);
                animator.SetTrigger("alert"); 
                isAlerted = true; 
            }
            else
            {
                animator.SetBool("isAttack", false);
                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);

                float dirNormal = Mathf.Sign(directionX);
                rb.linearVelocity = new Vector2(dirNormal * runSpeed, rb.linearVelocity.y);
                FlipTowards(directionX);
            }
        }
        else 
        {
            // NẾU PLAYER NHẢY QUÁ CAO HOẶC RA KHỎI LÃNH THỔ -> QUAY LẠI TUẦN TRA
            isAlerted = false; 
            animator.SetBool("isAttack", false);
            animator.SetBool("isRun", false);
            
            PatrolLogic(); 
        }
    }

    private void PatrolLogic()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            patrolTimer = Random.Range(3f, 4f);
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
                if (Random.Range(0, 2) == 0) animator.Play("Blink"); 
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
        if (directionX > 0.1f) currentScale.x = -Mathf.Abs(currentScale.x); 
        else if (directionX < -0.1f) currentScale.x = Mathf.Abs(currentScale.x);
        transform.localScale = currentScale;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        hp -= damageAmount;
        isAlerted = true; 
        if (hp <= 0) { Die(); return; }
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
        Gizmos.color = Color.yellow;
        Vector2 drawPos = Application.isPlaying ? startPos : (Vector2)transform.position;
        
        // Vẽ lãnh thổ ngang
        Vector3 leftBound = new Vector3(drawPos.x - territoryRadius, drawPos.y, 0);
        Vector3 rightBound = new Vector3(drawPos.x + territoryRadius, drawPos.y, 0);
        Gizmos.DrawLine(leftBound, rightBound);
        
        // Vẽ giới hạn chiều cao (để bạn dễ căn chỉnh)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * verticalDetectionLimit);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.up * verticalDetectionLimit);
    }
}