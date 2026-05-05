using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform patrolLeft;
    public Transform patrolRight;

    Rigidbody2D rigidbody2d;
    Animator animator;
    SpriteRenderer sprite;

    int direction = 1;
    bool isAlive = true;

    float leftX;
    float rightX;

    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        leftX = Mathf.Min(patrolLeft.position.x, patrolRight.position.x);
        rightX = Mathf.Max(patrolLeft.position.x, patrolRight.position.x);
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        rigidbody2d.linearVelocity = new Vector2(direction * moveSpeed, rigidbody2d.linearVelocity.y);
        animator.SetBool("IsMoving", true);

        if (direction == 1 && transform.position.x >= rightX)
        {
            TurnAround();
        }
        else if (direction == -1 && transform.position.x <= leftX)
        {
            TurnAround();
        }
    }
    void TurnAround()
    {
        direction *= -1;
        sprite.flipX = direction < 0;
    }

    public void Dead()
    {
        if (!isAlive) return;

        isAlive = false;
        rigidbody2d.linearVelocity = Vector2.zero;
        rigidbody2d.simulated = false;

        animator.SetTrigger("Dead");
        Destroy(gameObject, 0.8f);
    }
}