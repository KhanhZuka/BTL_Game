using UnityEngine;

public class FlyingBullet : MonoBehaviour
{
    [Header("--- Bullet Settings ---")]
    public float speed = 5f; 
    public int damage = 15; 
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.isTrigger || hitInfo.CompareTag("Enemy")) return; 

        if (hitInfo.CompareTag("Player"))
        {
            PlayerController playerStats = hitInfo.GetComponent<PlayerController>();
            if (playerStats != null) playerStats.ChangeHealth(-damage);
        }
        Destroy(gameObject); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) return;


        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerStats = collision.gameObject.GetComponent<PlayerController>();
            if (playerStats != null) playerStats.ChangeHealth(-damage);
        }
        Destroy(gameObject); 
    }
}