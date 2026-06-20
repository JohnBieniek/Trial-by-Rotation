using System.Collections.Generic;
using UnityEngine;

public class JudgeAudioManager : MonoBehaviour
{
    public static JudgeAudioManager Instance;

    [SerializeField] private AudioClip[] judgeClips;
    [SerializeField] private AudioSource audioSource;

    private readonly List<AudioClip> clipDeck = new();
    private readonly Queue<AudioClip> playQueue = new();

    private bool gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RefillDeck();
    }

    private void Update()
    {
        if (gameOver)
            return;

        if (!audioSource.isPlaying && playQueue.Count > 0)
        {
            AudioClip nextClip = playQueue.Dequeue();
            audioSource.clip = nextClip;
            audioSource.Play();
        }
    }

    public void QueueRandomJudgeClip()
    {
        if (gameOver)
            return;

        if (judgeClips == null || judgeClips.Length == 0)
            return;

        if (clipDeck.Count == 0)
            RefillDeck();

        int index = Random.Range(0, clipDeck.Count);

        AudioClip clip = clipDeck[index];
        clipDeck.RemoveAt(index);

        playQueue.Enqueue(clip);
    }

    private void RefillDeck()
    {
        clipDeck.Clear();

        foreach (AudioClip clip in judgeClips)
        {
            if (clip != null)
                clipDeck.Add(clip);
        }
    }

    public void SetGameOver(bool value)
    {
        gameOver = value;

        if (value)
        {
            playQueue.Clear();
            audioSource.Stop();
        }
    }
}