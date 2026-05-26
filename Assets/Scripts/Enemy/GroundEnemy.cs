using UnityEngine;

public class GroundEnemy : EnemySystemController
{
    [Header("--- Coordinate Vision (Tầm nhìn Ngang) ---")]
    public float visionWidthX = 10f;  
    public float visionHeightY = 1.5f; 

    [Header("--- Ground Patrol & Turn Around ---")]
    public Transform frontSensor;      // ĐIỂM DÒ ĐƯỜNG: Kéo 1 Object trống đặt ở mũi chân quái vào đây
    public LayerMask obstacleLayer;    // CHỌN LAYER: Chọn Layer của Tường/Đất để quái biết đâu là vật cản
    public float wallCheckDistance = 0.5f; 
    public float cliffCheckDistance = 1f;  

    protected float patrolDirection = 1f;
    protected float waitTimer = 0f;

    [Header("--- Animation Settings ---")]
    public bool hasWalkAnim = true;   
    public bool hasAttackAnim = true; 

    protected override void Start()
    {
        base.Start(); 
        rb.gravityScale = 1f; 
    }

    protected override void HandleAI()
{
    float diffX = player.position.x - transform.position.x;
    float diffY = player.position.y - transform.position.y;
    float distanceFromHomeX = Mathf.Abs(player.position.x - startPos.x);

    bool isAtSameLevel = Mathf.Abs(diffY) <= visionHeightY;
    
    // Tối ưu tầm nhìn: Chỉ cần nằm trong vùng hộp quét ngang là thấy (bất kể trước sau)
    bool isPlayerInVisionBox = Mathf.Abs(diffX) <= visionWidthX; 
    
    bool isPlayerInTerritory = distanceFromHomeX <= territoryRadius;
    
    // Chỉ cần cùng độ cao và nằm trong khoảng cách Vision là được tính là "Thấy"
    bool canSeePlayer = isAtSameLevel && isPlayerInVisionBox;

    if (canSeePlayer)
    {
        // 1. KIỂM TRA TẦM ĐÁNH TRƯỚC
        if (Mathf.Abs(diffX) <= attackRange)
        {
            // TRONG TẦM ĐÁNH -> Đứng lại quất luôn (Dù có ngoài lãnh thổ cũng ném)
            isAlerted = true;
            StopMoving();
            FlipTowards(player.position.x);
            
            if (attackTimer >= attackCooldown) 
            {
                if (hasAttackAnim) anim.SetTrigger("Attack");
                if (hasWalkAnim) anim.SetBool("isWalk", false);
                PerformAttack(); 
                attackTimer = 0f;
            }
        }
        // 2. NẾU NGOÀI TẦM ĐÁNH, MỚI XÉT ĐẾN LÃNH THỔ
        else if (isPlayerInTerritory)
        {
            // Còn trong lãnh thổ -> Chạy theo rượt
            isAlerted = true;
            ChasePlayer();
        }
        else
        {
            // Ngoài tầm đánh VÀ ngoài lãnh thổ -> Bỏ qua, đi tuần tra tiếp
            isAlerted = false;
            Patrol();
        }
    }
    else
    {
        // Không thấy Player -> Tuần tra
        isAlerted = false;
        Patrol();
    }
}

    protected virtual void PerformAttack() 
    {
    }

    private void ChasePlayer()
    {
        if (hasWalkAnim) anim.SetBool("isWalk", true);
        float dirNormal = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirNormal * chaseSpeed, rb.linearVelocity.y);
        FlipTowards(player.position.x);
    }

    private void Patrol()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (hasWalkAnim) anim.SetBool("isWalk", false);
            StopMoving();
            return;
        }

        // ==========================================
        //  KIỂM TRA QUAY ĐẦU (RAYCAST)
        // ==========================================
        bool isHittingWall = false;
        bool isNearCliff = false;

        if (frontSensor != null)
        {
            Vector2 facingDir = GetFacingDirection();
            
            // Bắn tia ngang xem có đụng tường không
            isHittingWall = Physics2D.Raycast(frontSensor.position, facingDir, wallCheckDistance, obstacleLayer);
            
            // Bắn tia cắm xuống xem có bị hụt chân (vực) không
            isNearCliff = !Physics2D.Raycast(frontSensor.position, Vector2.down, cliffCheckDistance, obstacleLayer);
        }

        float distanceFromHomeX = transform.position.x - startPos.x;
        bool isOutOfTerritory = (patrolDirection == 1f && distanceFromHomeX >= territoryRadius) || 
                                (patrolDirection == -1f && distanceFromHomeX <= -territoryRadius);

        // NẾU Đụng tường HOẶC Tới vực HOẶC Đi quá giới hạn lãnh thổ -> QUAY ĐẦU
        if (isHittingWall || isNearCliff || isOutOfTerritory)
        {
            patrolDirection *= -1f;
            waitTimer = Random.Range(1f, 2.5f);
            StopMoving();
            FlipTowards(transform.position.x + patrolDirection);
            return; // Đứng lại chờ quay đầu, bỏ qua việc đi tiếp ở frame này
        }

        
        if (hasWalkAnim) anim.SetBool("isWalk", true);
        rb.linearVelocity = new Vector2(patrolDirection * moveSpeed, rb.linearVelocity.y);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        Vector2 facingDir = Application.isPlaying ? GetFacingDirection() : (transform.localScale.x < 0 ? Vector2.right : Vector2.left);
        
        // Vẽ tầm nhìn hình hộp
        Vector3 center = transform.position + new Vector3(facingDir.x * visionWidthX / 2f, 0, 0);
        Vector3 size = new Vector3(visionWidthX, visionHeightY * 2f, 0);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);

        // VẼ 2 TIA LASER TRONG EDITOR ĐỂ BẠN CĂN CHỈNH
        if (frontSensor != null)
        {
            Gizmos.color = Color.yellow; // Tia vàng: Dò tường
            Gizmos.DrawRay(frontSensor.position, facingDir * wallCheckDistance);
            
            Gizmos.color = Color.green;  // Tia xanh lá: Dò vực
            Gizmos.DrawRay(frontSensor.position, Vector3.down * cliffCheckDistance);
        }
    }
}