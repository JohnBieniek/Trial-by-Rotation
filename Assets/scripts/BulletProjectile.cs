using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletProjectile : MonoBehaviour
{
    [Header("Detonation")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private ParticleSystem detonationParticles;

    [Header("Enemy")]
    [SerializeField] private string enemyTag = "AI";
    [SerializeField] private float stunDuration = .25f;
    private Rigidbody2D rb;
    [SerializeField]
    private Collider2D bulletCollider;
    private GameObject owner;
    [SerializeField] private float knockbackAmount;

    [SerializeField] private AudioSource bulletAudio;

    public float KnockbackAmount => knockbackAmount;
    [SerializeField] private float speed;
    private bool hasDetonated = false;
    private Vector2 travelDirection;
    public Vector2 TravelDirection => travelDirection;

    [SerializeField] private GameObject deathExplosionPrefab;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.angularDamping = 0;
        bulletAudio = GetComponent<AudioSource>();

        if (bulletAudio != null)
        {
            bulletAudio.Play();
        }
    }
    public void Launch(Vector2 direction)
    {
        travelDirection = direction.normalized;
        rb.linearVelocity = direction.normalized * speed;
        //Debug.Log("Buzzsaw speed: " + rb.linearVelocity.magnitude);
        Collider2D playerCollider = GameObject
            .FindGameObjectWithTag("Player")
            .GetComponent<Collider2D>();

            Physics2D.IgnoreCollision(bulletCollider, playerCollider);
    


       
        Destroy(gameObject, 5f);
    }

    private void Update()
    {

      
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDetonated) return;
        if (collision.gameObject.CompareTag("Player"))
            return;
        if (collision.gameObject == owner) return;
        if (!GameController.hasStarted || GameController.Instance.IsGameWonOrLost())
        {
            Destroy(gameObject);
            return;
        }
        if (collision.gameObject.CompareTag(enemyTag))
        {
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                enemyRb.AddForce(pushDirection * knockbackAmount, ForceMode2D.Impulse);
            }
        }
        if (deathExplosionPrefab != null)
        {
            Instantiate(
                deathExplosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }
        Detonate();
    }

    private void Detonate()
    {
        hasDetonated = true;

        if (bulletAudio != null)
        {
            bulletAudio.Stop();
        }
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, 2f);
        }

        if (detonationParticles != null)
        {
            Instantiate(detonationParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}