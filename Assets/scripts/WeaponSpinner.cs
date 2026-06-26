using System.Collections;
using UnityEngine;

public class WeaponSpinner : MonoBehaviour
{
    [Header("Spinner")]
    [SerializeField] private RectTransform wheel;
    [SerializeField] private float minSpinSpeed = 360f;
    [SerializeField] private float maxSpinSpeed = 900f;
    [SerializeField] private float stopDuration = 2f;
    [SerializeField] private float startGameDelay = 2.5f;


    [Header("Game")]
    [SerializeField] private GameController gameController;

    private float spinSpeed;
    private bool isStopping;
    private bool hasSelected;
    [SerializeField] private float slotOffsetDegrees = 0f;
    private PlayerShotgunShooter shotgunScript;
    private PlayerGunShooter machineGunScript;
    private PlayerBuzzsawShooter buzzsawLauncherScript;
    [Header("Player")]
    [SerializeField] private GameObject player;
    public static WeaponSpinner Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
        shotgunScript = player.GetComponent<PlayerShotgunShooter>();
        machineGunScript = player.GetComponent<PlayerGunShooter>();
        buzzsawLauncherScript = player.GetComponent<PlayerBuzzsawShooter>();
    }
    private void OnEnable()
    {
        ResetSpinner();
    }
    public void ResetSpinner()
    {
        Debug.Log("Resetting spinner");
        StopAllCoroutines();

        isStopping = false;
        hasSelected = false;

        DisableAllWeapons();

        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);

        if (Random.value < 0.5f)
            spinSpeed *= -1f;
    }

    private void Update()
    {
        if (wheel == null || hasSelected)
            return;

        if (!isStopping)
        {
            wheel.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);

            if (Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(StopSpinner());
        }
    }

    public void StartTrialButtonPressed()
    {
        if (!isStopping && !hasSelected)
            StartCoroutine(StopSpinner());
    }

    private IEnumerator StopSpinner()
    {
        isStopping = true;

        float startSpeed = spinSpeed;
        float elapsed = 0f;

        while (elapsed < stopDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / stopDuration;
            spinSpeed = Mathf.Lerp(startSpeed, 0f, t);

            wheel.Rotate(0f, 0f, spinSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        spinSpeed = 0f;

        SelectWeapon();

        yield return new WaitForSecondsRealtime(startGameDelay);

        gameController.StartFirstTrial();
    }
    public void EnableRandomWeapon()
    {
        DisableAllWeapons();

        switch (Random.Range(0, 3))
        {
            case 0:
                shotgunScript.enabled = true;
                Debug.Log("Selected Blaster");
                break;

            case 1:
                machineGunScript.enabled = true;
                Debug.Log("Selected Gatling Gun");
                break;

            case 2:
                buzzsawLauncherScript.enabled = true;
                Debug.Log("Selected Buzzsaw Launcher");
                break;
        }
    }
    private void SelectWeapon()
    {
        hasSelected = true;

        DisableAllWeapons();

        int slot = GetSelectedSlot();

        // 6 slots total:
        // 0, 3 = shotgun
        // 1, 4 = machine gun
        // 2, 5 = buzzsaw
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
    public void DisableAllWeapons()
    {
        if (shotgunScript != null) shotgunScript.enabled = false;
        if (machineGunScript != null) machineGunScript.enabled = false;
        if (buzzsawLauncherScript != null) buzzsawLauncherScript.enabled = false;
    }
}