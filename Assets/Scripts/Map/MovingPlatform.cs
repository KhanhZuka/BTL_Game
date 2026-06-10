using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("--- Waypoints ---")]
    public Transform[] waypoints; 
    public float speed = 3f;      
    public float waitTime = 1f;   

    private int currentWaypointIndex = 0;
    private float timer;
    private bool isWaiting = false;
    private Rigidbody2D rb;

    private PlayerController playerOnPlatform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Length < 2) return;

        if (isWaiting)
        {
            timer += Time.fixedDeltaTime;
            // Khi đứng chờ, vận tốc truyền cho Player bằng 0
            if (playerOnPlatform != null) playerOnPlatform.platformVelocity = Vector2.zero;

            if (timer >= waitTime)
            {
                isWaiting = false;
                timer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        Vector2 targetPos = waypoints[currentWaypointIndex].position;
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        
        // Tính toán vận tốc hiện tại của bệ đỡ
        Vector2 currentVelocity = (newPos - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Truyền vận tốc cho Player nếu đang đứng trên bệ
        if (playerOnPlatform != null)
        {
            playerOnPlatform.platformVelocity = currentVelocity;
        }

        if (Vector2.Distance(rb.position, targetPos) < 0.05f)
        {
            isWaiting = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Nếu Player dẫm từ trên xuống
            if (collision.contacts[0].normal.y < -0.5f)
            {
                playerOnPlatform = collision.gameObject.GetComponent<PlayerController>();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerOnPlatform != null)
            {
                // QUAN TRỌNG: Trả lại vận tốc bằng 0 cho Player khi nhảy ra khỏi bệ
                playerOnPlatform.platformVelocity = Vector2.zero;
                playerOnPlatform = null;
            }
        }
    }
}