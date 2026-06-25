using TMPro;
using UnityEngine;

public class StatusPanelController : MonoBehaviour
{
    public static StatusPanelController Instance;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text spinSpeedText;


    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();

        int rps = Mathf.RoundToInt(
         playerMovement.GetComponent<Rigidbody2D>().angularVelocity / 360f
        );

        spinSpeedText.text = $"Spin Speed: {rps} rps";


        int enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;

        enemyCountText.text = "Remaining plantiffs: " + enemiesRemaining;
    }
}