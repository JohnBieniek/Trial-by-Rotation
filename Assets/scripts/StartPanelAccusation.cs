using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static StartPanelAccusation.Accusation;

public class StartPanelAccusation : MonoBehaviour, IPointerDownHandler
{
    public static StartPanelAccusation Instance;

    [System.Serializable]
    public class Accusation
    {
        [TextArea(2, 4)]
        public string text;

        public AudioClip audioClip;

        [SerializeField]
        public Testimony[] testimonies;
    }
    [System.Serializable]
    public class Testimony
    {
        public AudioClip audioClip;

        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [SerializeField] private TMP_Text accusationText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Accusation[] accusations;
    [SerializeField] private float testimonyVolume = 2f;
 
    [SerializeField] private float startingPlantiffCount = 3;
    public float startingPlantiffCountThisTrial = 3;
    private float plantiffCount = 0;
    private readonly List<int> remainingAccusations = new();
    private List<Testimony> remainingTestimonies;

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
        PickRandomAccusation();
        accusationAudioPlayed = false;

        if (audioUnlocked)
        {
            GameController.Instance.PlayMenuMusic();
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

        if(plantiffCount == 0)
            plantiffCount = 3;
        else plantiffCount += 2;
        startingPlantiffCountThisTrial = plantiffCount;


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
        audioSource.PlayOneShot(currentAccusation.audioClip, 0.8f);

        //Debug.Log("Playing accusation: " + currentAccusation.audioClip.name);
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

        if (remainingTestimonies!=null && remainingTestimonies.Count > 0)
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
        //Debug.Log("Starting testimony loop with " + remainingTestimonies.Count + " testimonies.");

        PlayAccusationAudioOnce();
        yield return new WaitForSeconds(14.0f);

        while (remainingTestimonies.Count > 0)
        {
            int bagIndex = Random.Range(0, remainingTestimonies.Count);
            Testimony testimony = remainingTestimonies[bagIndex];

            AudioClip clip = testimony.audioClip;

            //Debug.Log("Selected testimony: " + (clip != null ? clip.name : "null") + " from bag index: " + bagIndex);

            remainingTestimonies.RemoveAt(bagIndex);

            if (audioSource != null && clip != null)
            {
                JudgeAudioManager.Instance.clearAnnouncements();

                audioSource.PlayOneShot(
                    clip,
                    testimonyVolume * testimony.volume
                );

                //Debug.Log("Playing testimony: " + clip.name + " at volume " + testimony.volume);
            }

            if (remainingTestimonies.Count == 0)
                break;

            yield return new WaitForSeconds(13);
        }

        testimonyCoroutine = null;
    }

    private void BuildTestimonyBag(Accusation accusation)
    {
        if (remainingTestimonies == null)
            remainingTestimonies = new List<Testimony>();
        remainingTestimonies.Clear();

        if (accusation == null || accusation.testimonies == null)
            return;

        foreach (Testimony testimony in accusation.testimonies)
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