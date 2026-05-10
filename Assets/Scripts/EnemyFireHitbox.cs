using UnityEngine;

public class EnemyFireHitbox : MonoBehaviour
{
    [Header("--- Sát thương lửa ---")]
    public float damagePerSecond = 20f; 
    public string playerTag = "Player2"; // Chú ý: Đặt đúng Tag của người chơi

    // Hàm OnTriggerStay2D sẽ chạy LIÊN TỤC mỗi frame khi Player đứng trong lửa
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            
            /*
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // Vì trừ máu liên tục nên phải nhân với Time.deltaTime
                health.TakeDamage(damagePerSecond * Time.deltaTime);
            }
            */
            
            Debug.Log("Player đang bị đốt cháy! Trừ máu...");
        }
    }
}