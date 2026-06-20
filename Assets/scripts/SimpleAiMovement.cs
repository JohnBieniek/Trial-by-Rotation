using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleAiMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float actionDelay = 1f;
    [SerializeField] private float maxSpeed = 20f;

    private Rigidbody2D rb;
    private float nextActionTime;

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
        if (player == null)
        {
            Debug.LogError("AI has no player target.");
            return;
        }
        rb.angularVelocity = 1500f;
        if (Time.time < nextActionTime)
            return;

        nextActionTime = Time.time + actionDelay;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        rb.AddForce(direction * speed, ForceMode2D.Impulse);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        Debug.Log("AI pushed. Velocity: " + rb.linearVelocity);
    }
}