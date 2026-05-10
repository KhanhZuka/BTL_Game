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
    public float detectionRange = 12f; 
    public float attackRange = 9f;       // TẦM ĐÁNH (Xa)
    public float verticalDetectionLimit = 1.5f; 
    public float territoryRadius = 5f;   // TẦM TUẦN TRA (Nhỏ lại)
    private Vector2 startPos; 

    [Header("--- Combat ---")]
    public float attackCooldown = 2.5f; 
    private float attackTimer = 0f;

    private bool isDead = false;
    private bool isAlerted = false; 
    
    private float waitTimer = 0f;
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
            GameObject pObj = GameObject.FindGameObjectWithTag("Player2");
            if (pObj != null) player = pObj.transform;
        }

        if (projectilePrefab == null)
        {
            projectilePrefab = Resources.Load<GameObject>("SpearProjectile");
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

        if (isAttacking || stateInfo.IsName("Hit"))
        {
            StopMoving();
            return;
        }

        if (attackTimer < 0.1f) return;

        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);
        float directionX = player.position.x - transform.position.x;
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
        float playerDistanceFromHomeX = Mathf.Abs(player.position.x - startPos.x);

        bool isPlayerInTerritory = playerDistanceFromHomeX <= territoryRadius;
        bool isPlayerAtSameLevel = verticalDistance <= verticalDetectionLimit;

        bool isFacingRight = transform.localScale.x < 0; 
        bool isPlayerToRight = directionX > 0;
        bool isFacingPlayer = (isFacingRight && isPlayerToRight) || (!isFacingRight && !isPlayerToRight);

        bool canSeePlayer = isPlayerAtSameLevel && distanceToPlayerX <= detectionRange && isPlayerInTerritory && isFacingPlayer;

        if (canSeePlayer && !isAlerted)
        {
            StopMoving();
            FlipTowards(directionX);
            isAlerted = true; 
            return;
        }

        if (isAlerted)
        {
            if (isPlayerAtSameLevel && distanceToPlayerX <= detectionRange && isPlayerInTerritory)
            {
                if (distanceToPlayerX <= attackRange)
                {
                    StopMoving();
                    FlipTowards(directionX);

                    if (attackTimer >= attackCooldown)
                    {
                        animator.SetTrigger("Attack");
                        animator.SetBool("isWalk", false);
                        
                        // ==========================================
                        // TẠO CÂY GIÁO NGAY TRONG UPDATE
                        // ==========================================
                        if (projectilePrefab != null && firePoint != null)
                        {
                            GameObject spear = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                            float facingDir = transform.localScale.x < 0 ? 1f : -1f;
                            
                            SpearProjectile script = spear.GetComponent<SpearProjectile>();
                            if (script != null) script.Setup(facingDir);
                        }

                        attackTimer = 0f; 
                    }
                }
                else
                {
                    animator.SetBool("isWalk", true);
                    float dirNormal = Mathf.Sign(directionX);
                    rb.linearVelocity = new Vector2(dirNormal * chaseSpeed, rb.linearVelocity.y);
                    FlipTowards(directionX);
                }
            }
            else 
            {
                isAlerted = false; 
                animator.SetBool("isWalk", false);
                PatrolLogic(); 
            }
        }
        else
        {
            animator.SetBool("isWalk", false);
            PatrolLogic();
        }
    }

    private void PatrolLogic()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            animator.SetBool("isWalk", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        animator.SetBool("isWalk", true);
        rb.linearVelocity = new Vector2(patrolDirection * walkSpeed, rb.linearVelocity.y);

        float distanceFromHomeX = transform.position.x - startPos.x;

        if ((patrolDirection == 1f && distanceFromHomeX >= territoryRadius) || 
            (patrolDirection == -1f && distanceFromHomeX <= -territoryRadius))
        {
            patrolDirection *= -1f;
            waitTimer = Random.Range(1f, 2.5f); 
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            
            Vector3 currentScale = transform.localScale;
            currentScale.x = patrolDirection == 1f ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
        Vector2 drawPos = Application.isPlaying ? startPos : (Vector2)transform.position;
        
        Gizmos.color = Color.yellow;
        Vector3 leftBound = new Vector3(drawPos.x - territoryRadius, drawPos.y, 0);
        Vector3 rightBound = new Vector3(drawPos.x + territoryRadius, drawPos.y, 0);
        Gizmos.DrawLine(leftBound, rightBound);
        Gizmos.DrawLine(leftBound + Vector3.up * 0.5f, leftBound - Vector3.up * 0.5f);
        Gizmos.DrawLine(rightBound + Vector3.up * 0.5f, rightBound - Vector3.up * 0.5f);
        
        Gizmos.color = Color.cyan;
        Vector3 attackLeft = new Vector3(transform.position.x - attackRange, transform.position.y, 0);
        Vector3 attackRight = new Vector3(transform.position.x + attackRange, transform.position.y, 0);
        Gizmos.DrawLine(attackLeft, attackRight);
        Gizmos.DrawLine(attackLeft + Vector3.up * 0.3f, attackLeft - Vector3.up * 0.3f);
        Gizmos.DrawLine(attackRight + Vector3.up * 0.3f, attackRight - Vector3.up * 0.3f);
    }
}