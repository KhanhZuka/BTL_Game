using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerOne : MonoBehaviour
{
    public static PlayerOne instance;   
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction AttackAction;
    Rigidbody2D rigidbody2D;

    Vector2 move;

    float speed = 5.0f;
    float jumpForce = 16.0f;

    Animator animator;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int damage = 1;

    public int maxHealth = 30;
    private int currentHealth;
    public int health { get { return currentHealth; } }
    public GameObject FirePrefabs;
    public InputAction FireAction;

    public bool hasKey = false;

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

        currentHealth = maxHealth;
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

        // Quay mặt
        if (move.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (move.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        animator.SetFloat("Speed", move.magnitude);

        // Nhảy
        if (JumpAction.WasPressedThisFrame())
        {
            JumpPlayer();
            
        }

        if (AttackAction.WasPressedThisFrame())
        {
            AttackPlayer();
        }

        if (FireAction.WasPressedThisFrame())
        {
            LaunchFire();
        }
    }

    private void AttackPlayer()
    {
        animator.SetTrigger("Attack");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Đấm trúng: " + enemy.name);
            BatEnemy.instance.ChangeHealth(5);
            // enemy.GetComponent<BatEnemy>().TakeDamage(damage);
        }
    }


    private void FixedUpdate()
    {
        rigidbody2D.linearVelocity =
            new Vector2(move.x * speed, rigidbody2D.linearVelocity.y);
    }

    private void JumpPlayer()
    {
        rigidbody2D.linearVelocity =
            new Vector2(rigidbody2D.linearVelocity.x, jumpForce);
    }

    public void LaunchFire()
    {
        animator.SetTrigger("AttackFire");

        Vector2 direction;

        if (transform.localScale.x > 0)
            direction = Vector2.right;
        else
            direction = Vector2.left;

        GameObject fire = Instantiate(FirePrefabs,rigidbody2D.position + direction * 0.6f - Vector2.up * 0.5f, Quaternion.identity);
        Fire fireObject = fire.GetComponent<Fire>();
        fireObject.AddForce(direction, 300f);
    }

    public void ChangeHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth+amount, 0, maxHealth);
        if (currentHealth <= 0) animator.SetTrigger("Die");
        HealthUIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }
}