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

        // Ngừng di chuyển khi đang có animation đặc biệt
        if (isAttacking || stateInfo.IsName("Alert") || stateInfo.IsName("Hit") || stateInfo.IsName("Blink"))
        {
            StopMoving();
            if (isAttacking) animator.SetBool("isAttack", false);
            return;
        }

        // 1. Tính toán các loại khoảng cách
        float distanceToPlayer = Vector2.Distance(player.position, transform.position);
        float directionX = player.position.x - transform.position.x;
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
        float playerDistanceFromHomeX = Mathf.Abs(player.position.x - startPos.x);

        // 2. Các điều kiện cơ bản
        bool isPlayerInTerritory = playerDistanceFromHomeX <= territoryRadius;
        bool isPlayerAtSameLevel = verticalDistance <= verticalDetectionLimit;

        // 3. ĐIỀU KIỆN QUAN TRỌNG: Quái có đang quay mặt về phía Player không?
        // Theo hàm FlipTowards của bạn: scale.x âm là quay phải, dương là quay trái
        bool isFacingRight = transform.localScale.x < 0; 
        bool isPlayerToRight = directionX > 0;
        bool isFacingPlayer = (isFacingRight && isPlayerToRight) || (!isFacingRight && !isPlayerToRight);

        // ==========================================
        // BỘ NÃO AI CỦA QUÁI
        // ==========================================
        
        // Quái chỉ "thấy" bạn khi: Cùng độ cao + Trong tầm nhìn + Trong lãnh thổ + ĐANG QUAY MẶT VỀ PHÍA BẠN
        bool canSeePlayer = isPlayerAtSameLevel && distanceToPlayer <= detectionRange && isPlayerInTerritory && isFacingPlayer;

        // XỬ LÝ LÚC MỚI PHÁT HIỆN
        if (canSeePlayer && !isAlerted)
        {
            StopMoving();
            FlipTowards(directionX);
            animator.SetTrigger("alert"); 
            isAlerted = true; // Chuyển sang trạng thái đã phát hiện mục tiêu
            return; 
        }

        // XỬ LÝ KHI ĐANG TRONG TRẠNG THÁI RƯỢT ĐUỔI (Đã Alert)
        if (isAlerted)
        {
            // Kiểm tra xem Player còn trong phạm vi đuổi không (không cần isFacingPlayer nữa vì đang rượt)
            if (isPlayerAtSameLevel && distanceToPlayer <= loseSightRange && isPlayerInTerritory)
            {
                // Nếu lọt vào tầm đánh -> Tấn công
                if (distanceToPlayer <= attackRange)
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
                // Nếu ở ngoài tầm đánh -> Chạy lại gần
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
                // MẤT DẤU (Player chạy quá xa, nhảy lên cao, hoặc ra khỏi lãnh thổ)
                isAlerted = false; 
                animator.SetBool("isAttack", false);
                animator.SetBool("isRun", false);
                PatrolLogic(); 
            }
        }
        // NẾU CHƯA PHÁT HIỆN GÌ (Player ở sau lưng hoặc ngoài tầm)
        else
        {
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
        
        // Dù đang quay lưng nhưng nếu bị đánh trúng sẽ tự động giật mình (Alert) và quay lại đánh
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
        
        Vector3 leftBound = new Vector3(drawPos.x - territoryRadius, drawPos.y, 0);
        Vector3 rightBound = new Vector3(drawPos.x + territoryRadius, drawPos.y, 0);
        Gizmos.DrawLine(leftBound, rightBound);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * verticalDetectionLimit);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.up * verticalDetectionLimit);
    }
}