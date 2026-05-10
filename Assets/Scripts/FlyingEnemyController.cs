using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public Transform player;

    [Header("--- Enemy Stats ---")]
    public float hp = 60f; 

    [Header("--- Flight Speeds ---")]
    public float flySpeed = 2f;    
    public float chaseSpeed = 4f;  

    [Header("--- Territory & Vision ---")]
    public float territoryRadius = 4f; // PHẠM VI BAY DẠO
    public float visionRadius = 5f;    
    private Vector2 startPos; 

    [Header("--- Combat ---")]
    public float attackRange = 4f;     // TẦM ĐÁNH XA LÊN
    public float attackCooldown = 2f; 
    private float attackTimer = 0f;

    [Header("--- Ranged Setup ---")]
    public Transform firePoint; 
    public GameObject projectilePrefab; 

    private bool isDead = false;
    private bool isChasing = false; 
    
    private Vector2 nextWaypoint;
    private bool hasWaypoint = false;
    private float waitTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player2");
            if (pObj != null) player = pObj.transform;
        }
        
        attackTimer = attackCooldown; 
        startPos = transform.position; 
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttacking = stateInfo.IsName("Attack");

        // Khi đang ở state Attack thì đứng im
        if (isAttacking || stateInfo.IsName("Hit"))
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isFly", false); 
            return;
        }

        if (attackTimer < 0.1f) return;

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);
        float distanceToHome = Vector2.Distance(player.position, startPos);

        bool inVisionRange = distanceToPlayer <= visionRadius;
        bool isBelow = player.position.y < transform.position.y;
        
        bool isFacingRight = transform.localScale.x < 0;
        bool isPlayerOnRight = player.position.x > transform.position.x;
        bool isFacingPlayer = (isFacingRight && isPlayerOnRight) || (!isFacingRight && !isPlayerOnRight);

        bool canSeePlayer = inVisionRange && isBelow && isFacingPlayer;
        bool inTerritory = distanceToHome <= territoryRadius;

      
        if (distanceToPlayer <= attackRange && canSeePlayer) 
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isFly", false);
            
            float directionX = player.position.x - transform.position.x;
            FlipTowards(directionX);

            if (attackTimer >= attackCooldown)
            {
                // 1. Chạy Animation đánh
                animator.SetTrigger("Attack"); 
                
                // 2. TẠO VIÊN ĐẠN NGAY LẬP TỨC
                if (projectilePrefab != null && firePoint != null)
                {
                    GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                    FlyingBullet script = bullet.GetComponent<FlyingBullet>();
                    if (script != null)
                    {
                        Vector2 aimDirection = (player.position - firePoint.position).normalized;
                        script.Setup(aimDirection);
                    }
                }

                // 3. Reset thời gian chờ
                attackTimer = 0f; 
            }
        }
        else if ((canSeePlayer || isChasing) && inTerritory)
        {
            isChasing = true; 
            hasWaypoint = false; 

            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            
            animator.SetBool("isFly", true);
            FlipTowards(direction.x);
        }
        else 
        {
            isChasing = false; 
            PatrolInFullCircle(); 
        }
    }

    private void PatrolInFullCircle()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isFly", false); 
            return;
        }

        if (!hasWaypoint)
        {
            nextWaypoint = startPos + Random.insideUnitCircle * territoryRadius;
            hasWaypoint = true;
            animator.SetBool("isFly", true);
        }

        float distanceToWaypoint = Vector2.Distance(transform.position, nextWaypoint);
        
        if (distanceToWaypoint < 0.2f)
        {
            hasWaypoint = false;
            waitTimer = Random.Range(1f, 3f);
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 dir = (nextWaypoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * flySpeed;
            FlipTowards(dir.x);
        }
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
        isChasing = true; 
        if (hp <= 0) { Die(); return; }
        animator.Play("Hit"); 
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.Play("Dead"); 
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 2f; 
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center = Application.isPlaying ? startPos : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, territoryRadius);

        Gizmos.color = Color.red;
        bool isFacingRight = transform.localScale.x < 0;
        float startAngle = isFacingRight ? 270f : 180f;
        float endAngle = isFacingRight ? 360f : 270f;
        int segments = 15;
        float angleStep = (endAngle - startAngle) / segments;
        Vector3 prevPoint = transform.position + new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad), 0) * visionRadius;
        Gizmos.DrawLine(transform.position, prevPoint);
        for (int i = 1; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 newPoint = transform.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * visionRadius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
        Gizmos.DrawLine(transform.position, prevPoint);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}