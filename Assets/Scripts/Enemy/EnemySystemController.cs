using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
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

    [Header("--- Combat ---")]
    public float attackCooldown = 2.5f;
    protected float attackTimer = 0f;

    [Header("--- References ---")]
    public Transform player;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected Vector2 startPos;

    protected bool isDead = false;
    protected bool isAlerted = false;

    protected bool isFrozen = false;
    protected SpriteRenderer sprite;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        currentHp = maxHp; 
        startPos = transform.position;
        attackTimer = attackCooldown;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        if (isDead || player == null || isFrozen) return;
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
        currentHp -= amount;
        isAlerted = true; 
        if (currentHp <= 0) Die();
        else anim.Play("Hit");
    }

    public virtual void Die()
    {
        if (currentHp <= 0) isDead = true;
        anim.Play("Dead");
        StopMoving();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    protected void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected void FlipTowards(float targetPositionX)
    {
        float directionX = targetPositionX - transform.position.x;
        Vector3 currentScale = transform.localScale;
        
        if (directionX > 0.1f) currentScale.x = -Mathf.Abs(currentScale.x);
        else if (directionX < -0.1f) currentScale.x = Mathf.Abs(currentScale.x);
        
        transform.localScale = currentScale;
    }

    // --- MỚI: HÀM LẤY HƯỚNG MẶT CỦA QUÁI ---
    protected Vector2 GetFacingDirection()
    {
        // Theo logic lật hình: quái hướng sang phải thì localScale.x âm, trái thì dương
        return transform.localScale.x < 0 ? Vector2.right : Vector2.left;
    }
    public virtual void Freeze(bool value)
    {
        isFrozen = value;
        if (isFrozen)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // đổi màu cho dễ nhìn
            if (sprite != null)
                sprite.color = new Color(0.75f, 0.95f, 1f);
        }
        else
        {
            if (sprite != null)
                sprite.color = Color.white;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPos : (Vector2)transform.position, territoryRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}