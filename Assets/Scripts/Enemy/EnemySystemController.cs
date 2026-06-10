using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class EnemySystemController : MonoBehaviour
{
    [Header("--- Base Stats ---")]
    public float maxHp = 100f;
    protected float currentHp;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("--- AI Ranges ---")]
    public float detectionRange = 10f;
    public float attackRange = 4f;
    public float territoryRadius = 5f;

    [Header("--- Combat & Contact Damage ---")]
    public float attackCooldown = 2.5f;
    protected float attackTimer = 0f;
    public int contactDamage = 15;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Vector2 startPos;

    protected SpriteRenderer spriteRenderer;

    protected bool isDead = false;
    protected bool isAlerted = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHp = maxHp; 
        startPos = transform.position;
        attackTimer = attackCooldown;

    }

    protected virtual void Update()
    {
        if (isDead) return;
        attackTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") || stateInfo.IsName("Hit"))
        {
            StopMoving();
            return;
        }

        HandleAI(); 
    }

    protected abstract void HandleAI();

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp = Mathf.Clamp(currentHp - amount, 0, maxHp);

        isAlerted = true; 
        
        

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Bị chém mất {amount} máu! Máu còn: {currentHp}/{maxHp}");
            anim.Play("Hit");
        }
    }

    public virtual void Die()
    {
        isDead = true;
        if (currentHp <= 0) isDead = true;
        anim.Play("Dead");
        StopMoving();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    protected void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected void FlipTowards(float targetPositionX)
    {
        float directionX = targetPositionX - transform.position.x;
        if (Mathf.Abs(directionX) > 0.1f)
        {
            spriteRenderer.flipX = directionX > 0;
        }
    }

    protected Vector2 GetFacingDirection()
    {
        return spriteRenderer.flipX ? Vector2.right : Vector2.left;
    }

    // Dùng cho trường hợp Collider của Quái và Player va chạm vật lý thông thường
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerStats = collision.gameObject.GetComponent<PlayerController>();
            if (playerStats != null)
            {
                playerStats.ChangeHealth(-contactDamage);
                Debug.Log($"[Va Chạm] Quái gây {contactDamage} sát thương va chạm lên Player.");
            }
        }

        if (collision.gameObject.CompareTag("Fireball"))
        {
            TakeDamage(20f); 
            anim.Play("Hit");
        }
    }

    // Dùng cho trường hợp Collider của Quái hoặc Player được tích chọn "Is Trigger"
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerController playerStats = other.GetComponent<PlayerController>();
            if (playerStats != null)
            {
                playerStats.ChangeHealth(-contactDamage);
                Debug.Log($"[Trigger] Quái gây {contactDamage} sát thương va chạm lên Player.");
            }
        }
    }

 

    protected virtual void OnDrawGizmosSelected()
    {
        
    }
}