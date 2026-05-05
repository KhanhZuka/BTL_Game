using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player == null) return;

        player.ChangeHealth(-1);

        // Knockback nhẹ 
        Rigidbody2D rigidbody2d = player.GetComponent<Rigidbody2D>();
        if (rigidbody2d != null)
        {
            float knockDir = Mathf.Sign(player.transform.position.x - transform.position.x);
            rigidbody2d.linearVelocity = new Vector2(6f * knockDir, 6f);
        }
    }
}
