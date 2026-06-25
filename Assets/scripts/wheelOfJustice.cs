using UnityEngine;

public class WheelOfJustice : MonoBehaviour
{

    private float rotationSpeed; // degrees per second

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
        Debug.Log("Wheel speed set to: " + rotationSpeed);
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
    }
    void Update()
    {
        if (!GameController.hasStarted)
        {
            return;
        }
        //Debug.Log("rotationSpeed: " + rotationSpeed);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}