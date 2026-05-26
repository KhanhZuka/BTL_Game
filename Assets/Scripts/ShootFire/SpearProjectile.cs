using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    [Header("--- Spear Settings ---")]
    public float speed = 3f;
    public int damage = 20; // Sát thương
    public float lifetime = 3f;
    
    [Header("--- Animation Settings ---")]
    // Thời gian chờ animation "Hit" chạy xong trước khi biến mất. 
    public float hitDestroyDelay = 0.5f; 

    private float moveDirection;
    private bool hasHit = false; //  đánh dấu xem lao đã va chạm chưa
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        // Tự động hủy sau một khoảng thời gian nếu bay ra ngoài map
        Destroy(gameObject, lifetime);
    }

    public void Setup(float direction)
    {
        moveDirection = direction;

        // Lật hình ảnh cây giáo theo hướng bay
        Vector3 localScale = transform.localScale;
        if (moveDirection < 0) 
            localScale.x = -Mathf.Abs(localScale.x); 
        else 
            localScale.x = Mathf.Abs(localScale.x);  
            
        transform.localScale = localScale;
    }

    void Update()
    {
        // NẾU ĐÃ TRÚNG ĐÍCH THÌ KHÔNG BAY NỮA (Đứng im để chạy Animation)
        if (hasHit) return; 

        // Bay liên tục về phía trước
        transform.Translate(Vector2.right * moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.isTrigger || hitInfo.CompareTag("Enemy") || hasHit) return; 

        hasHit = true; 


        if (hitInfo.CompareTag("Player"))
        {
            
            PlayerController playerStats = hitInfo.GetComponent<PlayerController>();
            if (playerStats != null) playerStats.ChangeHealth(-damage);
            
        }

        // 1. Chạy Animation nổ/vỡ
        if (anim != null) 
        {
            anim.SetTrigger("Collider"); 
        }

        // 2. Tắt Collider2D ngay lập tức để cây lao không bị va chạm lặp đi lặp lại khi đang chạy Animation
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. Hủy GameObject thay vì hủy lập tức, ta cho nó delay một khoảng thời gian chờ animation chạy xong
        Destroy(gameObject, hitDestroyDelay);
    }
}