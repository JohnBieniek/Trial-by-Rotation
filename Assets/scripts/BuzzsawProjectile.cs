using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BuzzsawProjectile : MonoBehaviour
{
    [Header("Detonation")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private ParticleSystem detonationParticles;

    [Header("Enemy")]
    [SerializeField] private string enemyTag = "AI";
    [SerializeField] private float stunDuration = .5f;
    private Rigidbody2D rb;
    private Collider2D sawCollider;
    private GameObject owner;
    [SerializeField] private float knockbackAmount;

    [SerializeField] private AudioSource buzzsawAudio;
    [SerializeField] private AudioClip buzzsawLoop;
    public float KnockbackAmount => knockbackAmount;
    [SerializeField] private float speed;
    private bool hasDetonated = false;
    private Vector2 travelDirection;
    public Vector2 TravelDirection => travelDirection;
    [SerializeField] private float spinSpeed = 720f;

    [SerializeField] private GameObject deathExplosionPrefab;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.angularDamping = 0;
        buzzsawAudio = GetComponent<AudioSource>();

        if (buzzsawAudio != null)
        {
            buzzsawAudio.loop = true;
            buzzsawAudio.Play();
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
        if (playerCollider != null && sawCollider!=null) Physics2D.IgnoreCollision(sawCollider, playerCollider);

        Destroy(gameObject, 10f);
    }

    private void Update()
    {

        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDetonated) return;
        if (!GameController.hasStarted  || GameController.Instance.IsGameWonOrLost())
        {
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
            return;
        if (collision.gameObject == owner) return;

        if (collision.gameObject.CompareTag(enemyTag))
        {
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 pushDirection = travelDirection;

                enemyRb.AddForce(pushDirection * knockbackAmount, ForceMode2D.Impulse);//Push in the direction the bullet goes regardless of impact angle
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

        if (buzzsawAudio != null)
        {
            buzzsawAudio.Stop();
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