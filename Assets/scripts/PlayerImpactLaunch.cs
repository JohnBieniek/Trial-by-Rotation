using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerImpactLaunch : MonoBehaviour
{
    [Header("Launch Tuning")]
    [SerializeField] private float launchMultiplier = 1.5f;
    [SerializeField] private float maxLaunchForce = 30f;
    [SerializeField] private float minLaunchForce = 3f;

    [Header("Optional Spin Bonus")]
    [SerializeField] private bool usePlayerSpinBonus = true;
    [SerializeField] private float playerSpinMultiplier = 0.01f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnvironmentalObstacle obstacle =
            collision.gameObject.GetComponent<EnvironmentalObstacle>();

        if (obstacle == null)
        {
            return;
        }

        ContactPoint2D contact = collision.GetContact(0);

        Vector2 launchDirection =
            ((Vector2)transform.position - contact.point).normalized;

        float playerSpeed = rb.linearVelocity.magnitude;
        float obstacleSurfaceSpeed = obstacle.GetSurfaceSpeedAtPoint(contact.point);
        float obstacleMass = obstacle.MassMultiplier;

        float playerSpinBonus = 0f;

        if (usePlayerSpinBonus)
        {
            playerSpinBonus = Mathf.Abs(rb.angularVelocity) * playerSpinMultiplier;
        }

        float launchForce =
            (playerSpeed + obstacleSurfaceSpeed + playerSpinBonus)
            * obstacleMass
            * launchMultiplier;

        launchForce = Mathf.Clamp(launchForce, minLaunchForce, maxLaunchForce);

        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
    }
}