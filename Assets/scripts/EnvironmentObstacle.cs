using UnityEngine;

public class EnvironmentalObstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private float massMultiplier = 1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeedDegreesPerSecond = 180f;

    public float MassMultiplier => massMultiplier;

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeedDegreesPerSecond * Time.deltaTime);
    }

    public float GetSurfaceSpeedAtPoint(Vector2 worldPoint)
    {
        Vector2 center = transform.position;
        float radius = Vector2.Distance(center, worldPoint);

        float angularSpeedRadians =
            rotationSpeedDegreesPerSecond * Mathf.Deg2Rad;

        return Mathf.Abs(angularSpeedRadians * radius);
    }
}