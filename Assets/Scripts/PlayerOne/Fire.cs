using UnityEngine;

public class Fire : MonoBehaviour
{
    Rigidbody2D rigidbody2D;
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.magnitude > 100f)
        {
            Destroy(gameObject);
        }
    }

    public void AddForce(Vector2 direction, float force)
    {
        rigidbody2D.AddForce(direction * force);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
          BatEnemy batEnemy = collision.GetComponent<BatEnemy>();
        BerserkEnemy berserk = collision.GetComponent<BerserkEnemy>();
        if (batEnemy != null) {
            batEnemy.ChangeHealthBat(10);
            Destroy(gameObject);
        }
        if (berserk != null)
        {
            berserk.ChangeHealthBerserk(10);
            Destroy(gameObject);
        }
    }
}
