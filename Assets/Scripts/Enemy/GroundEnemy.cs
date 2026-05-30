using UnityEngine;

public class GroundEnemy : EnemySystemController
{
    [Header("--- Coordinate Vision (Tầm nhìn Ngang) ---")]
    public float visionWidthX = 10f;  
    public float visionHeightY = 1.5f; 

    [Header("--- Ground Patrol & Turn Around ---")]
    public Transform frontSensor;      
    public LayerMask obstacleLayer;    
    public float wallCheckDistance = 0.5f; 
    public float cliffCheckDistance = 1f;  

    protected float waitTimer = 0f;

    [Header("--- Animation Settings ---")]
    public bool hasWalkAnim = true;   
    public bool hasRunAnim = true;    
    public bool hasAttackAnim = true; 
    public bool hasAlertAnim = true;  

    protected Transform targetPlayer;
    private bool wasAlerted = false;  

    protected override void Start()
    {
        base.Start(); 
        rb.gravityScale = 1f; 

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) targetPlayer = pObj.transform;
    }

    protected override void HandleAI()
    {
        if (frontSensor != null)
        {
            float facingX = GetFacingDirection().x;
            Vector3 sensorLocalPos = frontSensor.localPosition;
            sensorLocalPos.x = Mathf.Abs(sensorLocalPos.x) * facingX;
            frontSensor.localPosition = sensorLocalPos;
        }

        if (targetPlayer == null)
        {
            Patrol();
            return;
        }

        float diffX = targetPlayer.position.x - transform.position.x;
        float diffY = targetPlayer.position.y - transform.position.y;
        float distanceFromHomeX = Mathf.Abs(targetPlayer.position.x - startPos.x);

        // 1. Kiểm tra độ cao
        bool isAtSameLevel = Mathf.Abs(diffY) <= visionHeightY;

        // 2. Kiểm tra hướng nhìn
        bool isFacingRight = GetFacingDirection().x > 0;
        bool isPlayerInFront = (isFacingRight && diffX > 0 && diffX <= visionWidthX) || 
                               (!isFacingRight && diffX < 0 && diffX >= -visionWidthX);

        bool isPlayerInTerritory = distanceFromHomeX <= territoryRadius;
        
        // Tầm nhìn: Cùng độ cao + Ở trước mặt
        bool canSeePlayer = isAtSameLevel && isPlayerInFront;

        // TÍNH NĂNG THÔNG MINH
        if (isAlerted && !canSeePlayer && isPlayerInTerritory)
        {
            FlipTowards(targetPlayer.position.x);
            canSeePlayer = true; 
        }

        if (canSeePlayer)
        {
            // Kích hoạt animation Alert (giật mình/cảnh báo) khi vừa phát hiện
            if (!wasAlerted && hasAlertAnim)
            {
                anim.SetTrigger("alert");
            }
            wasAlerted = true;

            if (Mathf.Abs(diffX) <= attackRange)
            {
                isAlerted = true;
                StopMoving();
                FlipTowards(targetPlayer.position.x);
                
                if (attackTimer >= attackCooldown) 
                {
                    if (hasAttackAnim) anim.SetTrigger("Attack");
                    if (hasWalkAnim) anim.SetBool("isWalk", false);
                    if (hasRunAnim) anim.SetBool("isRun", false); // Dừng chạy khi đánh
                    PerformAttack(); 
                    attackTimer = 0f;
                }
            }
            else if (isPlayerInTerritory)
            {
                isAlerted = true;
                ChasePlayer();
            }
            else
            {
                isAlerted = false;
                wasAlerted = false; // Reset lại trạng thái alert
                Patrol();
            }
        }
        else
        {
            isAlerted = false;
            wasAlerted = false; // Reset lại trạng thái alert
            Patrol();
        }
    }

    protected virtual void PerformAttack() 
    {
    }

    private void ChasePlayer()
    {
        if (hasWalkAnim) anim.SetBool("isWalk", false); // Tắt animation đi bộ
        if (hasRunAnim) anim.SetBool("isRun", true);    // Kích hoạt animation chạy

        float dirNormal = Mathf.Sign(targetPlayer.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirNormal * chaseSpeed, rb.linearVelocity.y);
        FlipTowards(targetPlayer.position.x);
    }

    private void Patrol()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (hasWalkAnim) anim.SetBool("isWalk", false);
            if (hasRunAnim) anim.SetBool("isRun", false); 
            StopMoving();
            return;
        }

        float currentFacingDirX = GetFacingDirection().x;

        bool isHittingWall = false;
        bool isNearCliff = false;

        if (frontSensor != null)
        {
            Vector2 facingVector = new Vector2(currentFacingDirX, 0);
            isHittingWall = Physics2D.Raycast(frontSensor.position, facingVector, wallCheckDistance, obstacleLayer);
            isNearCliff = !Physics2D.Raycast(frontSensor.position, Vector2.down, cliffCheckDistance, obstacleLayer);
        }

        float distanceFromHomeX = transform.position.x - startPos.x;
        bool isOutOfTerritory = (currentFacingDirX > 0 && distanceFromHomeX >= territoryRadius) || 
                                (currentFacingDirX < 0 && distanceFromHomeX <= -territoryRadius);

        if (isHittingWall || isNearCliff || isOutOfTerritory)
        {
            FlipTowards(transform.position.x - currentFacingDirX); 
            waitTimer = Random.Range(1f, 2.5f);
            StopMoving();
            return; 
        }
        
        if (hasRunAnim) anim.SetBool("isRun", false);  // Tắt chạy khi tuần tra
        if (hasWalkAnim) anim.SetBool("isWalk", true); // Bật đi bộ
        rb.linearVelocity = new Vector2(currentFacingDirX * moveSpeed, rb.linearVelocity.y);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        float currentFacingDirX = Application.isPlaying && spriteRenderer != null ? GetFacingDirection().x : (transform.localScale.x < 0 ? -1f : 1f);
        Vector2 facingDir = new Vector2(currentFacingDirX, 0);
        
        Vector3 center = transform.position + new Vector3(facingDir.x * visionWidthX / 2f, 0, 0);
        Vector3 size = new Vector3(visionWidthX, visionHeightY * 2f, 0);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);

        if (frontSensor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(frontSensor.position, facingDir * wallCheckDistance);
            Gizmos.color = Color.green;  
            Gizmos.DrawRay(frontSensor.position, Vector3.down * cliffCheckDistance);
        }
    }
}