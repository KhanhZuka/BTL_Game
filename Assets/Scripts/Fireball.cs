using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public int baseDamage = 2;
    public float lifeTime = 3f;
    public float damageMultiplier = 1f;

    private Vector2 direction;
    private Rigidbody2D rb;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed; 
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        //transform.Translate(direction * speed * Time.deltaTime);
    }
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyPatrol enemy = other.gameObject.GetComponentInParent<EnemyPatrol>();
            if (enemy != null)
            {
                int finalDamage = (int)(baseDamage * damageMultiplier);
                enemy.TakeDamage(finalDamage);

                Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float dir = transform.localScale.x > 0 ? 1f : -1f;
                    rb.AddForce(new Vector2(dir * 5f, 2f), ForceMode2D.Impulse);
                }
            }
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
