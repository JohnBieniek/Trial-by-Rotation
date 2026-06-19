using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = -90f; // degrees per second

    private void Start()
    {
        GameObject wheel = GameObject.FindWithTag("Wheel of Justice");

        if (wheel != null)
        {
            transform.SetParent(wheel.transform, true);
        }
        else
        {
            Debug.LogError("Could not find GameObject named 'Wheel of Justice'");
        }
    }

    void Update()
    {
        Vector2 movement = Vector2.zero;

        if (Keyboard.current.aKey.isPressed)
            movement.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            movement.x += 1f;

        if (Keyboard.current.sKey.isPressed)
            movement.y -= 1f;

        if (Keyboard.current.wKey.isPressed)
            movement.y += 1f;

        movement = movement.normalized;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        transform.position += (Vector3)movement * moveSpeed * Time.deltaTime;
    }
}