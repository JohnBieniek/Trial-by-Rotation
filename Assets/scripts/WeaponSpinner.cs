using System.Collections;
using UnityEngine;
using static PlayerMovement;

public class WeaponSpinner : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private GameController gameController;

    [Header("Spinner")]
    [SerializeField] private RectTransform wheel;
    [SerializeField] private RectTransform playerWheel;
    [SerializeField] private float minSpinSpeed = 360f;
    [SerializeField] private float maxSpinSpeed = 900f;
    [SerializeField] private float stopDuration = 2f;
    [SerializeField] private float startGameDelay = 2.5f;
    [SerializeField] private float slotOffsetDegrees = 0f;// Allows me to correct for the images base offset to make slot 0 allign properly
    public static WeaponSpinner Instance { get; private set; }
    private float spinSpeed;
    private float playerSpinSpeed;
    private bool isStopping;
    private bool hasSelected;
    [SerializeField] private RectTransform defenseWheel;

    private float defenseSpinSpeed;
    private PlayerMovement playerMovement;

    [Header("Player Models")]
    [SerializeField] private GameObject axisModel;
    [SerializeField] private GameObject maulerModel;
    [SerializeField] private GameObject tridentModel;
    [SerializeField] private GameObject player;

    //Weapon scripts
    private PlayerShotgunShooter shotgunScript;
    private PlayerGunShooter machineGunScript;
    private PlayerBuzzsawShooter buzzsawLauncherScript;

    private void Awake()
    {
        Instance = this;//Singleton
        shotgunScript = player.GetComponent<PlayerShotgunShooter>();
        machineGunScript = player.GetComponent<PlayerGunShooter>();
        buzzsawLauncherScript = player.GetComponent<PlayerBuzzsawShooter>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }
    private void OnEnable()
    {
        ResetSpinner();
    }
    public void ResetSpinner()
    {
        //Debug.Log("Resetting spinner");
        StopAllCoroutines();// Stops spinners

        isStopping = false;//Reset variables needed to stop for the next spin
        hasSelected = false;

        DisableAllWeapons();//Clears the weapon script off the player making it ready to recieve a single script from the spinner

        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);//Start weapon spinner
        if (Random.value < 0.5f) spinSpeed *= -1f;

        playerSpinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);//Start player model spinner
        if (Random.value < 0.5f) playerSpinSpeed *= -1f;

        defenseSpinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);//Start the defnse spinner
        if (Random.value < 0.5f) defenseSpinSpeed *= -1f;
    }

    private void Update()
    {
        if (wheel == null || hasSelected) return;//Only spin when we have a valid wheel and the wheel has not stopped yet. 

        if (!isStopping)
        {
            wheel.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);//Spin the weapon wheel
            playerWheel.Rotate(0f, 0f, playerSpinSpeed * Time.unscaledDeltaTime);//Spin the player model wheel
            defenseWheel.Rotate(0f, 0f, defenseSpinSpeed * Time.unscaledDeltaTime);//Spin the defense wheel
            if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(StopSpinner());//Allow the user to proceed without the mouse
        }
    }

    // Called when the user presses the "Start Trial" button. If the spinner is not already stopping and a selection has not been made, it starts the StopSpinner coroutine to gradually stops the spinners to select a weapon and model.
    public void StartTrialButtonPressed()
    {
        if (!isStopping && !hasSelected)
            StartCoroutine(StopSpinner());
    }

    private IEnumerator StopSpinner()
    {
        isStopping = true;

        float startSpeed = spinSpeed;
        float playerStartSpeed = playerSpinSpeed;
        float defenseStartSpeed = defenseSpinSpeed;

        float elapsed = 0f;

        while (elapsed < stopDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / stopDuration;

            spinSpeed = Mathf.Lerp(startSpeed, 0f, t);
            playerSpinSpeed = Mathf.Lerp(playerStartSpeed, 0f, t);
            defenseSpinSpeed = Mathf.Lerp(defenseStartSpeed, 0f, t);

            wheel.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);
            playerWheel.Rotate(0f, 0f, playerSpinSpeed * Time.unscaledDeltaTime);
            defenseWheel.Rotate(0f, 0f, defenseSpinSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        spinSpeed = 0f;
        playerSpinSpeed = 0f;
        defenseSpinSpeed = 0f;

        SelectWeapon();
        SelectModel();
        SelectDefenseAbility();

        yield return new WaitForSecondsRealtime(startGameDelay);

        gameController.StartFirstTrial();
    }

    private void SelectDefenseAbility()
    {
        int slot = GetSelectedDefenseSlot();

        switch (slot)
        {
            case 0:
            case 3:
                playerMovement.SetDefenseAbility(DefenseAbility.SlowTime);
                Debug.Log("Selected Chrono");
                break;

            case 1:
            case 4:
                playerMovement.SetDefenseAbility(DefenseAbility.Repulse);
                Debug.Log("Selected Repulsor");
                break;

            case 2:
            case 5:
                playerMovement.SetDefenseAbility(DefenseAbility.Teleport);
                Debug.Log("Selected Teleport");
                break;
        }
    }

    private int GetSelectedDefenseSlot()
    {
        float z = defenseWheel.eulerAngles.z;
        float slotSize = 360f / 6f;
        float pointerAngle = 90f;

        float correctedAngle =
            (pointerAngle - z + slotOffsetDegrees + 360f) % 360f;

        int slot = Mathf.FloorToInt(correctedAngle / slotSize);

        Debug.Log("DefenseWheel z: " + z + " corrected: " + correctedAngle + " slot: " + slot);

        return slot;
    }

    //Reveal the selcted model, hide all others
    private void ShowModel(GameObject model)
    {
        axisModel.SetActive(false);
        maulerModel.SetActive(false);
        tridentModel.SetActive(false);

        model.SetActive(true);
    }

    //Allow the player to skip the spinner panel and randomly select a model instead
    public void EnableRandomModel()
    {
        DisableAllModels();

        switch (Random.Range(0, 3))
        {
            case 0:
                ShowModel(axisModel);
                //Debug.Log("Selected Axis");
                break;

            case 1:
                ShowModel(maulerModel);
                //Debug.Log("Selected Mauler");
                break;

            case 2:
                ShowModel(tridentModel);
                //Debug.Log("Selected Trident");
                break;
        }
    }

    //Allow the player to skip the spinner panel and randomly select a weapon instead
    public void EnableRandomWeapon()
    {
        DisableAllWeapons();

        switch (Random.Range(0, 3))
        {
            case 0:
                shotgunScript.enabled = true;
                //Debug.Log("Selected Blaster");
                break;

            case 1:
                machineGunScript.enabled = true;
                //Debug.Log("Selected Gatling Gun");
                break;

            case 2:
                buzzsawLauncherScript.enabled = true;
                //Debug.Log("Selected Buzzsaw Launcher");
                break;
        }
    }

    //Allow the player to skip the spinner panel and randomly select a weapon instead
    public void EnableRandomDefense()
    {
        DisableAllWeapons();

        switch (Random.Range(0, 3))
        {
            case 0:
                playerMovement.SetDefenseAbility(DefenseAbility.SlowTime);
                Debug.Log("Selected Chrono");
                break;

            case 1:
                playerMovement.SetDefenseAbility(DefenseAbility.Repulse);
                Debug.Log("Selected Repulsor");
                break;

            case 2:
                playerMovement.SetDefenseAbility(DefenseAbility.Teleport);
                Debug.Log("Selected Teleport");
                break;
        }
    
    }

    //Look at the postion of the wheel, determine what slot was landed on, and load content based on the selection
    private void SelectModel()
    {
        hasSelected = true;

        DisableAllModels();

        int slot = GetSelectedPlayerSlot();

        switch (slot)
        {
            case 0:
            case 3:
                ShowModel(maulerModel);
                //Debug.Log("Selected Mauler");
                break;

            case 2:
            case 5:
                ShowModel(tridentModel);
                //Debug.Log("Selected trident");
                break;

            case 1:
            case 4:
                ShowModel(axisModel);
                //Debug.Log("Selected axis");
                break;
        }
    }

    //Look at the postion of the wheel, determine what slot was landed on, and load content based on the selection
    private void SelectWeapon()
    {
        hasSelected = true;

        DisableAllWeapons();

        int slot = GetSelectedSlot();

        switch (slot)
        {
            case 0:
            case 3:
                machineGunScript.enabled = true; // Gatling
                Debug.Log("Selected Gatling Gun");
                break;

            case 2:
            case 5:
                buzzsawLauncherScript.enabled = true; // Whirler
                Debug.Log("Selected Buzzsaw Launcher");
                break;

            case 1:
            case 4:
                Debug.Log("Selected Shotgun");
                shotgunScript.enabled = true; // Blaster
                break;
        }
    }
    private int GetSelectedSlot()
    {
        float z = wheel.eulerAngles.z;
        float slotSize = 360f / 6f;

        float pointerAngle = 90f;

        float correctedAngle =
            (pointerAngle - z + slotOffsetDegrees + 360f) % 360f;

        int slot = Mathf.FloorToInt(correctedAngle / slotSize);

        Debug.Log("Wheel z: " + z + " corrected: " + correctedAngle + " slot: " + slot);

        return slot;
    }

    private int GetSelectedPlayerSlot()
    {
        float z = playerWheel.eulerAngles.z;
        float slotSize = 360f / 6f;

        float pointerAngle = 90f;

        float correctedAngle =
            (pointerAngle - z + slotOffsetDegrees + 360f) % 360f;

        int slot = Mathf.FloorToInt(correctedAngle / slotSize);

        Debug.Log("PlayerWheel z: " + z + " corrected: " + correctedAngle + " slot: " + slot);

        return slot;
    }
    
    //Reset and prepare for a new weapon script to be active
    public void DisableAllWeapons()
    {
        if (shotgunScript != null) shotgunScript.enabled = false;
        if (machineGunScript != null) machineGunScript.enabled = false;
        if (buzzsawLauncherScript != null) buzzsawLauncherScript.enabled = false;
    }

    //Reset and prepare for a new model to be active
    public void DisableAllModels()
    {
        axisModel.SetActive(false);
        maulerModel.SetActive(false);
        tridentModel.SetActive(false);
    }
}