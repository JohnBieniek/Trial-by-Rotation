using UnityEngine;

public class GemController : MonoBehaviour
{
    [SerializeField] private AudioClip tink1;
    [SerializeField][Range(0f, 1f)] private float tinkVolume = 1f;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tink1 != null)
        {
            audioSource.PlayOneShot(tink1, tinkVolume);
        }
    }
}