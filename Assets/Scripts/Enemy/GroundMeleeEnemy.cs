using UnityEngine;

public class GroundMeleeEnemy : GroundEnemy
{
    [Header("--- Frontal Melee Setup ---")]
    public Vector2 attackHitboxSize = new Vector2(1.5f, 2f); 
    public Vector2 attackHitboxOffset = new Vector2(1f, 0f); 
    
    public int meleeDamage = 25;     
    public LayerMask playerLayer;         
    
    [Header("--- Hit Effects ---")]
    public GameObject hitEffectPrefab; 

    protected override void Start()
    {
        maxHp = 200f; 
        base.Start();
        contactDamage = 10; 
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
    }

    public void DealDamage()
    {
        if (isDead) return;

        // Lấy hướng chém từ FlipX
        float facingDirX = GetFacingDirection().x;
        Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(attackHitboxOffset.x * facingDirX, attackHitboxOffset.y);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(hitboxCenter, attackHitboxSize, 0f, playerLayer);

        foreach (Collider2D p in hitPlayers)
        {
            if (p.CompareTag("Player"))
            {
                PlayerController stats = p.GetComponent<PlayerController>();
                if (stats != null)
                {
                    stats.ChangeHealth(-meleeDamage);

                    if (hitEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(hitEffectPrefab, p.transform.position, Quaternion.identity);
                        Destroy(effect, 1f); 
                    }
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
    }
}