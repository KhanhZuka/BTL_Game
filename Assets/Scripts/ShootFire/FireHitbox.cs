using UnityEngine;

public class FireHitbox : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    public int damage = 15; 
    
    [Tooltip("Thời gian giữa mỗi lần đốt máu (giây). Giúp Player không bị chết ngay lập tức")]
    public float damageTickRate = 0.5f; 
    
    private float nextDamageTime = 0f;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(float facingDirX)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = (facingDirX > 0);
        }
    }

    private void OnTriggerStay2D(Collider2D hitInfo)
    {
        if (hitInfo.isTrigger || hitInfo.CompareTag("Enemy")) return; 

        if (hitInfo.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            PlayerController playerStats = hitInfo.GetComponent<PlayerController>();
            if (playerStats != null) 
            {
                playerStats.ChangeHealth(-damage);
                
                nextDamageTime = Time.time + damageTickRate;
                
                Debug.Log($"[Khạc Lửa] Thiêu cháy Player! Trừ {damage} máu.");
            }
        }
    }
}