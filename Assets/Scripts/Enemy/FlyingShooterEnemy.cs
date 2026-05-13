using UnityEngine;

public class FlyingShooterEnemy : FlyingEnemy
{
    [Header("--- Ranged Attack ---")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            FlyingBullet script = bullet.GetComponent<FlyingBullet>();
            
            if (script != null)
            {
                Vector2 aimDirection = (player.position - firePoint.position).normalized;
                script.Setup(aimDirection);
            }
        }
    }
}