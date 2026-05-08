using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D bossCollider;
    public Collider2D playerCollider; 
    
    [Header("--- Boss Stats ---")]
    public float hp = 1000f;

    [Header("--- Movement ---")]
    public float moveSpeed = 1f;
    public Transform player; 
    public float chaseRange = 15f; 
    
    [Header("--- Combat AI ---")]
    public float attackRange = 8f; 
    public float globalCooldown = 2.5f; 
    private float attackTimer = 0f;

    [Header("Skill Ranges & Speeds")]
    public float roarRange = 2.5f;   
    public float rollRange = 5.5f;   
    public float rollSpeed = 12f; 
    
    [Header("Take Off (Jump) Settings")]
    public float takeOffForceY = 8f;  
    public float takeOffSpeedX = 3f; 

    private bool isDead = false;
    private int lastAttack = -1; 
    
    private bool isJumping = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        bossCollider = GetComponent<Collider2D>();

        if (playerCollider == null && player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        // Nếu Boss đã chết, dừng toàn bộ vòng lặp AI tại đây
        if (isDead) return;

        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isRolling = stateInfo.IsName("RollAttack");
        
        if (bossCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(bossCollider, playerCollider, isRolling || isJumping);
        }

        if (isRolling)
        {
            float facingDir = transform.localScale.x == -1 ? 1f : -1f;
            rb.linearVelocity = new Vector2(facingDir * rollSpeed, rb.linearVelocity.y);
            return; 
        }

        // ====================================================================
        // DUY TRÌ ĐÀ NHẢY QUÁN TÍNH
        // ====================================================================
        if (isJumping)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                if (!stateInfo.IsName("TakeOff")) animator.Play("TakeOff");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                if (!stateInfo.IsName("Fall")) animator.Play("Fall");
            }
            else if (Mathf.Abs(rb.linearVelocity.y) < 0.05f && stateInfo.IsName("Fall"))
            {
                animator.Play("Landing");
                rb.linearVelocity = Vector2.zero; 
                isJumping = false; 
            }
            
            return; 
        }

        // ====================================================================
        // AI TẤN CÔNG VÀ ĐI BỘ
        // ====================================================================
        if (attackTimer < 0.1f) return;

        bool canAct = stateInfo.IsName("Idle") || stateInfo.IsName("Walk");
        if (!canAct)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; 
        }

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);
        float directionX = player.position.x - transform.position.x; 

        if (distanceToPlayer <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            FlipTowards(directionX); 

            if (attackTimer >= globalCooldown)
            {
                PerformSmartRandomAttack(distanceToPlayer);
                attackTimer = 0f;
            }
            else
            {
                animator.Play("Idle"); 
            }
        }
        else if (distanceToPlayer <= chaseRange)
        {
            float dirNormal = Mathf.Sign(directionX); 
            rb.linearVelocity = new Vector2(dirNormal * moveSpeed, rb.linearVelocity.y);
            animator.Play("Walk"); 
            FlipTowards(directionX);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.Play("Idle");
        }
    }

    private void PerformSmartRandomAttack(float distance)
    {
        List<int> possibleAttacks = new List<int>();

        if (distance <= roarRange) 
        {
            possibleAttacks.Add(0); 
            possibleAttacks.Add(1); 
        }
        else if (distance <= rollRange) 
        {
            possibleAttacks.Add(1); 
            possibleAttacks.Add(2); 
            possibleAttacks.Add(3); 
        }
        else 
        {
            possibleAttacks.Add(2); 
            possibleAttacks.Add(1); 
            possibleAttacks.Add(3); 
        }

        if (possibleAttacks.Count > 1 && possibleAttacks.Contains(lastAttack))
        {
            possibleAttacks.Remove(lastAttack);
        }

        int randomIndex = Random.Range(0, possibleAttacks.Count);
        int chosenAttack = possibleAttacks[randomIndex];
        lastAttack = chosenAttack;

        switch (chosenAttack)
        {
            case 0:
                animator.Play("RoarAnticipation"); 
                break;
            case 1:
                animator.Play("RollAttackAnticipation"); 
                break;
            case 2:
                animator.Play("SpikeAttackAnticipation"); 
                break;
            case 3:
                isJumping = true; 
                animator.Play("TakeOff"); 
                float jumpDir = transform.localScale.x == -1 ? 1f : -1f;
                rb.linearVelocity = new Vector2(jumpDir * takeOffSpeedX, takeOffForceY);
                break;
        }
    }

    private void FlipTowards(float directionX)
    {
        if (directionX > 0.1f)
            transform.localScale = new Vector3(-1, 1, 1); 
        else if (directionX < -0.1f)
            transform.localScale = new Vector3(1, 1, 1);  
    }

    public void TakeDamage(float damageAmount)
    {
        // 1. Nếu đã chết từ trước rồi thì bỏ qua luôn (chống bug chém trúng xác Boss chết nhiều lần)
        if (isDead) return;
        
        hp -= damageAmount;
        attackTimer = 0f; 
        
        // 2. KIỂM TRA CHẾT ƯU TIÊN HÀNG ĐẦU
        if (hp <= 0) 
        {
            Die();
            return; // THOÁT NGAY LẬP TỨC, không cho chạy xuống code Hit bên dưới nữa
        }
        
        // 3. Nếu chưa chết thì mới bị giật mình (Hit)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (isJumping || stateInfo.IsName("TakeOff") || stateInfo.IsName("Fall"))
        {
            animator.Play("HitAir");
            isJumping = false; // Gỡ trạng thái nhảy để nó rớt xuống
        }
        else 
        {
            animator.Play("HitGround"); 
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        isJumping = false; // Tắt luôn biến nhảy đề phòng đang bay mà chết
        
        animator.Play("Dead"); // Kích hoạt hình ảnh nằm gục
        
        // Cố định Boss tại chỗ, dập tắt mọi lực quán tính bay/lướt
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 0; 
        
        // Tắt hộp va chạm để người chơi không bị kẹt vào xác Boss
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }
    }
}