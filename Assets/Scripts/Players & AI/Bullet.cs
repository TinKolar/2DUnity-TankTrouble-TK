using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 4.5f;
    public float lifetime = 10f;

    [HideInInspector] public IBulletOwner owner;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    public void Activate(Vector2 fireDirection)
    {
        moveDirection = fireDirection.normalized;
        transform.up = moveDirection; // Align bullet with movement direction
        gameObject.SetActive(true);
        Invoke(nameof(Deactivate), lifetime);
    }

    private void FixedUpdate()
    {
        // Kinematic movement must be done in FixedUpdate
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            Deactivate();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = collision.GetContact(0).normal;
            moveDirection = Vector2.Reflect(moveDirection, normal);
            transform.up = moveDirection; // Re-align after bounce
        }
        else if (collision.gameObject.CompareTag("AITank"))
        {
            Destroy(collision.gameObject);
            Deactivate();
        }
    }

    public void Deactivate()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        owner?.ReturnBullet(this);
    }
}