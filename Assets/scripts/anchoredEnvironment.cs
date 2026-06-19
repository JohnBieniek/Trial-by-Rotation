using UnityEngine;

public class AnchoredEnvironment : MonoBehaviour
{
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
}