using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleAiMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float actionDelay = 3f;
    [SerializeField] private float maxSpeed = 20f;

    private Rigidbody2D rb;
    private float nextActionTime;

    [Header("Death")]
    [SerializeField] private Transform wheelOfJustice;
    [SerializeField] private float wheelRadius = 30f;
    [SerializeField] private float deathBuffer = 1f;
    [SerializeField] private float explosionVolume = 1f;
    [SerializeField] private AudioClip explosionSound;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 2f;
    [SerializeField] private float spinKnockbackMultiplier = 0.002f;
    [SerializeField] private float maxKnockback = 40f;
    [SerializeField] private float knockbackStunTime = 0.35f;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 1.0f;

    private bool isStunned = false;
    private float stunEndsAt = 0f;
    private float stunnedUntil = 0f;
    private void Awake()
    {
        Debug.Log("AI Awake: " + gameObject.name);
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {


        rb.angularVelocity = 400;
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
        }
    }

    private void FixedUpdate()
    {
        CheckIfOffWheel();

        if (isStunned)
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
        if (player == null)
        {
            Debug.LogError("AI has no player target.");
            return;
        }
        rb.angularVelocity = 1500f;
        if (Time.time < stunnedUntil)
        {
            return;
        }
        if (Time.time < nextActionTime)
            return;

        nextActionTime = Time.time + actionDelay;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        rb.AddForce(direction * speed, ForceMode2D.Impulse);


        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        Debug.Log("AI pushed. Velocity: " + rb.linearVelocity);
    }

    private void CheckIfOffWheel()
    {
        if (wheelOfJustice == null)
            return;

        float distance = Vector2.Distance(
            rb.position,
            wheelOfJustice.position
        );

        if (distance > wheelRadius - deathBuffer)
        {
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    explosionSound,
                    Camera.main.transform.position,
                    explosionVolume
                );
            }

            JudgeAudioManager.Instance.QueueRandomJudgeClip();
            
            Destroy(gameObject);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (wheelOfJustice == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wheelOfJustice.position, wheelRadius - deathBuffer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        ContactPoint2D contact = collision.GetContact(0);

        Vector2 knockbackDirection =
            ((Vector2)transform.position - contact.point).normalized;

        float playerSpeed = playerRb.linearVelocity.magnitude;
        float playerSpin = Mathf.Abs(playerRb.angularVelocity);

        float knockbackForce =
            (playerSpeed + playerSpin * spinKnockbackMultiplier)
            * knockbackMultiplier;

        knockbackForce = Mathf.Clamp(knockbackForce, 0f, maxKnockback);
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        nextActionTime = Time.time + actionDelay;
        isStunned = true;
        stunEndsAt = Time.time + stunDuration;
        nextActionTime = stunEndsAt + actionDelay;
    }
}