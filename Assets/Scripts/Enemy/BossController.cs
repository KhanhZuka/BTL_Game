using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class BossController : EnemySystemController
{
    private Collider2D bossCollider;
    
    // Tự quản lý mục tiêu do class cha đã ẩn/xóa biến player
    private Transform targetPlayer; 
    public Collider2D[] playerColliders; 
    
    [Header("--- Boss Skill Ranges & Speeds ---")]
    public float roarRange = 2.5f;   
    public float rollRange = 5.5f;   
    public float rollSpeed = 12f; 
    
    [Header("--- Take Off (Jump) Settings ---")]
    public float takeOffForceY = 8f;  
    public float takeOffSpeedX = 6f; 
    public float minJumpDistance = 4f; 

    [Header("--- Knockback ---")]
    public float knockbackForceX = 10f; 
    public float knockbackForceY = 3f;  

    [Header("--- Spike Attack (Bắn 2 bên) ---")]
    public GameObject spikeBulletPrefab; 
    public Transform leftShootPoint;     
    public Transform rightShootPoint;
    public float spikeSpawnDelay = 1.5f; 

    private int lastAttack = -1; 
    private bool isJumping = false; 

    protected override void Start()
    {
        maxHp = 1000f;          
        attackCooldown = 2.5f;  
        detectionRange = 15f;   
        attackRange = 8f; 
        
        // Cài đặt sát thương khi Player vô tình chạm vào Boss
        contactDamage = 1; 

        base.Start(); 

        bossCollider = GetComponent<Collider2D>();

        // Tự tìm Player qua Tag khi bắt đầu
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            targetPlayer = pObj.transform;
            playerColliders = pObj.GetComponentsInChildren<Collider2D>();
        }
    }

    protected override void Update()
    {
        if (isDead || targetPlayer == null) return;
        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool isRolling = stateInfo.IsName("RollAttack");
        
        if (bossCollider != null && playerColliders != null)
        {
            foreach (Collider2D pCol in playerColliders)
            {
                Physics2D.IgnoreCollision(bossCollider, pCol, isRolling || isJumping);
            }
        }

        if (isRolling)
        {
            float facingDir = GetFacingDirection().x; 
            rb.linearVelocity = new Vector2(facingDir * rollSpeed, rb.linearVelocity.y);
            return; 
        }

        if (isJumping)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                if (!stateInfo.IsName("TakeOff")) anim.Play("TakeOff");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                if (!stateInfo.IsName("Fall")) anim.Play("Fall");
            }
            else if (Mathf.Abs(rb.linearVelocity.y) < 0.05f && stateInfo.IsName("Fall"))
            {
                anim.Play("Landing");
                StopMoving(); 
                isJumping = false; 
            }
            return; 
        }

        HandleAI();
    }

    protected override void HandleAI()
    {
        if (attackTimer < 0.1f) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool canAct = stateInfo.IsName("Idle") || stateInfo.IsName("Walk");
        
        if (!canAct)
        {
            StopMoving();
            return; 
        }

        float distanceToPlayer = Vector2.Distance(targetPlayer.position, transform.position);
        float directionX = targetPlayer.position.x - transform.position.x; 

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            FlipTowards(targetPlayer.position.x); 

            if (attackTimer >= attackCooldown)
            {
                PerformSmartRandomAttack(distanceToPlayer);
                attackTimer = 0f;
            }
            else
            {
                anim.Play("Idle"); 
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            float dirNormal = Mathf.Sign(directionX); 
            rb.linearVelocity = new Vector2(dirNormal * moveSpeed, rb.linearVelocity.y);
            anim.Play("Walk"); 
            FlipTowards(targetPlayer.position.x);
        }
        else
        {
            StopMoving();
            anim.Play("Idle");
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

        if (distance < minJumpDistance && possibleAttacks.Contains(3))
        {
            possibleAttacks.Remove(3);
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
                anim.Play("RoarAnticipation"); 
                break;
            case 1:
                anim.Play("RollAttackAnticipation"); 
                break;
            case 2:
                anim.Play("SpikeAttackAnticipation"); 
                StartCoroutine(SpawnSpikesRoutine(spikeSpawnDelay));
                break;
            case 3:
                isJumping = true; 
                anim.Play("TakeOff"); 
                
                float distanceX = targetPlayer.position.x - transform.position.x;
                float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
                
                if (gravity > 0.1f) 
                {
                    float timeInAir = (2f * takeOffForceY) / gravity;
                    float requiredSpeedX = distanceX / timeInAir;
                    rb.linearVelocity = new Vector2(requiredSpeedX, takeOffForceY);
                }
                else
                {
                    float jumpDirX = Mathf.Sign(distanceX); 
                    rb.linearVelocity = new Vector2(jumpDirX * takeOffSpeedX, takeOffForceY);
                }
                break;
        }
    }

    private IEnumerator SpawnSpikesRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (spikeBulletPrefab != null)
        {
            Vector3 trueLeftPos = transform.position + Vector3.left;
            Vector3 trueRightPos = transform.position + Vector3.right;

            if (leftShootPoint != null && rightShootPoint != null)
            {
                if (leftShootPoint.position.x < rightShootPoint.position.x)
                {
                    trueLeftPos = leftShootPoint.position;
                    trueRightPos = rightShootPoint.position;
                }
                else
                {
                    trueLeftPos = rightShootPoint.position;
                    trueRightPos = leftShootPoint.position;
                }
            }

            // 1. VIÊN BẮN SANG TRÁI MÀN HÌNH (Góc xoay 180 độ)
            GameObject leftBullet = Instantiate(spikeBulletPrefab, trueLeftPos, Quaternion.Euler(0, 0, 180f));
            
            FlyingBullet flyLeft = leftBullet.GetComponent<FlyingBullet>();
            if (flyLeft != null) flyLeft.Launch(); 
            else 
            {
                SpearProjectile spearLeft = leftBullet.GetComponent<SpearProjectile>();
                if (spearLeft != null) spearLeft.Setup(-1f); 
                if (spearLeft != null) spearLeft.Setup(-1f); 
            }

            // 2. VIÊN BẮN SANG PHẢI MÀN HÌNH (Góc xoay 0 độ)
            GameObject rightBullet = Instantiate(spikeBulletPrefab, trueRightPos, Quaternion.identity);
            
            FlyingBullet flyRight = rightBullet.GetComponent<FlyingBullet>();
            if (flyRight != null) flyRight.Launch(); 
            else 
            {
                SpearProjectile spearRight = rightBullet.GetComponent<SpearProjectile>();
                if (spearRight != null) spearRight.Setup(1f); 
                if (spearRight != null) spearRight.Setup(1f); 
            }
        }
    }
    
    
    public override void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        
        currentHp -= damageAmount;
        isAlerted = true; 
        
        if (currentHp <= 0) 
        {
            Die();
            return; 
        }
        
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        bool isUsingHeavySkill = stateInfo.IsName("RollAttack") || 
                                 stateInfo.IsName("RollAttackAnticipation") || 
                                 stateInfo.IsName("RoarAnticipation") ||
                                 stateInfo.IsName("SpikeAttackAnticipation");

        if (isUsingHeavySkill)
        {
            return; 
        }

        attackTimer = 0f; 
        
        if (isJumping || stateInfo.IsName("TakeOff") || stateInfo.IsName("Fall"))
        {
            anim.Play("HitAir");
            isJumping = false; 
        }
        else 
        {
            anim.Play("HitGround"); 
        }
    }

    // ĐÃ THÊM OVERRIDE: Trừ máu xong mới hất văng
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Gọi class cha để TRỪ MÁU trước (Class cha sẽ lo vụ Check "Player")
        base.OnCollisionEnter2D(collision);

        // HẤT VĂNG PLAYER NẾU BỊ ĐỤNG TRÚNG
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                float knockbackDirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                playerRb.linearVelocity = new Vector2(knockbackDirX * knockbackForceX, knockbackForceY);
                
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                bool isRolling = stateInfo.IsName("RollAttack");
                if (!isRolling && !isJumping)
                {
                    rb.linearVelocity = Vector2.zero; 
                }
            }
        }
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        isJumping = false; 
        
        anim.Play("Dead"); 
        StopMoving();
        rb.gravityScale = 0; 
        
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }
    }
}