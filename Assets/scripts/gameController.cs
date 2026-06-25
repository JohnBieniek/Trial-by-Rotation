using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private StartPanelAccusation startPanelAccusation;

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

    [Header("Judge")]
    [SerializeField] private AudioClip guiltyClip;
    [SerializeField] private AudioClip innocentClip;
    [SerializeField] private AudioSource audioSource;


    [Header("Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private float musicVolume = .18f;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip gameOverMusicClip;
    [SerializeField] private float gameOverMusicVolume = .18f;
    [SerializeField] private AudioSource gameOverMusicAudioSource;
    [Header("Spinner")]
    [SerializeField] private AudioSource spinnerAudioSource;
    [SerializeField] private float spinnerVolume = .18f;
    [SerializeField] private AudioClip spinnerClip;
    [SerializeField] private TextMeshProUGUI refutedText;
    private int highestPlaintiffsRefuted=0;
    [SerializeField] private GameObject[] layouts;

    private int plaintiffsRefutedThisTrial;
    private readonly Dictionary<Transform, Transform> originalParents = new();

    public static GameController Instance { get; private set; } = new GameController();
    public void LoadRandomLayout()
    {
        ReturnRotatingObjects();

        foreach (GameObject layout in layouts)
            layout.SetActive(false);

        int index = Random.Range(0, layouts.Length);
        GameObject selectedLayout = layouts[index];

        selectedLayout.SetActive(true);

        AttachRotatingObjects(selectedLayout.transform);

        Debug.Log("activated layout: " + selectedLayout.name);
    }

    private void AttachRotatingObjects(Transform selectedLayout)
    {
        foreach (Transform child in selectedLayout.GetComponentsInChildren<Transform>(true))
        {
            if (!child.CompareTag("RotatesWithWheel"))
                continue;

            if (!originalParents.ContainsKey(child))
                originalParents.Add(child, child.parent);

            child.SetParent(wheelOfJustice, true);
        }
    }

    private void ReturnRotatingObjects()
    {
        foreach (var pair in originalParents)
        {
            Transform obj = pair.Key;
            Transform parent = pair.Value;

            if (obj != null && parent != null)
                obj.SetParent(parent, true);
        }

        originalParents.Clear();
    }
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
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();

        float spinSpeed =
            playerMovement.GetComponent<Rigidbody2D>().angularVelocity;
        spinSpeed = Mathf.Abs(spinSpeed);
        if (spinSpeed < 1080) spinSpeed = 0;
        spinnerAudioSource.volume = spinnerVolume* (spinSpeed/200);
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
        musicAudioSource.Pause();
        spinnerAudioSource.Pause();
        if (audioSource != null && innocentClip != null)
        {
            audioSource.PlayOneShot(innocentClip);
        }
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(
    FindObjectsSortMode.None);

        foreach (ParticleSystem particle in particles)
        {
            Destroy(particle.gameObject);
        }
        Time.timeScale = 0f;
        StatusPanelController.Instance.gameObject.SetActive(false);
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }

        
    }

    public void SetPlaintiffsRefutedThisTrial()
    {
        int enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;
        plaintiffsRefutedThisTrial = (int)Mathf.Max(
            0,
            StartPanelAccusation.Instance.startingPlantiffCountThisTrial - enemiesRemaining
        );

        highestPlaintiffsRefuted = Mathf.Max(
            highestPlaintiffsRefuted,
            plaintiffsRefutedThisTrial
        );

        refutedText.text = "Most plaintiffs refuted in a trial: " + highestPlaintiffsRefuted;
    }

    //Called by the start button on the start panel to start every trial, including the first one
    public void StartFirstTrial()
    {
        Debug.Log("StartFirstTrial clicked");
        hasStarted = true;
        isGameOver = false;
        hasWon = false;
        plaintiffsRefutedThisTrial = 0;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        PlayMusic();
        StartSpinnerLoop();
        LoadRandomLayout();
        startPanelAccusation.StartGame();
        if (startPanel != null)
            startPanel.SetActive(false);

        SpawnPlayer();
        EnemySpawner.Instance.SpawnEnemies();
        float maxSpeed = 1 + ((StartPanelAccusation.Instance.GetPlaintiffCount() - 3) / 2f) * 3f;
        Debug.Log("maxSpeed: " + maxSpeed);
        GameObject wheelObject = GameObject.FindWithTag("Wheel of Justice");

        if (wheelObject != null)
        {
            WheelOfJustice wheel = wheelObject.GetComponent<WheelOfJustice>();

            if (wheel != null)
            {
                wheel.ResetRotation();

                float newWheelSpeed = Random.Range(maxSpeed/2, maxSpeed);

                Debug.Log("Setting wheel speed to " + newWheelSpeed);

                wheel.SetRotationSpeed(newWheelSpeed);
            }
        }



        StatusPanelController.Instance.gameObject.SetActive(true);
    }

    private void StartSpinnerLoop()
    {
        if (spinnerAudioSource == null)
            return;
        spinnerAudioSource.clip = spinnerClip;
        spinnerAudioSource.loop = true;
        spinnerAudioSource.volume = spinnerVolume;
        if (spinnerAudioSource.time > 0)
        {
            spinnerAudioSource.UnPause();
        }
        else
        {
            spinnerAudioSource.Play();
        }
    }
    private void PlayMusic()
    {
        if (audioSource == null || musicClip == null)
            return;

        musicAudioSource.clip = musicClip;
        musicAudioSource.loop = true;
        Debug.Log("Playing music clip: " + musicClip.name);
        musicAudioSource.volume = musicVolume;
        if (musicAudioSource.time > 0)
        {
            musicAudioSource.UnPause();
        }
        else
        {
            musicAudioSource.Play();
        }
    }

    private void PlayGameOverMusic()
    {
        if (gameOverMusicAudioSource == null || gameOverMusicClip == null)
            return;

        gameOverMusicAudioSource.clip = gameOverMusicClip;
        gameOverMusicAudioSource.loop = true;
        Debug.Log("Playing music clip: " + gameOverMusicClip.name);
        gameOverMusicAudioSource.volume = gameOverMusicVolume;
        if (gameOverMusicAudioSource.time > 0)
        {
            gameOverMusicAudioSource.UnPause();
        }
        else
        {
            gameOverMusicAudioSource.Play();
        }
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
        gameOverMusicAudioSource.Pause();
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
        spinnerAudioSource.Pause();
        musicAudioSource.Pause();
        if (audioSource != null && guiltyClip != null)
        {
            audioSource.PlayOneShot(guiltyClip);
        }
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(
    FindObjectsSortMode.None);
        PlayGameOverMusic();
        foreach (ParticleSystem particle in particles)
        {
            Destroy(particle.gameObject);
        }
        
        int enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;
        int refutedThisTrial = (int)Mathf.Max(0, StartPanelAccusation.Instance.GetPlaintiffCount() -enemiesRemaining);
        Debug.Log("plantiffs this trial:" + StartPanelAccusation.Instance.GetPlaintiffCount());
        Debug.Log("plaintiffs refuted this trial: " + refutedThisTrial);
        highestPlaintiffsRefuted = Mathf.Max(
            highestPlaintiffsRefuted,
            refutedThisTrial
        );
        highestPlaintiffsRefuted = (int)Mathf.Max(
            highestPlaintiffsRefuted,
            StartPanelAccusation.Instance.GetPlaintiffCount()-3
        );
        SetPlaintiffsRefutedThisTrial();
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
        gameOverMusicAudioSource.Pause();
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