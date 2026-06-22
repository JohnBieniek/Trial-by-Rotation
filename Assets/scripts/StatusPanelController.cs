using TMPro;
using UnityEngine;

public class StatusPanelController : MonoBehaviour
{
    public static StatusPanelController Instance;
    [SerializeField] private TMP_Text enemyCountText;



    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        int enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;

        enemyCountText.text = "Remaining plantiffs: " + enemiesRemaining;
    }
}