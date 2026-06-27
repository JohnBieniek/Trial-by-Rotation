using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
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
    [SerializeField] private GameObject spinnerPanel;
    [SerializeField] private StartPanelAccusation startPanelAccusation;
    [SerializeField] private GameObject initialStartButton;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject firingInstructions;//Set firing instructions visible in game for 5 seconds if they have played for 10 seconds without firing
    private Boolean playerClickedOnce = false;
    private float timeSinceMouseClick = 0f;
    private float firingInstructionsVisibleTimer = 0f;
    private bool firingInstructionsShown = false;
    private bool firingInstructionsVisible = false;
    [SerializeField] private GameObject initialInstructions;
    private float firstGameTimer = 0f;//Determine when initialInstructions fade from screen
    private bool firstTrialStarted = false;
    private bool initialInstructionsHidden = false;

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
    [SerializeField] private AudioSource menuMusicAudioSource;
    [SerializeField] private float menuMusicVolume = .11f;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameOverMusicClip;
    [SerializeField] private float gameOverMusicVolume = .18f;
    [SerializeField] private AudioSource gameOverMusicAudioSource;

    [SerializeField] private AudioSource victoryMusicAudioSource;
    [SerializeField] private AudioClip victoryMusicClip;
    [SerializeField] private float victoryMusicVolume = .18f;

    [Header("Spinner")]
    [SerializeField] private AudioSource spinnerAudioSource;
    [SerializeField] private float spinnerVolume = .18f;
    [SerializeField] private AudioClip spinnerClip;
    [SerializeField] private TextMeshProUGUI refutedText;
    private int highestPlaintiffsRefuted=0;
    [SerializeField] private GameObject[] layouts;

    private int plaintiffsRefutedThisTrial;
    private readonly Dictionary<Transform, Transform> originalParents = new();
    private PlayerMovement playerMovement;
    [SerializeField] private float winCheckInterval = 0.5f;
    private float nextWinCheckTime;
    public static GameController Instance { get; private set; }

    [SerializeField] private GameObject defensiveInstructions;

    private bool playerUsedDefenseOnce = false;
    private float timeSinceDefenseUse = 0f;
    private float defensiveInstructionsVisibleTimer = 0f;
    private bool defensiveInstructionsShown = false;
    private bool defensiveInstructionsVisible = false;
    [SerializeField] private float defensiveInstructionDelay = 25f;
    [SerializeField] private float defensiveInstructionDisplayTime = 5f;


    [SerializeField] private GameObject spin; // existing bottom-screen "hold space" instruction

    [Header("Spin Instruction Reminder")]
    [SerializeField] private float spinInstructionDelay = 5f;
    [SerializeField] private float spinInstructionCameraLockTime = 3f;
    [SerializeField] private float spinInstructionReturnTime = 2f;
    [SerializeField] private float spinInstructionPlayerYOffset = -80f;

    private bool playerUsedSpinOnce = false;
    private float timeSinceSpinUse = 0f;
    private bool spinInstructionShown = false;
    private bool spinInstructionMoving = false;

    private RectTransform spinRect;
    private Canvas spinCanvas;
    private Vector2 spinOriginalPosition;
    private bool spinOriginalPositionSaved = false;
    private bool spinOriginalActiveState = true;
    private Coroutine spinInstructionRoutine;

    private float cameraLockedUntil = -1f;
    private Vector3 lockedCameraPosition;

    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;
    private float timeScaleBeforePause = 1f;

    void Awake()
    {
        Instance = this;
        if (firingInstructions != null) firingInstructions.SetActive(false);
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        PlayMenuMusic();
        SetupSpinInstruction();
        if (defensiveInstructions != null)
            defensiveInstructions.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void SetupSpinInstruction()
    {
        if (spin == null)
            return;

        spinRect = spin.GetComponent<RectTransform>();
        spinCanvas = spin.GetComponentInParent<Canvas>();

        if (spinRect != null)
        {
            spinOriginalPosition = spinRect.anchoredPosition;
            spinOriginalPositionSaved = true;
        }

        spinOriginalActiveState = spin.activeSelf;
    }

    private void MoveSpinToDefensiveInstructionPosition()
    {
        if (spin == null || spinRect == null || defensiveInstructions == null)
            return;

        RectTransform defensiveRect = defensiveInstructions.GetComponent<RectTransform>();

        if (defensiveRect == null)
            return;

        // Use world UI position, not anchoredPosition.
        // This works even if anchors/pivots are different.
        spinRect.position = defensiveRect.position;
    }
    private System.Collections.IEnumerator MoveSpinInstructionNearPlayer()
    {
        if (spin == null || spinRect == null)
            yield break;

        spinInstructionMoving = true;
        spinInstructionShown = true;

        spinOriginalActiveState = spin.activeSelf;
        spin.SetActive(true);

        MoveSpinToDefensiveInstructionPosition();

        yield return new WaitForSeconds(spinInstructionCameraLockTime);

        Vector2 startPosition = spinRect.anchoredPosition;
        Vector2 endPosition = spinOriginalPosition;

        float elapsed = 0f;

        while (elapsed < spinInstructionReturnTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / spinInstructionReturnTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            spinRect.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                t
            );

            yield return null;
        }

        spinRect.anchoredPosition = endPosition;
        spin.SetActive(spinOriginalActiveState);

        spinInstructionMoving = false;
        spinInstructionRoutine = null;
    }
    public void LoadRandomLayout()
    {
        ReturnRotatingObjects();

        foreach (GameObject layout in layouts)
            layout.SetActive(false);

        int index = UnityEngine.Random.Range(0, layouts.Length);
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
        if (spinnerPanel != null)
        {
            spinnerPanel.SetActive(false);
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

    public void NotifyDefensiveAbilityUsed()
    {
        playerUsedDefenseOnce = true;
        timeSinceDefenseUse = 0f;

        if (defensiveInstructions != null)
            defensiveInstructions.SetActive(false);

        defensiveInstructionsVisible = false;
    }
    public void TogglePause()
    {
        if (isGameOver || hasWon || !hasStarted)
            return;

        if (pausePanel == null)
            return;

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    private void PauseGame()
    {
        isPaused = true;

        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();

        if (musicAudioSource != null)
            musicAudioSource.Pause();

        if (spinnerAudioSource != null)
            spinnerAudioSource.Pause();
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = timeScaleBeforePause <= 0f ? 1f : timeScaleBeforePause;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (musicAudioSource != null)
            musicAudioSource.UnPause();

        if (spinnerAudioSource != null && hasStarted && !hasWon && !isGameOver)
            spinnerAudioSource.UnPause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        if (isPaused) { return; }
        //Movement instructions shown on your first playthrough
        if (firstTrialStarted && !initialInstructionsHidden)
        {
            firstGameTimer += Time.deltaTime;

            if (firstGameTimer >= 5f)
            {
                initialInstructions.SetActive(false);
                initialInstructionsHidden = true;
            }
        }
        //Firing instructions shown if you don't fire your weapon for 10 seconds
        if (firstTrialStarted && !firingInstructionsShown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                playerClickedOnce = true;
                timeSinceMouseClick = 0f;
            }
            else
            {
                timeSinceMouseClick += Time.deltaTime;
            }

            if (!playerClickedOnce && timeSinceMouseClick >= 10f)
            {
                firingInstructions.SetActive(true);
                firingInstructionsVisible = true;
                firingInstructionsShown = true;
                defensiveInstructionDelay -= 7;
                firingInstructionsVisibleTimer = 0f;
            }
        }
        if (firingInstructionsVisible)
        {
            firingInstructionsVisibleTimer += Time.deltaTime;

            if (firingInstructionsVisibleTimer >= 5f)
            {
                firingInstructions.SetActive(false);
                firingInstructionsVisible = false;
            }
        }
        if (firstTrialStarted && !spinInstructionShown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerUsedSpinOnce = true;
                timeSinceSpinUse = 0f;
            }
            else if (!playerUsedSpinOnce)
            {
                timeSinceSpinUse += Time.deltaTime;
            }

            if (!playerUsedSpinOnce && timeSinceSpinUse >= spinInstructionDelay)
            {
                spinInstructionRoutine = StartCoroutine(MoveSpinInstructionNearPlayer());
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (startPanel != null && startPanel.activeInHierarchy)
            {
                startPanelAccusation.PlayAccusationAudioOnce();

                startSpinnerPanel();
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
        if (firstTrialStarted && !defensiveInstructionsShown)
        {
            if (!playerUsedDefenseOnce)
            {
                timeSinceDefenseUse += Time.deltaTime;
            }

            if (!playerUsedDefenseOnce && timeSinceDefenseUse >= defensiveInstructionDelay)
            {
                defensiveInstructions.SetActive(true);
                defensiveInstructionsVisible = true;
                defensiveInstructionsShown = true;
                defensiveInstructionsVisibleTimer = 0f;
            }
        }

        if (defensiveInstructionsVisible)
        {
            defensiveInstructionsVisibleTimer += Time.deltaTime;

            if (defensiveInstructionsVisibleTimer >= defensiveInstructionDisplayTime)
            {
                defensiveInstructions.SetActive(false);
                defensiveInstructionsVisible = false;
            }
        }

        if (isGameOver || player == null || wheelOfJustice == null)
        {
            return;
        }

        if (hasStarted && !hasWon && !isGameOver)
        {
            if(Time.time >= nextWinCheckTime){ 
                nextWinCheckTime = Time.time + winCheckInterval;
                CheckForWin();
            }
            float distance = Vector2.Distance(player.position, wheelOfJustice.position);

            if (distance > maxDistanceFromWheelCenter + wheelEdgeBuffer)
            {
                //Debug.Log("distance at game over: " + distance);
                if (!hasStarted || hasWon || isGameOver)
                    return;
                GameOver();
            }

            float spinSpeed = playerMovement.GetComponent<Rigidbody2D>().angularVelocity;
            spinSpeed = Mathf.Abs(spinSpeed);
            if (spinSpeed < 1080) spinSpeed = 0;
            spinnerAudioSource.volume = spinnerVolume * (spinSpeed / 200);
        }
    }

    private void CheckForWin()
    {
        if (startPanel != null && startPanel.activeInHierarchy)
            return;
        if (spinnerPanel != null && spinnerPanel.activeInHierarchy)
            return;

        if (!hasWon && hasStarted
            && !isGameOver) { 
            enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
                FindObjectsSortMode.None
            ).Length;
            

            if (enemiesRemaining == 0)
            {
                JudgeAudioManager.Instance.SetGameOver(true);
                //Debug.Log("All enemies defeated! Player wins!");
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
        PlayVictoryMusic();
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(
     FindObjectsSortMode.None);

        foreach (ParticleSystem particle in particles)
        {
            if (particle.transform.IsChildOf(player) || particle.transform.tag == "Environment")
                continue;

            Destroy(particle.gameObject);
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerMovement>().ResetAbilities();

        Time.timeScale = 0f;
        StatusPanelController.Instance.gameObject.SetActive(false);
        DestroyProjectiles();
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


    public void startSpinnerPanel()
    {
        //Allow players to skip the spinner panel after they've seen it once
        initialStartButton.SetActive(false);
        startButton.SetActive(true);
        skipButton.SetActive(true);

        if (startPanel != null)
            startPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        startPanelAccusation.PlayAccusationAudioOnce();
        if (spinnerPanel != null)
        {
            spinnerPanel.SetActive(true);

            WeaponSpinner spinner = spinnerPanel.GetComponent<WeaponSpinner>();
            if (spinner != null)
                spinner.ResetSpinner();
        }
    }

    //Clear old bullets before the start of a new round
    private void DestroyProjectiles()
    {
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile"))
        {
            Destroy(projectile);
        }
    }

    //Allow the user to skip the spinner panel after round 1
    public void SkipToTrial()
    {
        WeaponSpinner.Instance.EnableRandomDefense();
        WeaponSpinner.Instance.EnableRandomWeapon();
        WeaponSpinner.Instance.EnableRandomModel();

        StartFirstTrial();
    }

    //Called by the start button on the start panel to start every trial, including the first one
    public void StartFirstTrial()
    {
        //Debug.Log("StartFirstTrial clicked");
        if (!firstTrialStarted) firstTrialStarted = true;
        hasStarted = true;
        isGameOver = false;
        hasWon = false;
        plaintiffsRefutedThisTrial = 0;

        menuMusicAudioSource.Pause();

        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (spinnerPanel != null)
            spinnerPanel.SetActive(false);

        PlayMusic();
        StartSpinnerLoop();
        LoadRandomLayout();
        startPanelAccusation.StartGame();
        if (startPanel != null)
            startPanel.SetActive(false);

        SpawnPlayer();
        EnemySpawner.Instance.SpawnEnemies();
        float maxSpeed = 4 + ((StartPanelAccusation.Instance.GetPlaintiffCount() - 3) / 2f) * 3f;
        //Debug.Log("maxSpeed: " + maxSpeed);
        GameObject wheelObject = GameObject.FindWithTag("Wheel of Justice");

        if (wheelObject != null)
        {
            WheelOfJustice wheel = wheelObject.GetComponent<WheelOfJustice>();

            if (wheel != null)
            {
                wheel.ResetRotation();

                float newWheelSpeed = UnityEngine.Random.Range(maxSpeed/2, maxSpeed);

                //Debug.Log("Setting wheel speed to " + newWheelSpeed);

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

    //Plays and loops the in game music
    private void PlayMusic()
    {
        if (audioSource == null || musicClip == null)
            return;

        musicAudioSource.clip = musicClip;
        musicAudioSource.loop = true;
        //Debug.Log("Playing music clip: " + musicClip.name);
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

    public void PlayMenuMusic()
    {
        if (menuMusicAudioSource == null || menuMusicClip == null)
            return;

        menuMusicAudioSource.clip = menuMusicClip;
        menuMusicAudioSource.loop = true;
        //Debug.Log("Playing music clip: " + menuMusicClip.name);
        menuMusicAudioSource.volume = menuMusicVolume;
        if (menuMusicAudioSource.time > 0)
        {
            menuMusicAudioSource.UnPause();
        }
        else
        {
            menuMusicAudioSource.Play();
        }
    }

    private void PlayVictoryMusic()
    {
        if (victoryMusicAudioSource == null || victoryMusicClip == null)
            return;

        victoryMusicAudioSource.clip = victoryMusicClip;
        victoryMusicAudioSource.loop = true;
        //Debug.Log("Playing music clip: " + victoryMusicClip.name);
        victoryMusicAudioSource.volume = victoryMusicVolume;
        if (victoryMusicAudioSource.time > 0)
        {
            victoryMusicAudioSource.UnPause();
        }
        else
        {
            victoryMusicAudioSource.Play();
        }
    }

    private void PlayGameOverMusic()
    {
        if (gameOverMusicAudioSource == null || gameOverMusicClip == null)
            return;

        gameOverMusicAudioSource.clip = gameOverMusicClip;
        gameOverMusicAudioSource.loop = true;
        //Debug.Log("Playing music clip: " + gameOverMusicClip.name);
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
        Transform wheelCenter = GameObject.FindWithTag("Wheel of Justice").transform;
        if (playerObj == null || wheelOfJustice == null)
            return;

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * 4f;

        playerObj.transform.position =
            wheelOfJustice.position + (Vector3)(offset);

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    //Called by the win button on the win panel to go to the starting menu
    public void StartNewTrial()
    {
        //Debug.Log("StartNewTrial clicked");
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
        PlayMenuMusic();
        gameOverMusicAudioSource.Pause();
        victoryMusicAudioSource.Pause();
        audioSource.Stop();
        startPanelAccusation.ShowNextAccusation();
    }

    public bool IsMenuOpen()
    {
        return (startPanel != null && startPanel.activeInHierarchy)
            || (spinnerPanel != null && spinnerPanel.activeInHierarchy);
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
        //Debug.Log("GAME OVER TRIGGERED");
        startPanelAccusation.GameOver();
        JudgeAudioManager.Instance.SetGameOver(true);
        isGameOver = true;
        
        spinnerAudioSource.Pause();
        musicAudioSource.Pause();
        
        if (audioSource != null && guiltyClip != null)
        {
            audioSource.PlayOneShot(guiltyClip);
        }
        PlayGameOverMusic();

        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (ParticleSystem particle in particles)
        {
            if (particle.transform.IsChildOf(player) || particle.transform.tag=="Environment")
                continue;

            Destroy(particle.gameObject);
        }
        DestroyProjectiles();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerMovement>().ResetAbilities();
        int enemiesRemaining = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        ).Length;
        int refutedThisTrial = (int)Mathf.Max(0, StartPanelAccusation.Instance.GetPlaintiffCount() -enemiesRemaining);
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

    //Called by the Accept summons button on the game over screen to go to the manin menu to start a new trial
    public void PlayAgain()
    {
        Time.timeScale = 0f;
        PlayMenuMusic();
        isGameOver = false;
        hasStarted = false;
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        JudgeAudioManager.Instance.SetGameOver(false);
        victoryMusicAudioSource.Pause();
        gameOverMusicAudioSource.Pause();
        StartPanelAccusation.Instance.ShowNextAccusation();
    }

    //Let the camera chase you, unless we are displaying one time instructional text on screen
    private void LateUpdate()
    {
        if (isGameOver || mainCamera == null || player == null)
        {
            return;
        }
        Vector3 targetPosition = player.position + cameraOffset;
        float followSpeed = cameraFollowSpeed;
        if (firstGameTimer <= 5f || firingInstructionsVisible || defensiveInstructionsVisible || spinInstructionMoving) {
            followSpeed = 15f; // or 20-30 if you want it faster
        }
      
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
            );
        
    }
}