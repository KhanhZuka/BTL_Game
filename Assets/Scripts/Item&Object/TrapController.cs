using UnityEngine;

public class TrapController : MonoBehaviour
{
    [Header("--- Tham chiếu ---")]
    public Transform player; 
    private Animator animator;

    [Header("--- Cài đặt tầm đánh (Chỉ hướng xuống) ---")]
    public float triggerRangeY = 5f; // Chiều dài phạm vi soi xuống dưới
    public float triggerWidthX = 1f; // Độ rộng của bẫy sang 2 bên (để tạo thành một cột chữ nhật)
    
    [Header("--- Cài đặt Hồi chiêu ---")]
    public float attackCooldown = 2f; 
    private float nextAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. Kiểm tra xem Player có đang nằm BÊN DƯỚI bẫy không
        bool isBelow = player.position.y < transform.position.y;

        // 2. Tính khoảng cách từ bẫy chiếu thẳng xuống Player
        float distanceY = transform.position.y - player.position.y;
        
        // 3. Tính khoảng cách chiều ngang (để đảm bảo Player không đứng quá xa sang trái/phải)
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        // Kích hoạt khi: Player ở dưới bẫy + lọt vào độ dài tia Y + lọt vào độ rộng X
        if (isBelow && distanceY <= triggerRangeY && distanceX <= triggerWidthX)
        {
            if (Time.time >= nextAttackTime)
            {
                TriggerAttack();
            }
        }
    }

    void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack"); 
        }
        nextAttackTime = Time.time + attackCooldown;
    }

    // Vẽ khung hiển thị vùng nhận diện của bẫy (Chỉ vẽ thẳng xuống dưới)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        // Dời tâm của khung chữ nhật xuống dưới đúng bằng một nửa chiều dài RangeY
        Vector3 center = transform.position - new Vector3(0, triggerRangeY / 2f, 0);
        
        // Kích thước khung: Rộng = WidthX * 2, Dài = RangeY
        Vector3 size = new Vector3(triggerWidthX * 2, triggerRangeY, 0);
        
        Gizmos.DrawWireCube(center, size);
    }
}