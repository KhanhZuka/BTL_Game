using UnityEngine;

public class GroundDashEnemy : GroundEnemy
{
    [Header("--- Contact Setup ---")]
    public float bounceForce = 5f; 

    private float damageTimer = 0f;
    public float damageCooldown = 1f; // Tránh việc người chơi bị trừ máu liên tục 60 lần/giây khi đứng dính vào nó

    protected override void Start()
    {
        maxHp = 100f; // Máu trung bình
        contactDamage = 30; // Sát thương khi tông trúng

        base.Start();
        
        // CỐT LÕI NẰM Ở ĐÂY: Xóa nhận diện người chơi để AI tự động chuyển vĩnh viễn sang trạng thái Patrol (Đi tuần)
        targetPlayer = null; 

        attackRange = 0f; 
        hasAttackAnim = false;
    }

    // Bỏ trống hàm này vì sát thương của nó không xài Animation Trigger hay sinh ra vũ khí
    protected override void PerformAttack() 
    { 
    }
    
    // 1. Gây sát thương ngay cú chạm đầu tiên
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    // 2. Gây sát thương nếu Player bị dồn vào góc tường và kẹt chung với quái
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageCooldown)
            {
                DealDamage(collision.gameObject);
                damageTimer = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            damageTimer = 0f; 
        }
    }

    private void DealDamage(GameObject playerObj)
    {
        PlayerController playerStats = playerObj.GetComponent<PlayerController>();
        if (playerStats != null)
        {
            playerStats.ChangeHealth(-contactDamage);
            
            float bounceDirection = transform.localScale.x < 0 ? -1f : 1f; 
            rb.linearVelocity = new Vector2(-bounceDirection * bounceForce, rb.linearVelocity.y);
            
            if(anim != null) anim.SetTrigger("Hit"); 
        }
    }
}