using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerOne : MonoBehaviour
{
    public static PlayerOne instance;

    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction AttackAction;
    public InputAction FireAction;

    Rigidbody2D rigidbody2D;
    Animator animator;

    Vector2 move;

    float speed = 5.0f;
    float jumpForce = 16.0f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    public bool isGrounded = true;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int damage = 1;

    public int maxHealth = 100;
    private int currentHealth;
    public int health { get { return currentHealth; } }
    public float dieAnimationTime = 1.0f;

    public GameObject FirePrefabs;
    public bool hasKey = false;

    private Vector3 startPosition;
    private bool isDead = false;

    public int soXu = 0;
    public int soQuaiDead = 0;
    public SkillCooldownUI skillUI;
    bool canFire = true;
    float fireCooldown = 5f;

    [Header("Sound")]
    public AudioSource audioSource;
    // Sound_Movement
    public AudioClip footstepSounds;
    public float footstepInterval = 0.35f;
    private float nextFootstepTime = 0f;

    public AudioClip attackSounds;
    // Sound_Jump
    public AudioClip jumpSound;
    public AudioClip fireSound;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        AttackAction.Enable();
        FireAction.Enable();

        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;

        currentHealth = maxHealth;
        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        
    }

    void Update()
    {
        if (isDead) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,groundLayer);

        move = MoveAction.ReadValue<Vector2>();

        if (move.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (move.x > 0)
            transform.localScale = new Vector3(1, 1, 1);

        animator.SetFloat("Speed", Mathf.Abs(move.x));
        if (Mathf.Abs(move.x) > 0.1f && isGrounded)
        {
            if (Time.time >= nextFootstepTime)
            {
                audioSource.PlayOneShot(footstepSounds);
                nextFootstepTime = Time.time + footstepInterval;
            }
        }

        if (JumpAction.WasPressedThisFrame() && isGrounded)
        {
            JumpPlayer();
            audioSource.PlayOneShot(jumpSound);
        }

        if (AttackAction.WasPressedThisFrame() && isGrounded)
        {
            AttackPlayer();
            audioSource.PlayOneShot(attackSounds);
        }

        if (FireAction.WasPressedThisFrame() && isGrounded)
        {
            LaunchFire();
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rigidbody2D.linearVelocity =
            new Vector2(move.x * speed, rigidbody2D.linearVelocity.y);
    }

    private void JumpPlayer()
    {
        rigidbody2D.linearVelocity =
            new Vector2(rigidbody2D.linearVelocity.x, jumpForce);

        isGrounded = false;
    }

    private void AttackPlayer()
    {
        if (!isGrounded) return;

        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            BatEnemy bat = enemy.GetComponent<BatEnemy>();
            BerserkEnemy berserk = enemy.GetComponent<BerserkEnemy>();
            WarriorEnemy warrior = enemy.GetComponent<WarriorEnemy>();

            if (bat != null) bat.ChangeHealthBat(5);
            if (berserk != null) berserk.ChangeHealthBerserk(5);
            if (warrior != null) warrior.ChangeHealthWarrior(5);
        }
    }

    public void LaunchFire()
    {
        if (!isGrounded) return;
        if (!canFire) return;

        canFire = false;

        animator.SetTrigger("AttackFire");
        audioSource.PlayOneShot(fireSound);
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        GameObject fire = Instantiate(
            FirePrefabs,
            rigidbody2D.position + direction * 0.6f - Vector2.up * 0.5f,
            Quaternion.identity
        );

        Fire fireObject = fire.GetComponent<Fire>();
        fireObject.AddForce(direction, 300f);

        skillUI.StartCooldown(fireCooldown);

        StartCoroutine(FireCooldown());
    }

    IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        move = Vector2.zero;
        rigidbody2D.linearVelocity = Vector2.zero;

        animator.SetTrigger("Die");

        yield return new WaitForSeconds(dieAnimationTime);

        HealthUIManager.Instance.LoseLife();

        if (HealthUIManager.Instance.IsGameOver())
        {
            GameOver();
        }
        else
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        currentHealth = maxHealth;
        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        rigidbody2D.position = startPosition;
        rigidbody2D.linearVelocity = Vector2.zero;

        isDead = false;
        isGrounded = true;
        animator.Play("Idle");
    }

    private void GameOver()
    {
        isDead = true;
        move = Vector2.zero;
        rigidbody2D.linearVelocity = Vector2.zero;
        Debug.Log("Game Over");
        GameData.lastCoins = soXu;
        GameData.lastEnemyDead = soQuaiDead;
        Time.timeScale = 1f;
        GameData.lastMap = SceneManager.GetActiveScene().name;
        GameData.backToLosePanel = true;
        SceneManager.LoadScene("UIScene");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            StartCoroutine(DieRoutine());
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}