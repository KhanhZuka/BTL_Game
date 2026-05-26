using System.Collections; // Bắt buộc phải có dòng này để dùng Coroutine
using UnityEngine;

public class GroundMeleeEnemy : GroundEnemy
{
    [Header("--- Frontal Melee Setup ---")]
    public Vector2 attackHitboxSize = new Vector2(1.5f, 2f); 
    public Vector2 attackHitboxOffset = new Vector2(1f, 0f); 
    public int meleeDamage = 25;     
    
    [Header("--- Hit Effects & Timing ---")]
    public GameObject hitEffectPrefab; 
    
    
    public float damageDelay = 0.05f; 

    protected override void Start()
    {
        maxHp = 200f; 
        base.Start();
        contactDamage = 10; 

        // Ép tầm đánh bằng khoảng cách Hitbox
        attackRange = attackHitboxOffset.x + (attackHitboxSize.x / 2f) - 0.2f; 
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Khởi động luồng đếm thời gian song song với Animation
        StartCoroutine(CalculateDamageDelay());
    }

    // Coroutine: Chủ động tính toán thời gian chạy
    private IEnumerator CalculateDamageDelay()
    {
        // 1. Đợi đúng 1 khoảng thời gian damageDelay
        yield return new WaitForSeconds(damageDelay);

        // 2. Gây sát thương sau khi đã đợi xong
        DealDamage();
    }

    public void DealDamage()
    {
        if (isDead || targetPlayer == null || !targetPlayer.CompareTag("Player")) return;

        float facingDirX = GetFacingDirection().x;
        Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(attackHitboxOffset.x * facingDirX, attackHitboxOffset.y);
        Vector2 playerPos = targetPlayer.position;

        bool isInHitboxX = Mathf.Abs(playerPos.x - hitboxCenter.x) <= (attackHitboxSize.x / 2f);
        bool isInHitboxY = Mathf.Abs(playerPos.y - hitboxCenter.y) <= (attackHitboxSize.y / 2f);

        if (isInHitboxX && isInHitboxY)
        {
            PlayerController stats = targetPlayer.GetComponent<PlayerController>();
            if (stats != null)
            {
                stats.ChangeHealth(-meleeDamage);

                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, targetPlayer.position, Quaternion.identity);
                    Destroy(effect, 1f); 
                }
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        float facingDirX = Application.isPlaying && spriteRenderer != null ? GetFacingDirection().x : (transform.localScale.x < 0 ? -1f : 1f);
        Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(attackHitboxOffset.x * facingDirX, attackHitboxOffset.y);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); 
        Gizmos.DrawCube(hitboxCenter, attackHitboxSize);
        Gizmos.color = Color.red; 
        Gizmos.DrawWireCube(hitboxCenter, attackHitboxSize);

        Gizmos.color = Color.blue;
        float range = attackHitboxOffset.x + (attackHitboxSize.x / 2f) - 0.2f;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}