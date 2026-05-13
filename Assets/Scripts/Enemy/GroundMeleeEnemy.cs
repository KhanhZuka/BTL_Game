using UnityEngine;

public class GroundMeleeEnemy : GroundEnemy
{
    [Header("--- Melee Setup ---")]
    public Transform attackPoint;    // Điểm đánh (Tạo 1 object trống đặt ở trước mặt quái)
    public float meleeHitRadius = 0.8f; // Bán kính đòn đánh
    public int meleeDamage = 25;     // Sát thương cận chiến
    public LayerMask playerLayer;    // Chọn Layer của Player để đánh không bị nhầm vào quái khác

    protected override void Start()
    {
        maxHp = 200f; // Quái cận chiến thường trâu bò hơn
        base.Start();
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (attackPoint != null)
        {
            // Tạo một vòng tròn sát thương ở vị trí attackPoint
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRadius, playerLayer);

            foreach (Collider2D p in hitPlayers)
            {
                if (p.CompareTag("Player") || p.CompareTag("Player2"))
                {
                    PlayerController stats = p.GetComponent<PlayerController>();
                    if (stats != null)
                    {
                        stats.ChangeHealth(-meleeDamage);
                    }
                }
            }
        }
    }

    // Vẽ thêm vòng tròn đòn đánh cận chiến ra màn hình Editor để bạn dễ căn chỉnh độ to nhỏ
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (attackPoint != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(attackPoint.position, meleeHitRadius);
        }
    }
}