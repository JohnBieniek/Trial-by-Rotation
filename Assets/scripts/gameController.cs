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

    [SerializeField] private StartPanelAccusation startPanelAccusation;
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

    public bool IsGameWonOrLost()
    {
        return isGameOver || hasWon;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (startPanel != null && startPanel.activeInHierarchy)
            {
                StartFirstTrial();
            }
            else if (winPanel != null && winPanel.activeInHierarchy)
            {
                StartNewTrial();
            }
            else if (gameOverPanel != null && gameOverPanel.activeInHierarchy)
            {
                PlayAgain();
            }
        }

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
        if (startPanel != null && startPanel.activeInHierarchy)
            return;
        if (!hasWon && hasStarted
            && !isGameOver) { 
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
    }

    private void WinTrial()
    {
        hasWon = true;
        hasStarted = false;
        startPanelAccusation.GameOver();//Stop testimony loop and audio
        if (audioSource != null && innocentClip != null)
        {
            audioSource.PlayOneShot(innocentClip);
        }
        Time.timeScale = 0f;
        StatusPanelController.Instance.gameObject.SetActive(false);
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }

    }

    public void StartFirstTrial()
    {
        Debug.Log("StartFirstTrial clicked");
        hasStarted = true;
        isGameOver = false;
        hasWon = false;

        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);



       
        startPanelAccusation.StartGame();
        if (startPanel != null)
            startPanel.SetActive(false);
        //UnityEngine.SceneManagement.Scene currentScene =
        //  UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        //UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
        SpawnPlayer();
        EnemySpawner.Instance.SpawnEnemies();
        StatusPanelController.Instance.gameObject.SetActive(true);
    }

    private void SpawnPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null || wheelOfJustice == null)
            return;

        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * 4f;

        playerObj.transform.position =
            wheelOfJustice.position + (Vector3)offset;

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    //Called by the win button on the win panel to start a new trial
    public void StartNewTrial()
    {
        Debug.Log("StartNewTrial clicked");
        hasStarted= true;
        hasWon = false;
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        if (JudgeAudioManager.Instance != null)
        {
            JudgeAudioManager.Instance.SetGameOver(false);
        }

        Time.timeScale = 1f;
        audioSource.Stop();
        //UnityEngine.SceneManagement.Scene currentScene =
        //    UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        //UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
        startPanelAccusation.ShowNextAccusation();
        //startPanelAccusation.StartGame();
 
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
        startPanelAccusation.GameOver();
        JudgeAudioManager.Instance.SetGameOver(true);
        isGameOver = true;

        if (audioSource != null && guiltyClip != null)
        {
            audioSource.PlayOneShot(guiltyClip);
        }
        SpawnPlayer();
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();
        StatusPanelController.Instance.gameObject.SetActive(false);
        StartPanelAccusation.Instance.SetPlaintiffCount(0f);
        Time.timeScale = 0f;

    }

    public void PlayAgain()
    {
        Time.timeScale = 0f;
        isGameOver = false;
        hasStarted = false;
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        JudgeAudioManager.Instance.SetGameOver(false);
        StartPanelAccusation.Instance.ShowNextAccusation();
        //Scene currentScene = SceneManager.GetActiveScene();
        //SceneManager.LoadScene(currentScene.name);
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