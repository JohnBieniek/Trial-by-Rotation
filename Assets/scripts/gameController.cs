using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform wheelOfJustice;

    [Header("Game Over Rules")]
    [SerializeField] private float maxDistanceFromWheelCenter = -1f;
    [SerializeField] private float wheelEdgeBuffer = 0f;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject startPanel;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraFollowSpeed = 5f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

    [Header("Win")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string enemyTag = "AI";
    [SerializeField] private int enemiesRemaining;
    private bool hasWon = false;
    public static bool hasStarted { get; private set; }

    private bool isGameOver = false;

    [SerializeField] private AudioClip guiltyClip;
    [SerializeField] private AudioClip innocentClip;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {

        hasStarted = false;
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (maxDistanceFromWheelCenter <= 0f)
        {
            maxDistanceFromWheelCenter = GetWheelRadius();
        }
    }

    private void Update()
    {
        if (isGameOver || player == null || wheelOfJustice == null)
        {
            return;
        }

        if (!hasWon)
        {
            CheckForWin();
        }

        float distance = Vector2.Distance(player.position, wheelOfJustice.position);

        if (distance > maxDistanceFromWheelCenter + wheelEdgeBuffer)
        {
            Debug.Log("distance at game over: " + distance);
            GameOver();

        }
    }

    private void CheckForWin()
    {
        enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;
        if (enemiesRemaining == 0)
        {
            JudgeAudioManager.Instance.SetGameOver(true);
            Debug.Log("All enemies defeated! Player wins!");
            WinTrial();
        }
    }

    private void WinTrial()
    {
        hasWon = true;
        if (audioSource != null && innocentClip != null)
        {
            audioSource.PlayOneShot(innocentClip);
        }
        Time.timeScale = 0f;
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }
    }

    public void StartFirstTrial()
    {
        hasStarted = true;
        startPanel.SetActive(false);
    }


    public void StartNewTrial()
    {
        Debug.Log("StartNewTrial clicked");
        hasStarted= true;
        startPanel.SetActive(false);
        if (JudgeAudioManager.Instance != null)
        {
            JudgeAudioManager.Instance.SetGameOver(false);
        }

        Time.timeScale = 1f;

        UnityEngine.SceneManagement.Scene currentScene =
            UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    private float GetWheelRadius()
    {
        SpriteRenderer spriteRenderer = wheelOfJustice.GetComponent<SpriteRenderer>();

        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;

        return Mathf.Min(width, height) / 2f;
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER TRIGGERED");
        JudgeAudioManager.Instance.SetGameOver(true);
        isGameOver = true;

        if (audioSource != null && guiltyClip != null)
        {
            audioSource.PlayOneShot(guiltyClip);
        }

        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        JudgeAudioManager.Instance.SetGameOver(false);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void LateUpdate()
    {
        if (isGameOver || mainCamera == null || player == null)
        {
            return;
        }

        Vector3 targetPosition = player.position + cameraOffset;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );
    }
}