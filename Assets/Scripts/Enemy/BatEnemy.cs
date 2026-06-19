using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BatEnemy : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyData;

    public Transform pointA;
    public Transform pointB;

    public Transform player;

    public float speed = 2.0f;
    public float chaseDistance = 5.0f;

    Rigidbody2D rigidbody2D;
    Transform target;
    bool isChasing = false;

    public Image fillHealth;
    public Image frameHealth;

    private int maxHealthBat;
    private int healthBat;

    public static BatEnemy instance;
    public float attackDistance = 1.2f;

    Animator animator;

    public bool isAttacking = false;
    bool canDamage = true;
    bool playerInAttackRange = false;
    PlayerOne playerTarget;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip batSound;

    private float nextSoundTime;
    public float SoundInterval = 3f;

    private int damage = 20;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        target = pointB;

        ApplyEnemyData();

        healthBat = maxHealthBat;
        UpdateHealthBar();
    }

    void ApplyEnemyData()
    {
        if (enemyData == null) return;

        maxHealthBat = enemyData.maxHealth;
        damage = enemyData.damage;
        speed = enemyData.speed;
        chaseDistance = enemyData.chaseDistance;
        attackDistance = enemyData.attackDistance;
        SoundInterval = enemyData.soundInterval;
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseDistance)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (distanceToPlayer < attackDistance)
        {
            PlayBatSoundWhenNear();
            AttackPlayer();
        }
        else if (isChasing)
        {
            PlayBatSoundWhenNear();
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rigidbody2D.linearVelocity = direction * speed;

        Flip(direction.x);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            target = target == pointA ? pointB : pointA;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rigidbody2D.linearVelocity = direction * speed;

        Flip(direction.x);
    }

    void PlayBatSoundWhenNear()
    {
        if (Time.time >= nextSoundTime)
        {
            if (audioSource != null && batSound != null)
            {
                audioSource.PlayOneShot(batSound);
            }

            nextSoundTime = Time.time + SoundInterval;
        }
    }

    void Flip(float directionX)
    {
        if (directionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (directionX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void ChangeHealthBat(int amount)
    {
        animator.SetTrigger("Hurt");

        healthBat -= amount;
        healthBat = Mathf.Clamp(healthBat, 0, maxHealthBat);

        UpdateHealthBar();

        if (healthBat <= 0)
        {
            animator.SetTrigger("Dead");
            PlayerOne.instance.soQuaiDead++;
        }
    }

    void UpdateHealthBar()
    {
        if (fillHealth != null)
        {
            fillHealth.fillAmount = (float)healthBat / maxHealthBat;
        }
    }

    void DestroyEnemyBat()
    {
        Destroy(gameObject);
    }

    void AttackPlayer()
    {
        rigidbody2D.linearVelocity = Vector2.zero;

        if (!isAttacking)
        {
            isAttacking = true;
            canDamage = true;

            animator.SetTrigger("Attack");

            Invoke(nameof(DamagePlayer), 1f);
            Invoke(nameof(ResetAttack), 1f);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
        canDamage = true;
    }

    void DamagePlayer()
    {
        if (playerTarget != null && playerTarget.health > 0 && playerInAttackRange && canDamage)
        {
            playerTarget.ChangeHealth(-damage);
            canDamage = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerOne player = collision.GetComponent<PlayerOne>();

        if (player != null)
        {
            playerInAttackRange = false;
            playerTarget = null;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerOne player = collision.GetComponent<PlayerOne>();

        if (player != null)
        {
            playerTarget = player;
            playerInAttackRange = true;
        }
    }
}