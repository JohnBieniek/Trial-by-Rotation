//using UnityEngine;

//public class SimpleAiMovement : MonoBehaviour
//{
//    [SerializeField] private Transform wheelOfJustice;
//    [SerializeField] private float wheelRadius = -1f;
//    [SerializeField] private Transform player;
//    [SerializeField] private float speed = 20f;
//    [SerializeField] private float actionDelay = 1f;
//    [SerializeField] private float maxSpeed = 20f;
    
//    [Header("Friction")]
//    [SerializeField] private float frictionModifier = 2f;

//    [Header("Spin")]
//    [SerializeField] private float spinAcceleration = 300f;
//    [SerializeField] private float maxRotationalSpeed = 720f;
//    [Range(0f, 100f)]
//    [SerializeField] private float chanceToSpinUp = 25f;
//    private Rigidbody2D rigidBody;
//    private float nextActionTime;

//    private void Awake()
//    {
//        Debug.Log("AI Awake: " + gameObject.name);
//        rigidBody = GetComponent<Rigidbody2D>();
//        rigidBody.gravityScale = 0f;
//    }

//    private void Start()
//    {
//        if (player == null)
//        {
//            GameObject found = GameObject.FindGameObjectWithTag("Player");
//            if (found != null)
//                player = found.transform;
//        }
//    }

//    private void Update()
//    {
//        CheckIfOffWheel();
//    }

//    private void MaybeSpinUp()
//    {
//        if (Mathf.Abs(rigidBody.angularVelocity) >= maxRotationalSpeed)
//        {
//            rigidBody.angularVelocity =
//                Mathf.Sign(rigidBody.angularVelocity) * maxRotationalSpeed;
//            return;
//        }

//        if (Random.Range(0f, 100f) < chanceToSpinUp * Time.fixedDeltaTime)
//        {
//            float spinDirection = Random.value < 0.5f ? -1f : 1f;
//            rigidBody.angularVelocity +=
//                spinDirection * spinAcceleration * Time.fixedDeltaTime;
//        }
//    }
//    private void FixedUpdate()
//    {
//        if (player == null)
//        {
//            Debug.LogError("AI has no player target.");
//            return;
//        }

//        if (Time.time < nextActionTime)
//            return;

//        nextActionTime = Time.time + actionDelay;

//        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

//        rigidBody.AddForce(direction * speed, ForceMode2D.Impulse);
//        MaybeSpinUp();

//        if (rigidBodylinearVelocity.magnitude > maxSpeed)
//            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * maxSpeed;

//        Debug.Log("AI pushed. Velocity: " + rigidBody.linearVelocity);
//    }
//    private void ApplyFriction()
//    {
//        if (rigidBody.linearVelocity.sqrMagnitude < 0.001f)
//        {
//            rigidBody.linearVelocity = Vector2.zero;
//            return;
//        }

//        Vector2 frictionForce = -rigidBody.linearVelocity * frictionModifier;
//        rigidBody.AddForce(frictionForce, ForceMode2D.Force);
//    }
//    private void CheckIfOffWheel()
//    {
//        if (wheelOfJustice == null)
//            return;

//        float radius = wheelRadius;

//        if (radius <= 0f)
//        {
//            SpriteRenderer sr = wheelOfJustice.GetComponent<SpriteRenderer>();

//            if (sr != null)
//            {
//                radius = Mathf.Min(
//                    sr.bounds.size.x,
//                    sr.bounds.size.y
//                ) * 0.5f;
//            }
//            else
//            {
//                return;
//            }
//        }

//        float distance =
//            Vector2.Distance(
//                transform.position,
//                wheelOfJustice.position);

//        if (distance > radius)
//        {
//            Destroy(gameObject);
//        }
//    }
//}