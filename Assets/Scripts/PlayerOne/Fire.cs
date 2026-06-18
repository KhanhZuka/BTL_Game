using UnityEngine;

public class Fire : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    private FirePool pool;

    private float lifeTime = 3f;
    private float lifeTimer;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        lifeTimer = lifeTime;
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0)
        {
            ReturnToPool();
        }
    }

    public void SetPool(FirePool firePool)
    {
        pool = firePool;
    }

    public void AddForce(Vector2 direction, float force)
    {
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0f;

        rigidbody2D.AddForce(direction * force);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BatEnemy batEnemy = collision.GetComponent<BatEnemy>();
        BerserkEnemy berserk = collision.GetComponent<BerserkEnemy>();
        WarriorEnemy warrior = collision.GetComponent<WarriorEnemy>();

        if (batEnemy != null)
        {
            batEnemy.ChangeHealthBat(10);
            ReturnToPool();
        }

        if (berserk != null)
        {
            berserk.ChangeHealthBerserk(10);
            ReturnToPool();
        }

        if (warrior != null)
        {
            warrior.ChangeHealthWarrior(10);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0f;

        if (pool != null)
        {
            pool.ReturnFire(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}