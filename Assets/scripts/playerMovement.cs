using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private Vector2 inputDirection;

    [Header("Movement")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField]private float rotationSpeed = -90f; // degrees per second
    [SerializeField] private float spinAcceleration = 1500f;
    [SerializeField] private float maxRotationalSpeed = 10800f;
    [SerializeField] private float verticalForceMultiplier = 1.5f;
    [SerializeField] private AudioSource defenseAudioSource;
    [Header("Teleport")]
    [SerializeField] public bool teleporterActive = false;
    [SerializeField] private float teleportCooldown = 5f;
    [SerializeField] private GameObject teleportIcon;
    [SerializeField] private ParticleSystem teleportStartParticles;
    [SerializeField] private ParticleSystem teleportEndParticles;
    [SerializeField] public AudioClip teleportAudioClip;
    private float nextTeleportTime = 0f;
    [SerializeField] private float teleportIconRadius = 1.5f;
    [SerializeField] private float teleportIconRotationSpeed = 45f; // degrees per second
    private float teleportIconAngle;
    [SerializeField] private float teleportParticleDestroyDelay = 2f;
    [SerializeField] private float defenseAudioVolume = .9f; 
    [Header("Repulse Ability")]
    [SerializeField] public bool repulsorActive = false;
    [SerializeField] private GameObject repulsorIcon;
    [SerializeField] private GameObject repulsorBlast;
    [SerializeField] private float repulsorCooldown = 5f;
    [SerializeField] private float repulsorRadius = 5f;
    [SerializeField] private float repulsorForce = 25f;
    [SerializeField] private float repulsorStunDuration = 0.35f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float repulsorIconRadius = 1.5f;
    [SerializeField] private float repulsorIconRotationSpeed = 45f; // degrees per second
    [SerializeField] private float repulsorBlastScaleMultiplier = 5f;
    private float repulsorIconAngle;
    [SerializeField] private float blastGrowTime = 0.05f;
    [SerializeField] public AudioClip repulsorAudioClip;
    private Coroutine blastRoutine;
    private float nextRepulseTime = 0f;

    [Header("Slow Time Ability")]
    [SerializeField] private Boolean chronoshiftActive = false;
    private bool chronoshifting = false;
    [SerializeField] private GameObject slowTimeIcon;
    [SerializeField] private float slowTimeCooldown = 10f;
    [SerializeField] private float slowTimeDuration = 3f;
    [SerializeField] private float slowTimeScale = 0.5f;

    [SerializeField] private float slowTimeIconRadius = 1.75f;
    [SerializeField] private float slowTimeIconOrbitSpeed = 45f;
    [SerializeField] private Transform slowTimeMinuteHand;
    [SerializeField] private float slowTimeMinuteHandSpeed = 12f;
    [SerializeField] public AudioClip chronoStartAudioClip;
    [SerializeField] public AudioClip chronoEndAudioClip;

    private float nextSlowTimeUseTime = 0f;
    private float slowTimeEndTime = 0f;
    private float slowTimeIconAngle = 0f;
    public static float PlayerTimeCompensation { get; private set; } = 1f;
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float chronoshiftMusicPitch = 0.5f;

    private float normalMusicPitch = 1f;

    [Header("Friction")]
    [SerializeField] private float frictionModifier = 2f;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 2f;
    [SerializeField] private float spinKnockbackMultiplier = 0.002f;
    [SerializeField] private float maxKnockback = 40f;

    [Header("Paricles")]
    [SerializeField] private ParticleSystem speedParticles;
    [SerializeField] private float particlesStartRps = 20f;
    [SerializeField] private GameObject aura;
    [Header("Auto Unstuck")]
    [SerializeField] private float stuckTimeBeforeUnstick = 2f;
    [SerializeField] private float unstuckDistance = 1.5f;
    [SerializeField] private float unstuckForce = 12f;
    [SerializeField] private LayerMask unstuckCollisionLayers;
    private Collider2D stuckCollider;

    private float stuckTimer;
    [SerializeField] private ParticleSystem[] fireParticles;

    private bool isActive;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        rigidBody.gravityScale = 0f;
        rigidBody.freezeRotation = false;
        SetFireActive(false);
        repulsorBlast.SetActive(false);

        if (musicSource != null)
            normalMusicPitch = musicSource.pitch;

        foreach(ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
{
            if (ps.name == "SpeedParticles")
            {
                speedParticles = ps;
                break;
            }
        }

        if (speedParticles == null)
            Debug.LogError("No speed particle system found on " + gameObject.name);
        else
            Debug.Log("Found speed particles: " + speedParticles.name);
    }

    private void Start()
    {
        GameObject wheel = GameObject.FindWithTag("Wheel of Justice");

        if (wheel != null)
        {
            transform.SetParent(wheel.transform, true);
        }
    }
    public void SetFireActive(bool active)
    {
        if (isActive == active)
            return;

        isActive = active;

        foreach (ParticleSystem ps in fireParticles)
        {
            if (ps == null)
                continue;

            if (active)
            {
                ps.gameObject.SetActive(true);
                ps.Play(true);
            }
            else
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }
    }
    private void PlayDefenseAudio(AudioClip clip)
    {
        Debug.Log("Playing " + clip.name);
       defenseAudioSource.PlayOneShot(clip, defenseAudioVolume);
    }

    private IEnumerator PlayRepulsorBlast()
    {
        repulsorBlast.SetActive(true);

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * repulsorRadius * 2f * repulsorBlastScaleMultiplier;
        
        repulsorBlast.transform.localScale = startScale;

        float elapsed = 0f;

        while (elapsed < blastGrowTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / blastGrowTime;
            repulsorBlast.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            repulsorBlast.transform.position = transform.position;
            yield return null;
        }

        repulsorBlast.transform.localScale = endScale;
        repulsorBlast.SetActive(false);
    }

    void Update()
    {
        if (!GameController.hasStarted)//Stop the player when the game is not started
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            return;
        }

        if (Keyboard.current.spaceKey.isPressed)//Spin the player when the space key  pressed
        {
            float acceleration = spinAcceleration * PlayerTimeCompensation;

            if (rigidBody.angularVelocity < 0f)
                acceleration *= 2f;

            rigidBody.angularVelocity += acceleration * Time.deltaTime;
        }
        else
        {
            float currentSpin = rigidBody.angularVelocity;

            if (Mathf.Abs(currentSpin) > 0f)
            {
                currentSpin -= Mathf.Sign(currentSpin) * 10f * PlayerTimeCompensation * Time.deltaTime;

                if (Mathf.Sign(currentSpin) != Mathf.Sign(rigidBody.angularVelocity))
                    currentSpin = 0f;

                rigidBody.angularVelocity = currentSpin;
            }
        }
        ClampRotationSpeed();
        if (teleporterActive && Time.time >= nextTeleportTime)
        {
            teleportIcon.SetActive(true);

            teleportIconAngle += teleportIconRotationSpeed * Time.deltaTime;

            float radians = teleportIconAngle * Mathf.Deg2Rad;

            teleportIcon.transform.position =
                transform.position +
                new Vector3(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians),
                    0f
                ) * teleportIconRadius;
            teleportIcon.transform.Rotate(0f, 0f, 180f * Time.deltaTime);
            if (Input.GetMouseButtonDown(1))
            {
                TeleportToCursor();
                nextTeleportTime = Time.time + teleportCooldown;
            }
        }
        else
        {
            teleportIcon.SetActive(false);
        }
        HandleRepulseAbility();
        HandleSlowTimeAbility();

        inputDirection = Vector2.zero;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputDirection.x -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputDirection.x += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            inputDirection.y -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            inputDirection.y += 1f;

        inputDirection = inputDirection.normalized;

        UpdateSpeedParticles();

    }

    private void HandleSlowTimeAbility()
    {
        if (!chronoshiftActive)
        {
            if (slowTimeIcon != null)
                slowTimeIcon.SetActive(false);

            return;
        }

        if (chronoshifting)
        {
            if (slowTimeIcon != null)
                slowTimeIcon.SetActive(false);

            if (Time.unscaledTime >= slowTimeEndTime)
                EndSlowTime();

            return;
        }

        bool cooldownReady = Time.unscaledTime >= nextSlowTimeUseTime;

        if (slowTimeIcon != null)
        {
            slowTimeIcon.SetActive(cooldownReady);

            if (cooldownReady)
            {
                OrbitSlowTimeIcon();

                if (slowTimeMinuteHand != null)
                {
                    slowTimeMinuteHand.Rotate(
                        0f,
                        0f,
                        -slowTimeMinuteHandSpeed * Time.unscaledDeltaTime
                    );
                }
            }
        }

        if (cooldownReady && Input.GetMouseButtonDown(1))
        {
            StartSlowTime();
        }
    }

    private void StartSlowTime()
    {
        GameController.Instance.NotifyDefensiveAbilityUsed();
        chronoshifting = true;
        PlayDefenseAudio(chronoStartAudioClip);
        slowTimeIcon.SetActive(false);
        if (musicSource != null)
            musicSource.pitch = chronoshiftMusicPitch;
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        PlayerTimeCompensation = 1f / slowTimeScale;

        slowTimeEndTime = Time.unscaledTime + slowTimeDuration;
        nextSlowTimeUseTime = Time.unscaledTime + slowTimeCooldown;
    }

    private void EndSlowTime()
    {
        PlayDefenseAudio(chronoEndAudioClip);
        chronoshifting = false;
        if (musicSource != null)
            musicSource.pitch = normalMusicPitch;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        PlayerTimeCompensation = 1f;
    }

    private void OrbitSlowTimeIcon()
    {
        slowTimeIconAngle += slowTimeIconOrbitSpeed * Time.unscaledDeltaTime;

        float radians = slowTimeIconAngle * Mathf.Deg2Rad;

        slowTimeIcon.transform.position =
            transform.position +
            new Vector3(
                Mathf.Cos(radians),
                Mathf.Sin(radians),
                0f
            ) * slowTimeIconRadius;

        slowTimeIcon.transform.rotation = Quaternion.identity;
    }

    private void HandleRepulseAbility()
    {
        bool repulseReady = Time.time >= nextRepulseTime;

        if (repulsorIcon != null)
        {
            if (repulsorActive)
            {
                repulsorIcon.SetActive(repulseReady);

                if (repulseReady)
                {
                    repulsorIconAngle += repulsorIconRotationSpeed * Time.deltaTime;

                    float radians = repulsorIconAngle * Mathf.Deg2Rad;

                    repulsorIcon.transform.position =
                        transform.position +
                        new Vector3(
                            Mathf.Cos(radians),
                            Mathf.Sin(radians),
                            0f
                        ) * repulsorIconRadius;
                    repulsorIcon.transform.Rotate(0f, 0f, 180f * Time.deltaTime);

                    if (Input.GetMouseButtonDown(1))
                    {
                        GameController.Instance.NotifyDefensiveAbilityUsed();
                        RepulseEnemies();
                        if (blastRoutine != null)
                            StopCoroutine(blastRoutine);

                        blastRoutine = StartCoroutine(PlayRepulsorBlast());
                        nextRepulseTime = Time.time + repulsorCooldown;
                    }
                }
            }
            else
            {
                repulsorIcon.SetActive(false);
            }
        } 
    }

    private void RepulseEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            repulsorRadius,
            enemyLayer
        );
        PlayDefenseAudio(repulsorAudioClip);
        foreach (Collider2D hit in hits)
        {
            Rigidbody2D enemyRb = hit.attachedRigidbody;

            if (enemyRb == null)
                continue;
            Vector2 direction = enemyRb.position - (Vector2)transform.position;

            SimpleAiMovement ai = enemyRb.GetComponent<SimpleAiMovement>();

            if (ai != null)
            {
                ai.ApplyRepulse(direction, repulsorForce, repulsorStunDuration);
            }
          
         }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, repulsorRadius);
    }
    [SerializeField] private int teleportParticleBurstCount = 30;
    [SerializeField] private int teleportParticleSortingOrder = 1000;
    //[SerializeField] private float teleportParticleDestroyDelay = 2f;

    private void TeleportToCursor()
    {
        GameController.Instance.NotifyDefensiveAbilityUsed();
        Vector3 start = transform.position;
        PlayDefenseAudio(teleportAudioClip);
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = transform.position.z;

        SpawnTeleportBurst(teleportStartParticles, start);

        transform.position = mouse;
        rigidBody.linearVelocity = Vector2.zero;

        SpawnTeleportBurst(teleportEndParticles, mouse);

        nextTeleportTime = Time.time + teleportCooldown;
    }

    private void SpawnTeleportBurst(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        GameObject go = Instantiate(prefab.gameObject, position, Quaternion.identity);
        go.SetActive(true);

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();

        if (renderer != null)
            renderer.sortingOrder = teleportParticleSortingOrder;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);
        ps.Emit(teleportParticleBurstCount);

        Destroy(go, teleportParticleDestroyDelay);
    }

    private void UpdateSpeedParticles()
    {
        if (speedParticles == null)
            speedParticles = GetComponentInChildren<ParticleSystem>(true);

        float rps = Mathf.Abs(rigidBody.angularVelocity) / 360f;

        if(rps>10) aura.SetActive(true);
        else aura.SetActive(false);

        if (rps >= particlesStartRps)
        {
            if (!speedParticles.isPlaying) { 
                speedParticles.Play(true);
                var main = speedParticles.main;
                main.startLifetime = rps/30;
            }
        }
        else
        {
            if (speedParticles.isPlaying)
                speedParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    private void FixedUpdate()
    {
        ApplyMovementForce();
        ApplyFriction();
        RestrictSpeed();
    }

    private void ApplyMovementForce()
    {
        if (inputDirection == Vector2.zero)
            return;

        Vector2 cameraRight = Camera.main.transform.right;
        Vector2 cameraUp = Camera.main.transform.up;


        Vector2 movementDirection =
         (cameraRight * inputDirection.x) +
         (cameraUp * inputDirection.y * verticalForceMultiplier);

        rigidBody.AddForce(movementDirection.normalized * thrustForce * PlayerTimeCompensation, ForceMode2D.Force);
    }

    private void ApplyFriction()
    {
        if (rigidBody.linearVelocity.sqrMagnitude < 0.001f)
        {
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 frictionForce = -rigidBody.linearVelocity * frictionModifier;
        rigidBody.AddForce(frictionForce, ForceMode2D.Force);
    }

    private void RestrictSpeed()
    {
        if (rigidBody.linearVelocity.magnitude > maxSpeed)
        {
            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * maxSpeed;
        }
    }

    public enum DefenseAbility
    {
        Teleport,
        Repulse,
        SlowTime
    }
    public void SetDefenseAbility(DefenseAbility ability)
    {
        ResetAbilities();

        switch (ability)
        {
            case DefenseAbility.Teleport:
                teleporterActive = true;
                Debug.Log("Teleporter active!");
                break;

            case DefenseAbility.Repulse:
                repulsorActive = true;
                Debug.Log("Repulsor Active!");
                break;

            case DefenseAbility.SlowTime:
                Debug.Log("Chrono active!");
                chronoshiftActive = true;
                break;
        }
    }
    public void ResetAbilities()
    {
        // Slow Time
        teleporterActive = false;
        repulsorActive = false;
        chronoshiftActive = false;
        chronoshifting = false;
        nextSlowTimeUseTime = 0f;
        slowTimeEndTime = 0f;
        if (musicSource != null)
            musicSource.pitch = normalMusicPitch;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        PlayerTimeCompensation = 1f;

        if (slowTimeIcon != null)
            slowTimeIcon.SetActive(false);

        // Teleport
        nextTeleportTime = 0f;
        if (teleportIcon != null)
            teleportIcon.SetActive(false);

        // Repulsor
        nextRepulseTime = 0f;
        if (repulsorIcon != null)
            repulsorIcon.SetActive(false);

        if (repulsorBlast != null)
            repulsorBlast.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("AI"))
            return;

        Rigidbody2D aiRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (aiRb == null)
            return;

        ContactPoint2D contact = collision.GetContact(0);

        Vector2 knockbackDirection =
            ((Vector2)transform.position - contact.point).normalized;

        float aiSpeed = Mathf.Clamp(aiRb.linearVelocity.magnitude, 5f, 30f);
        float playerRps = Mathf.Abs(rigidBody.angularVelocity) / 360f;

        // 0 at 0 RPS, 1 at 25 RPS
        float spinFactor = Mathf.Clamp01(playerRps / 10f);

        // Smooth falloff
        spinFactor = spinFactor * spinFactor;

        float knockbackForce = aiSpeed * Mathf.Lerp(1.3f, 0.2f, spinFactor);
        float currentMaxKnockback = maxKnockback;
        if (playerRps > 10) currentMaxKnockback = 10;
        if (playerRps > 20) currentMaxKnockback = 5;
        if (playerRps > 30) currentMaxKnockback = 2;
        if (playerRps > 40) currentMaxKnockback = 1;
        if(playerRps > 50) currentMaxKnockback = 0.5f;
        knockbackForce *= knockbackMultiplier;
        knockbackForce = Mathf.Clamp(knockbackForce, .5f, maxKnockback);

        rigidBody.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        //Debug.Log(
        //    $"AI Speed: {aiSpeed:F1}, RPS: {playerRps:F1}, Knockback: {knockbackForce:F1}"
        //);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & unstuckCollisionLayers) == 0)
            return;

        if (stuckCollider == collision.collider)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckCollider = collision.collider;
            stuckTimer = 0f;
        }

        if (stuckTimer >= stuckTimeBeforeUnstick)
        {
            AutoUnstuck(collision);
            stuckTimer = 0f;
            stuckCollider = null;
        }
    }

    private void ClampRotationSpeed()
{
    rigidBody.angularVelocity = Mathf.Clamp(
        rigidBody.angularVelocity,
        -maxRotationalSpeed,
        maxRotationalSpeed
    );
}

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == stuckCollider)
        {
            stuckCollider = null;
            stuckTimer = 0f;
        }
    }

    private void AutoUnstuck(Collision2D collision)
    {
        Vector2 pushDirection = Vector2.zero;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            pushDirection += contact.normal;
        }

        if (pushDirection.sqrMagnitude < 0.001f)
        {
            pushDirection = ((Vector2)transform.position - collision.rigidbody.position).normalized;
        }
        else
        {
            pushDirection.Normalize();
        }

        rigidBody.position += pushDirection * unstuckDistance;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.AddForce(pushDirection * unstuckForce, ForceMode2D.Impulse);
    }
}