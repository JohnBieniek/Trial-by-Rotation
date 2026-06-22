//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//public class StartPanelAccusation : MonoBehaviour
//{
//    [System.Serializable]
//    public class Accusation
//    {
//        [TextArea(2, 4)]
//        public string text;
//        public AudioClip audioClip;
//        [SerializeField] public AudioClip[] testimonies;
//    }

//    [SerializeField] private TMP_Text accusationText;
//    [SerializeField] private AudioSource audioSource;
//    [SerializeField] private Accusation[] accusations;
//    private readonly List<int> remainingAccusations = new();
//    private readonly List<AudioClip> remainingTestimonies = new();


//    private Coroutine testimonyCoroutine;
//    private void OnEnable()
//    {
//        ShowRandomAccusation();
//    }

//    public void ShowRandomAccusation()
//    {
//        if (accusations == null || accusations.Length == 0)
//            return;

//        int index = GetRandomAccusationIndex();
//        Accusation chosen = accusations[index];

//        accusationText.text = "You stand accused of "+ chosen.text+"!";

//        if (audioSource != null && chosen.audioClip != null)
//        {
//            audioSource.Stop();
//            audioSource.PlayOneShot(accusations[index].audioClip);
//            Debug.Log("Playing audio clip: " + accusations[index].audioClip.name);
//        }
//        BuildTestimonyBag(chosen);
//    }

//    public void StartGame()
//    {
//        StartTestimonyLoop();
//    }

//    public void GameOver()
//    {
//        StopTestimonyLoop();
//    }

//    private void StartTestimonyLoop()
//    {
//        StopTestimonyLoop();

//        if (remainingTestimonies.Count > 0)
//            testimonyCoroutine = StartCoroutine(TestimonyLoop());
//    }

//    private void StopTestimonyLoop()
//    {
//        if (testimonyCoroutine != null)
//        {
//            StopCoroutine(testimonyCoroutine);
//            testimonyCoroutine = null;
//        }
//    }

//    private IEnumerator TestimonyLoop()
//    {
//        // First testimony after 10 seconds
//        yield return new WaitForSeconds(10f);

//        while (remainingTestimonies.Count > 0)
//        {
//            int bagIndex = Random.Range(0, remainingTestimonies.Count);
//            AudioClip clip = remainingTestimonies[bagIndex];

//            remainingTestimonies.RemoveAt(bagIndex);

//            if (audioSource != null && clip != null)
//            {
//                audioSource.PlayOneShot(clip);
//                Debug.Log("Playing testimony: " + clip.name);
//            }

//            if (remainingTestimonies.Count == 0)
//                break;

//            // Subsequent testimonies after 15-25 seconds
//            yield return new WaitForSeconds(Random.Range(20f, 250f));
//        }

//        testimonyCoroutine = null;
//    }

//    private void BuildTestimonyBag(Accusation accusation)
//    {
//        remainingTestimonies.Clear();

//        if (accusation == null || accusation.testimonies == null)
//            return;

//        foreach (AudioClip testimony in accusation.testimonies)
//        {
//            if (testimony != null)
//                remainingTestimonies.Add(testimony);
//        }
//    }

//    private int GetRandomAccusationIndex()
//    {
//        if (remainingAccusations.Count == 0)
//        {
//            for (int i = 0; i < accusations.Length; i++)
//            {
//                remainingAccusations.Add(i);
//            }
//        }

//        int bagIndex = Random.Range(0, remainingAccusations.Count);
//        int accusationIndex = remainingAccusations[bagIndex];

//        remainingAccusations.RemoveAt(bagIndex);

//        return accusationIndex;
//    }
//}