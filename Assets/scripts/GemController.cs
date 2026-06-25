using UnityEngine;

public class GemController : MonoBehaviour
{
    [SerializeField] private AudioClip tink1;
    [SerializeField][Range(0f, 1f)] private float tinkVolume = 1f;
    private AudioSource audioSource;
    private float ignoreTinksUntil;
    private float nextTinkTime;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void OnEnable()
    {
        ignoreTinksUntil = Time.time + 1f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameController.hasStarted)
            return;

        if (Time.time < ignoreTinksUntil)
            return;

        if (Time.time < nextTinkTime)
            return;
        nextTinkTime= Time.time+.05f;
  

        audioSource.PlayOneShot(tink1,tinkVolume);
    }
}