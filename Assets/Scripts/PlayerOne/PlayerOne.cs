using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerOne : MonoBehaviour
{
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

    void Start()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        AttackAction.Enable();

        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
    }

    private void AttackPlayer()
    {
        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

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


 
}