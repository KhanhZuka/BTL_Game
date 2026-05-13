using UnityEngine;

public class FlyingFireEnemy : FlyingEnemy
{
    [Header("--- Fire Attack ---")]
    public Transform mouthPoint;
    public GameObject fireBreathPrefab; // Đưa Prefab chứa hệ thống Particle Lửa + Collider Trigger gây dame vào đây
    public float fireDuration = 1.5f;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (fireBreathPrefab != null && mouthPoint != null)
        {
            // Spawn luồng lửa làm "con" (child) của quái để khi quái di chuyển/xoay, lửa xoay theo
            GameObject fire = Instantiate(fireBreathPrefab, mouthPoint.position, mouthPoint.rotation, mouthPoint);
            
            // Tự động xóa luồng lửa sau X giây
            Destroy(fire, fireDuration);
        }
    }
}