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

    AudioManager audioManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(audioManager == null )
        {
        audioManager = FindAnyObjectByType<AudioManager>();

        }
    }

    public void Activate(Vector2 fireDirection)
    {
        moveDirection = fireDirection.normalized;
        transform.up = moveDirection; 
        gameObject.SetActive(true);
        audioManager.PlaySFX(audioManager.ShootBullet);
        Invoke(nameof(DeactivateWrap), lifetime);
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
            audioManager.PlaySFX(audioManager.TankExploded);
            Destroy(collision.gameObject);
            Deactivate(true);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            audioManager.PlaySFX(audioManager.ReflectBullet);

            Vector2 normal = collision.GetContact(0).normal;
            moveDirection = Vector2.Reflect(moveDirection, normal);
            transform.up = moveDirection; // Re-align after bounce
        }
        else if (collision.gameObject.CompareTag("AITank"))
        {
            audioManager.PlaySFX(audioManager.TankExploded);

            Destroy(collision.gameObject);
            Deactivate(true);
        }
    }

    private void DeactivateWrap()
    {
        Deactivate(false);
    }
    public void Deactivate(bool hit=false)
    {
        if (!hit)
        {
            audioManager.PlaySFX(audioManager.BulletExploded);
        }

        CancelInvoke();
        gameObject.SetActive(false);
        owner?.ReturnBullet(this);
    }
}