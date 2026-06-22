using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartPanelAccusation : MonoBehaviour, IPointerDownHandler
{
    public static StartPanelAccusation Instance;

    [System.Serializable]
    public class Accusation
    {
        [TextArea(2, 4)]
        public string text;

        public AudioClip audioClip;

        public AudioClip[] testimonies;
    }

    [SerializeField] private TMP_Text accusationText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Accusation[] accusations;
    [SerializeField] private float testimonyVolume = 2f;
 
    [SerializeField] private float startingPlantiffCount = 2;
    private float plantiffCount = 0;
    private readonly List<int> remainingAccusations = new();
    private readonly List<AudioClip> remainingTestimonies = new();

    private Coroutine testimonyCoroutine;
    private Accusation currentAccusation;
    private static bool audioUnlocked;
    private bool accusationAudioPlayed;

    [SerializeField] private TMP_Text plantiffs;


 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        ShowNextAccusation();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        audioUnlocked = true;
        PlayAccusationAudioOnce();
    }
    public void ShowNextAccusation()
    {
        Debug.Log("Remaining accusation count before pick: " + remainingAccusations.Count);
        PickRandomAccusation();
        accusationAudioPlayed = false;

        if (audioUnlocked)
        {
            PlayAccusationAudioOnce();
        }
    }
    public float GetPlaintiffCount()
    {
        return plantiffCount;
    }
    public void SetPlaintiffCount(float count)
    {
        plantiffCount = count;
    }
    private void PickRandomAccusation()
    {
        if (accusations == null || accusations.Length == 0)
            return;

        int index = GetRandomAccusationIndex();
        currentAccusation = accusations[index];
   
        accusationText.text = "You stand accused of " + currentAccusation.text + "!";

     
            plantiffCount += 3;
        


        plantiffs.text = "Number of plantiffs: " + plantiffCount;
        Debug.Log("Starting plantiff count: " + plantiffCount);
        BuildTestimonyBag(currentAccusation);
    }

    public void PlayAccusationAudioOnce()
    {
        if (accusationAudioPlayed)
            return;

        if (audioSource == null || currentAccusation == null || currentAccusation.audioClip == null)
            return;

        accusationAudioPlayed = true;

        audioSource.Stop();
        audioSource.PlayOneShot(currentAccusation.audioClip);

        Debug.Log("Playing accusation: " + currentAccusation.audioClip.name);
    }

    public void StartGame()
    {
        audioUnlocked = true;
        StartTestimonyLoop();
    }

    public void GameOver()
    {
        StopTestimonyLoop();
    }

    private void StartTestimonyLoop()
    {
        if (testimonyCoroutine != null)
        {
            StopCoroutine(testimonyCoroutine);
            testimonyCoroutine = null;
        }

        if (remainingTestimonies.Count > 0)
            testimonyCoroutine = StartCoroutine(TestimonyLoop());
    }

    private void StopTestimonyLoop()
    {
        if (testimonyCoroutine != null)
        {
            StopCoroutine(testimonyCoroutine);
            testimonyCoroutine = null;
        }

        if (audioSource != null)
            audioSource.Stop();
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    private IEnumerator TestimonyLoop()
    {
        Debug.Log("Starting testimony loop with " + remainingTestimonies.Count + " testimonies.");
        PlayAccusationAudioOnce();
        yield return new WaitForSeconds(9.5f);

        while (remainingTestimonies.Count > 0)
        {
            int bagIndex = Random.Range(0, remainingTestimonies.Count);
            AudioClip clip = remainingTestimonies[bagIndex];

            Debug.Log("Selected testimony: " + (clip != null ? clip.name : "null") + " from bag index: " + bagIndex);

            remainingTestimonies.RemoveAt(bagIndex);

            if (audioSource != null && clip != null)
            {
                JudgeAudioManager.Instance.clearAnnouncements();
                audioSource.PlayOneShot(clip, testimonyVolume);
                Debug.Log("Playing testimony: " + clip.name);
            }

            if (remainingTestimonies.Count == 0)
                break;

            yield return new WaitForSeconds(Random.Range(10f, 12f));
        }

        testimonyCoroutine = null;
    }

    private void BuildTestimonyBag(Accusation accusation)
    {
        remainingTestimonies.Clear();

        if (accusation == null || accusation.testimonies == null)
            return;

        foreach (AudioClip testimony in accusation.testimonies)
        {
            if (testimony != null)
                remainingTestimonies.Add(testimony);
        }
    }

    private int GetRandomAccusationIndex()
    {
        if (remainingAccusations.Count == 0)
        {
            for (int i = 0; i < accusations.Length; i++)
                remainingAccusations.Add(i);
        }

        int bagIndex = Random.Range(0, remainingAccusations.Count);
        int accusationIndex = remainingAccusations[bagIndex];

        remainingAccusations.RemoveAt(bagIndex);

        return accusationIndex;
    }
}