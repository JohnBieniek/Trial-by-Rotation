using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BladeController : MonoBehaviour
{
    [SerializeField] private float rotationSpeedDegreesPerSecond = 244f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        float nextRotation = rb.rotation + rotationSpeedDegreesPerSecond * Time.fixedDeltaTime;
        rb.MoveRotation(nextRotation);
    }
}