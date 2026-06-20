using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
public class StartPanelAccusation : MonoBehaviour
{
    [System.Serializable]
    public class Accusation
    {
        [TextArea(2, 4)]
        public string text;
        public AudioClip audioClip;
    }

    [SerializeField] private TMP_Text accusationText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Accusation[] accusations;
    private readonly List<int> remainingAccusations = new();
    private void OnEnable()
    {
        ShowRandomAccusation();
    }

    public void ShowRandomAccusation()
    {
        if (accusations == null || accusations.Length == 0)
            return;

        int index = GetRandomAccusationIndex();
        Accusation chosen = accusations[index];

        accusationText.text = "You stand accused of "+ chosen.text+"!";

        if (audioSource != null && chosen.audioClip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(accusations[index].audioClip);
            Debug.Log("Playing audio clip: " + accusations[index].audioClip.name);
        }
    }

    private int GetRandomAccusationIndex()
    {
        if (remainingAccusations.Count == 0)
        {
            for (int i = 0; i < accusations.Length; i++)
            {
                remainingAccusations.Add(i);
            }
        }

        int bagIndex = Random.Range(0, remainingAccusations.Count);
        int accusationIndex = remainingAccusations[bagIndex];

        remainingAccusations.RemoveAt(bagIndex);

        return accusationIndex;
    }
}