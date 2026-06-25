using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Friction")]
    [SerializeField] private float frictionModifier = 2f;

    private Rigidbody2D rigidBody;
    private Vector2 inputDirection;
    public float RotationalSpeed => rigidBody.angularVelocity;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 2f;
    [SerializeField] private float spinKnockbackMultiplier = 0.002f;
    [SerializeField] private float maxKnockback = 40f;

    [SerializeField]
    private float rotationSpeed = -90f; // degrees per second
    [SerializeField] private float spinAcceleration = 1500f;
    [SerializeField] private float maxRotationalSpeed = 10800f;

    [SerializeField] private float verticalForceMultiplier = 1.5f;



    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        rigidBody.gravityScale = 0f;
        rigidBody.freezeRotation = false;
    }

    private void Start()
    {
        GameObject wheel = GameObject.FindWithTag("Wheel of Justice");

        if (wheel != null)
        {
            transform.SetParent(wheel.transform, true);
        }
    }

    void Update()
    {
        if (!GameController.hasStarted)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            return;
        }
        if (Keyboard.current.spaceKey.isPressed || Input.GetMouseButton(1))
        {
            float acceleration = spinAcceleration;

            if (rigidBody.angularVelocity < 0f)
                acceleration *= 2f;

            rigidBody.angularVelocity += acceleration * Time.deltaTime;
        }
        else
        {
            float currentSpin = rigidBody.angularVelocity;

            if (Mathf.Abs(currentSpin) > 0f)
            {
                currentSpin -= Mathf.Sign(currentSpin) * 10f * Time.deltaTime;

                if (Mathf.Sign(currentSpin) != Mathf.Sign(rigidBody.angularVelocity))
                    currentSpin = 0f;

                rigidBody.angularVelocity = currentSpin;
            }
        }

        inputDirection = Vector2.zero;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputDirection.x -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputDirection.x += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            inputDirection.y -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            inputDirection.y += 1f;

        inputDirection = inputDirection.normalized;
    }

    private void FixedUpdate()
    {
        ApplyMovementForce();
        ApplyFriction();
        RestrictSpeed();
    }

    private void ApplyMovementForce()
    {
        if (inputDirection == Vector2.zero)
            return;

        Vector2 cameraRight = Camera.main.transform.right;
        Vector2 cameraUp = Camera.main.transform.up;


        Vector2 movementDirection =
         (cameraRight * inputDirection.x) +
         (cameraUp * inputDirection.y * verticalForceMultiplier);

        rigidBody.AddForce(movementDirection.normalized * thrustForce, ForceMode2D.Force);
    }

    private void ApplyFriction()
    {
        if (rigidBody.linearVelocity.sqrMagnitude < 0.001f)
        {
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 frictionForce = -rigidBody.linearVelocity * frictionModifier;
        rigidBody.AddForce(frictionForce, ForceMode2D.Force);
    }

    private void RestrictSpeed()
    {
        if (rigidBody.linearVelocity.magnitude > maxSpeed)
        {
            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("AI"))
            return;

        Rigidbody2D aiRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (aiRb == null)
            return;

        ContactPoint2D contact = collision.GetContact(0);

        Vector2 knockbackDirection =
            ((Vector2)transform.position - contact.point).normalized;

        float aiSpeed = Mathf.Clamp(aiRb.linearVelocity.magnitude, 5f, 30f);
        float playerRps = Mathf.Abs(rigidBody.angularVelocity) / 360f;

        // 0 at 0 RPS, 1 at 25 RPS
        float spinFactor = Mathf.Clamp01(playerRps / 25f);

        // Smooth falloff
        spinFactor = spinFactor * spinFactor;

        float knockbackForce =
     aiSpeed * Mathf.Lerp(1.3f, 0.2f, spinFactor);

        knockbackForce *= knockbackMultiplier;
        knockbackForce = Mathf.Clamp(knockbackForce, 3f, maxKnockback);

        rigidBody.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        //Debug.Log(
        //    $"AI Speed: {aiSpeed:F1}, RPS: {playerRps:F1}, Knockback: {knockbackForce:F1}"
        //);
    }
}