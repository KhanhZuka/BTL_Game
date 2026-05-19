using UnityEngine;

public class FlyingShooterEnemy : FlyingEnemy
{
    [Header("--- Ranged Attack ---")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    
    public float offsetAngle = 0f; 

    protected override void PerformAttack()
    {
        base.PerformAttack();
    }

    public void ShootBullet()
    {
        if (isDead || targetPlayer == null) return;

        if (bulletPrefab != null && firePoint != null)
        {
            float facingX = GetFacingDirection().x;
            Vector3 fpLocalPos = firePoint.localPosition;
            fpLocalPos.x = Mathf.Abs(fpLocalPos.x) * facingX;
            firePoint.localPosition = fpLocalPos;

            Vector2 aimDirection = (targetPlayer.position - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

           
            bullet.transform.right = aimDirection; 
            
            if (offsetAngle != 0)
            {
                bullet.transform.Rotate(0, 0, offsetAngle);
            }

            FlyingBullet script = bullet.GetComponent<FlyingBullet>();
            if (script != null)
            {
                script.Launch();
            }
        }
    }
}