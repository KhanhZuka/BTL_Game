using UnityEngine;

public class FlyingBullet : MonoBehaviour
{
    [Header("--- Bullet Settings ---")]
    public float speed = 3f; // Tốc độ đạn 
    public int damage = 15; // Sát thương
    public float lifetime = 3f;

    private Vector2 moveDirection;

    void Start()
    {
        // Tự động hủy viên đạn sau 'lifetime' giây để tránh đầy bộ nhớ
        Destroy(gameObject, lifetime);
    }

    public void Setup(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // Xoay đầu đạn chĩa về hướng bay (Trục X hướng thẳng vào Player)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        // Đạn lao thẳng về phía trước theo hướng đã xoay (Vector3.right = trục X)
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Bỏ qua nếu chạm vào các vùng Trigger ảo khác hoặc chạm trúng Quái (để đạn không tự nổ)
        if (hitInfo.isTrigger || hitInfo.CompareTag("Enemy")) return; 

        // 2. Kiểm tra nếu chạm trúng Player
        if (hitInfo.CompareTag("Player") || hitInfo.CompareTag("Player2"))
        {
            PlayerController playerStats = hitInfo.GetComponent<PlayerController>();
            if (playerStats != null)
            {
                playerStats.ChangeHealth(-damage);
            }
            
            
            Debug.Log("Viên đạn trúng Player! Trừ " + damage + " máu.");
        }

        // 3. Chạm vào tường, đất, hoặc Player xong thì hủy viên đạn
        Destroy(gameObject); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Bỏ qua nếu va chạm với Quái
        if (collision.gameObject.CompareTag("Enemy")) return;

        // Gây sát thương nếu va chạm Player
        if (collision.gameObject.CompareTag("Player2"))
        {
            
            PlayerController playerStats = collision.gameObject.GetComponent<PlayerController>();
            if (playerStats != null) playerStats.ChangeHealth(-damage);
            
        }

        // Chạm vào bất cứ vật lý nào khác cũng vỡ đạn
        Destroy(gameObject); 
    }
}