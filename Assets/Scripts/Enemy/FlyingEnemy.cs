using UnityEngine;

public class FlyingEnemy : EnemySystemController
{
    [Header("--- Coordinate Vision (Tầm nhìn Cắm Xuống) ---")]
    public float visionWidthX = 8f;  // Nhìn xa về phía trước bao nhiêu
    public float visionDepthY = 6f;  // Nhìn sâu xuống dưới đất bao nhiêu
    
    [Header("--- Flight Logic ---")]
    public float minHeightAbovePlayer = 2f; // Mục tiêu bay luôn cao hơn Player (Bạn dặn ít nhất 0.5f, mình để mặc định 2f cho an toàn)

    private Vector2 nextWaypoint;
    private bool hasWaypoint = false;
    private float waitTimer = 0f;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0f; 
    }

    protected override void HandleAI()
    {
        float diffX = player.position.x - transform.position.x;
        float diffY = player.position.y - transform.position.y;
        float distanceToHomeX = Mathf.Abs(player.position.x - startPos.x);

        // 1. Nhìn Xuống: Tọa độ Y của player PHẢI THẤP HƠN quái (diffY < 0) và không sâu quá visionDepthY
        bool isPlayerBelow = diffY < 0 && diffY >= -visionDepthY;

        // 2. Nhìn Tới: Tọa độ X phải nằm TRƯỚC MẶT quái
        bool isFacingRight = GetFacingDirection().x > 0;
        bool isPlayerInFront = (isFacingRight && diffX > 0 && diffX <= visionWidthX) || 
                               (!isFacingRight && diffX < 0 && diffX >= -visionWidthX);

        bool inTerritory = distanceToHomeX <= territoryRadius;

        // BẮT BUỘC: Ở phía trước + Ở bên dưới + Trong lãnh thổ thì mới thấy
        bool canSeePlayer = isPlayerBelow && isPlayerInFront;

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceToPlayer <= attackRange && canSeePlayer)
        {
            StopMoving();
            anim.SetBool("isFly", false);
            FlipTowards(player.position.x);

            if (attackTimer >= attackCooldown)
            {
                anim.SetTrigger("Attack");
                PerformAttack();
                attackTimer = 0f;
            }
        }
        else if ((canSeePlayer || isAlerted) && inTerritory)
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
        // Kế thừa cho quái nhả đạn / lửa
    }

    private void ChasePlayer()
    {
        hasWaypoint = false;
        
        // MỚI: QUÁI LUÔN TÌM CÁCH BAY TRÊN ĐẦU NGƯỜI CHƠI (Tọa độ Y luôn lớn hơn player.Y + minHeight)
        float targetY = player.position.y + minHeightAbovePlayer;
        Vector2 targetPos = new Vector2(player.position.x, targetY);
        
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = direction * chaseSpeed;
        anim.SetBool("isFly", true);
        FlipTowards(player.position.x);
    }

    private void PatrolInFullCircle()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            StopMoving();
            anim.SetBool("isFly", false);
            return;
        }

        if (!hasWaypoint)
        {
            // Điểm tuần tra luôn sinh ra ở tọa độ Y ngang bằng hoặc cao hơn vị trí ban đầu
            Vector2 randomCircle = Random.insideUnitCircle * territoryRadius;
            nextWaypoint = startPos + new Vector2(randomCircle.x, Mathf.Abs(randomCircle.y));
            hasWaypoint = true;
            anim.SetBool("isFly", true);
        }

        if (Vector2.Distance(transform.position, nextWaypoint) < 0.2f)
        {
            hasWaypoint = false;
            waitTimer = Random.Range(1f, 3f);
            StopMoving();
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

    // VẼ KHUNG CHỮ NHẬT TẦM NHÌN CẮM XUỐNG DƯỚI
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Vector2 facingDir = Application.isPlaying ? GetFacingDirection() : (transform.localScale.x < 0 ? Vector2.right : Vector2.left);
        
        // Khung chữ nhật lệch về phía trước (X) và lệch xuống dưới (Y)
        Vector3 center = transform.position + new Vector3(facingDir.x * visionWidthX / 2f, -visionDepthY / 2f, 0);
        Vector3 size = new Vector3(visionWidthX, visionDepthY, 0);

        Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Tím mờ
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(center, size);
    }
}