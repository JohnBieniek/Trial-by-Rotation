using UnityEngine;

public class PlayerGunShooter : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float secondsBetweenShots =3f;

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
            ShootBullet();
            nextShotTime = Time.time + secondsBetweenShots;
        }
    }
    private void ShootBullet()
    {
        if (bulletPrefab == null) return;
        gunSource.PlayOneShot(gunSound, 0.4f);
        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - transform.position).normalized;

        float spinnerRadius = 1.5f; // adjust to spinner size

        Vector3 spawnPosition =
            transform.position + (Vector3)(direction * spinnerRadius);

        BulletProjectile bullet = Instantiate(
            bulletPrefab,
           spawnPosition,
            Quaternion.identity
        );


        bullet.Launch(
            direction
        );
    }
}