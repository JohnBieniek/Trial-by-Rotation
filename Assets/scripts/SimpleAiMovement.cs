using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleAiMovement : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody2D rb;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Death")]
    [SerializeField] private Transform wheelOfJustice;
    [SerializeField] private float wheelRadius = 30f;
    [SerializeField] private float deathBuffer = 1f;
    [SerializeField] private float explosionVolume = 1f;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private GameObject deathExplosionPrefab;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 2f;
    [SerializeField] private float spinKnockbackMultiplier = 0.002f;
    [SerializeField] private float maxKnockback = 40f;
    [SerializeField] private float knockbackStunTime = 0.35f;
    [SerializeField] private AudioClip clang1;

    [Header("Movement")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float actionDelay = 3f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 1.0f;
    private float knockbackUntilTime = 0f;

    private bool isStunned = false;
    private float stunEndsAt = 0f;
    private float nextActionTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        rb.angularVelocity = 400;
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    public void ApplyRepulse(Vector2 direction, float force, float stunDuration)
    {
        knockbackUntilTime = Time.time + stunDuration;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        Debug.Log("Repulsed" + this.name + "with " + force + " force");

    }
    private void CheckIfOffWheel()
    {
        wheelOfJustice = GameObject.Find("Wheel of Justice")?.transform;
        if (wheelOfJustice == null)
            return;

        float distance = Vector2.Distance(
            rb.position,
            wheelOfJustice.position
        );

        if (distance > wheelRadius - deathBuffer)
        {
            if (deathExplosionPrefab != null)
            {
                Instantiate(
                    deathExplosionPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }
            if (audioSource != null && deathSounds.Length > 0)
            {
                AudioClip randomClip = deathSounds[Random.Range(0, deathSounds.Length)];
                AudioSource.PlayClipAtPoint(
                   randomClip,
                   Camera.main.transform.position,
                   explosionVolume
               );
            }

            if (StartPanelAccusation.Instance != null && !StartPanelAccusation.Instance.IsPlaying())
            {
                JudgeAudioManager.Instance.QueueRandomJudgeClip();//Don't step on testimonies but play an announcement when enemies are killed otherwise
            }

            Destroy(gameObject);
        }
    }
    private void FixedUpdate()
    {
        if (!GameController.hasStarted)//Stop AI when game isn't playing
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        CheckIfOffWheel();//See if they are dead before actiong

        if (isStunned)//Wait for stun to end before doing anything else
        {
            if (Time.time >= stunEndsAt)
            {
                isStunned = false;
                nextActionTime = Time.time + actionDelay;
            }
            else
            {
                return;
            }
        }

        if (Time.time < knockbackUntilTime)
            return;

        if (Time.time < nextActionTime)//Small pauses between movements to make it more fair for the player
            return;
        nextActionTime = Time.time + actionDelay;

        rb.angularVelocity = 1500f;//Spin up the AI top quickly so it can hit again after being stunned

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        rb.AddForce(direction * speed, ForceMode2D.Impulse);

        if (rb.linearVelocity.magnitude > maxSpeed) rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))//Get knocked around by the projectiles based on their force and direction
        {
            BuzzsawProjectile projectile =
                collision.gameObject.GetComponent<BuzzsawProjectile>();

            if (projectile != null)
            {
                rb.AddForce(
                    projectile.TravelDirection * projectile.KnockbackAmount,
                    ForceMode2D.Impulse
                );
            }

            //Give the unit a moment before it can accelerate at the player again, varies by weapon
            isStunned = true;
            stunEndsAt = Time.time + stunDuration;
            nextActionTime = stunEndsAt + actionDelay;

            return;//Save processing time
        }

        if (!collision.gameObject.CompareTag("Player"))//Don't get knocked around by other enemies
            return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null)//Should always be true, present to prevent issues during testing
            return;

        if (clang1 != null)//Should always be true, present to prevent issues during testing
        {
            audioSource.PlayOneShot(clang1);
        }

        //Take knockback from the player
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 knockbackDirection = ((Vector2)transform.position - contact.point).normalized;
        float playerSpeed = playerRb.linearVelocity.magnitude;
        float playerSpin = Mathf.Abs(playerRb.angularVelocity);
        float knockbackForce = (playerSpeed + playerSpin * spinKnockbackMultiplier) * knockbackMultiplier;
        knockbackForce = Mathf.Clamp(knockbackForce, 0f, maxKnockback);
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        //Give the unit a moment before it can accelerate at the player again, varies by weapon
        isStunned = true;
        stunEndsAt = Time.time + stunDuration;
        nextActionTime = stunEndsAt + actionDelay;
    }
}