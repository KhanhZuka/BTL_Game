using UnityEngine;

public class FlyingBlazeEnemy : FlyingEnemy
{
    [Header("--- Fire Attack ---")]
    public Transform mouthPoint;
    public GameObject fireBreathPrefab; 
    public float fireDuration = 1.5f;

    [Header("--- Cân chỉnh góc lửa ---")]
    [Tooltip("Nếu luồng lửa bị ngược, hãy nhập 180, 90 hoặc -90")]
    public float offsetAngle = 0f;

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        if (fireBreathPrefab != null && mouthPoint != null && targetPlayer != null)
        {
            // 1. TỰ ĐỘNG ĐẢO VỊ TRÍ MIỆNG (mouthPoint) THEO FLIPX
            float facingX = GetFacingDirection().x;
            Vector3 mouthLocalPos = mouthPoint.localPosition;
            mouthLocalPos.x = Mathf.Abs(mouthLocalPos.x) * facingX;
            mouthPoint.localPosition = mouthLocalPos;

            // 2. TÍNH GÓC QUAY CHỈA THẲNG VÀO PLAYER
            Vector2 aimDirection = (targetPlayer.position - mouthPoint.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            
            // Cộng thêm offsetAngle đề phòng Particle của bạn thiết kế bị ngược hướng
            Quaternion fireRotation = Quaternion.Euler(0, 0, angle + offsetAngle);

            // 3. SPAWN LỬA (Làm con của mouthPoint + Áp dụng góc quay chéo xuống Player)
            GameObject fire = Instantiate(fireBreathPrefab, mouthPoint.position, fireRotation, mouthPoint);
            
            // 4. Tự động tắt lửa sau X giây
            Destroy(fire, fireDuration);
        }
    }
}