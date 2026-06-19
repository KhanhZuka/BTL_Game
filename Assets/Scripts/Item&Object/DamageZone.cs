using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 8f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Gây damage
        player.ChangeHealth(-damage);

        // Tính hướng đẩy
        Vector2 direction = (other.transform.position - transform.position).normalized;

        // Đẩy player
        Rigidbody2D rigidbody2d = other.GetComponent<Rigidbody2D>();
        if (rigidbody2d != null)
        {
            rigidbody2d.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce);
        }
    }
}