using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHalo : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Light2D haloLight;

    [SerializeField] private float activationRps = 15f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float fadeSpeed = 5f;

    private void Update()
    {
        float rps = Mathf.Abs(rb.angularVelocity) / 360f;

        float targetIntensity =
            rps >= activationRps ? maxIntensity : 0f;

        haloLight.intensity = Mathf.MoveTowards(
            haloLight.intensity,
            targetIntensity,
            fadeSpeed * Time.deltaTime
        );
    }
}