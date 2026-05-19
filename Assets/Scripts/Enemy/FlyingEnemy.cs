using UnityEngine;

public class FlyingEnemy : EnemySystemController
{
    [Header("--- Fan Attack Range (Tầm Đánh Hình Quạt) ---")]
    public float attackAngle = 90f;       // Độ xòe của quạt
    public float tiltDownAngle = 45f;     // Góc cắm đầu xuống đất
    
    [Header("--- Flight Logic ---")]
    public float minHeightAbovePlayer = 2f; 

    private Vector2 nextWaypoint;
    private bool hasWaypoint = false;
    private float waitTimer = 0f;

    protected Transform targetPlayer;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0f; 
        contactDamage = 10; 

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) targetPlayer = pObj.transform;
    }

    protected override void Update()
    {
        if (isDead) return;
        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        // Trạng thái Attack hoặc Hit -> Đứng im phăng phắc giữa không trung
        if (stateInfo.IsName("Attack") || stateInfo.IsName("Hit"))
        {
            rb.linearVelocity = Vector2.zero; // Dừng hẳn cả X và Y
            return;
        }

        HandleAI(); 
    }

    protected override void HandleAI()
    {
        if (targetPlayer == null)
        {
            PatrolInFullCircle();
            return;
        }

        float distanceToPlayer = Vector2.Distance(targetPlayer.position, transform.position);
        float distanceToHomeX = Mathf.Abs(targetPlayer.position.x - startPos.x);
        
        bool inTerritory = distanceToHomeX <= territoryRadius;
        bool inDetectionCircle = distanceToPlayer <= detectionRange;
        bool inAttackFan = false;
        
        if (distanceToPlayer <= attackRange)
        {
            float facingX = GetFacingDirection().x;
            Vector2 baseDir = facingX > 0 ? Vector2.right : Vector2.left;
            
            float actualTilt = facingX > 0 ? -tiltDownAngle : tiltDownAngle;
            Vector2 fanDirection = Quaternion.Euler(0, 0, actualTilt) * baseDir;

            Vector2 dirToPlayer = (targetPlayer.position - transform.position).normalized;
            float angleToPlayer = Vector2.Angle(fanDirection, dirToPlayer);

            if (angleToPlayer <= attackAngle / 2f)
            {
                inAttackFan = true;
            }
        }

        if (isAlerted && inTerritory && !inAttackFan)
        {
            FlipTowards(targetPlayer.position.x);
        }

        // KHI ĐANG ĐÁNH THÌ DỪNG LẠI
        if (inAttackFan)
        {
            rb.linearVelocity = Vector2.zero; // Ép dừng X và Y
            anim.SetBool("isFly", false);
            FlipTowards(targetPlayer.position.x);

            if (attackTimer >= attackCooldown)
            {
                anim.SetTrigger("Attack");
                PerformAttack();
                attackTimer = 0f;
            }
        }
        else if ((inDetectionCircle || isAlerted) && inTerritory)
        {
            ChasePlayer();
        }
        else
        {
            isAlerted = false;
            PatrolInFullCircle();
        }
    }

    protected virtual void PerformAttack() 
    {
    }

    private void ChasePlayer()
    {
        hasWaypoint = false;
        
        float targetY = targetPlayer.position.y + minHeightAbovePlayer;
        Vector2 targetPos = new Vector2(targetPlayer.position.x, targetY);
        
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = direction * chaseSpeed;
        anim.SetBool("isFly", true);
        FlipTowards(targetPlayer.position.x);
    }

    private void PatrolInFullCircle()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero; // Dừng chờ
            anim.SetBool("isFly", false);
            return;
        }

        if (!hasWaypoint)
        {
            Vector2 randomCircle = Random.insideUnitCircle * territoryRadius;
            nextWaypoint = startPos + new Vector2(randomCircle.x, Mathf.Abs(randomCircle.y));
            hasWaypoint = true;
            anim.SetBool("isFly", true);
        }

        if (Vector2.Distance(transform.position, nextWaypoint) < 0.2f)
        {
            hasWaypoint = false;
            waitTimer = Random.Range(1f, 3f);
            rb.linearVelocity = Vector2.zero; // Tới nơi thì dừng
        }
        else
        {
            Vector2 dir = (nextWaypoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
            FlipTowards(nextWaypoint.x);
        }
    }

    public override void Die()
    {
        base.Die();
        rb.gravityScale = 2f; 
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPos : (Vector2)transform.position, territoryRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        float facingX = Application.isPlaying && spriteRenderer != null ? GetFacingDirection().x : (transform.localScale.x < 0 ? -1f : 1f);
        Vector2 baseDir = facingX > 0 ? Vector2.right : Vector2.left;
        float actualTilt = facingX > 0 ? -tiltDownAngle : tiltDownAngle;
        Vector2 fanDirection = Quaternion.Euler(0, 0, actualTilt) * baseDir;

        DrawFanGizmo(transform.position, fanDirection, attackRange, attackAngle);
    }

    private void DrawFanGizmo(Vector2 origin, Vector2 direction, float radius, float angle)
    {
        float halfAngle = angle / 2f;
        Quaternion leftRot = Quaternion.Euler(0, 0, halfAngle);
        Quaternion rightRot = Quaternion.Euler(0, 0, -halfAngle);

        Vector2 leftRay = leftRot * direction * radius;
        Vector2 rightRay = rightRot * direction * radius;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, leftRay);
        Gizmos.DrawRay(origin, rightRay);

        int segments = 20;
        Vector2 prevPoint = origin + leftRay;
        float step = angle / segments;

        for (int i = 1; i <= segments; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, halfAngle - step * i);
            Vector2 nextPoint = origin + (Vector2)(rot * direction) * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}