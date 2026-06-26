using UnityEngine;

public class PlayerBuzzsawShooter : MonoBehaviour
{
    [Header("Buzzsaw")]
    [SerializeField] private BuzzsawProjectile buzzsawPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float secondsBetweenShots;

    private float nextShotTime;

    [Header("Audio")]
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioSource gunSource;

    private void Update()
    {
        if (!GameController.hasStarted || GameController.Instance.IsGameWonOrLost() || GameController.Instance.IsMenuOpen())
            return;


        if (Input.GetMouseButton(0) && Time.time >= nextShotTime)
        {
            //Debug.Log($"Shot from {gameObject.name} at {Time.time}");
            ShootBuzzsaw();
            nextShotTime = Time.time + secondsBetweenShots;
        }
    }
    private void ShootBuzzsaw()
    {
        if (buzzsawPrefab == null) return;

        gunSource.PlayOneShot(gunSound, 0.4f);

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - transform.position).normalized;

        float spinnerRadius = 1.5f; // adjust to spinner size

        Vector3 spawnPosition =
            transform.position + (Vector3)(direction * spinnerRadius);

        BuzzsawProjectile saw = Instantiate(
            buzzsawPrefab,
           spawnPosition,
            Quaternion.identity
        );

        saw.Launch(
            direction
        );
    }
}