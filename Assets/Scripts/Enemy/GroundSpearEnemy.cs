using UnityEngine;

public class GroundSpearEnemy : GroundEnemy
{
    [Header("--- Ranged Setup ---")]
    public float throwRange = 7f; //  tầm ném lao
    public Transform firePoint;
    public GameObject spearPrefab;

    protected override void Start()
    {
        maxHp = 120f; 
        base.Start();
        
        // Quái sẽ dừng lại và ném ngay khi Player lọt vào khoảng cách này
        attackRange = throwRange; 
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (spearPrefab != null && firePoint != null)
        {
            GameObject spear = Instantiate(spearPrefab, firePoint.position, firePoint.rotation);
            
            // FIX LỖI HƯỚNG NÉM: Dùng luôn hàm GetFacingDirection() của base class 
            // để đảm bảo mặt quay hướng nào, lao bay hướng đó
            float facingDir = GetFacingDirection().x; 
            
            SpearProjectile script = spear.GetComponent<SpearProjectile>();
            if (script != null) script.Setup(facingDir);
        }
    }
}