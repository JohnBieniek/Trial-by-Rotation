using UnityEngine;

public class BladeController : MonoBehaviour
{
    [SerializeField] private AudioClip clank1;

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
        if (clank1 != null)
        {
            audioSource.PlayOneShot(clank1, tinkVolume);
        }
    }
}