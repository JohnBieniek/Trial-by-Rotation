using UnityEngine;

public class PlayerShotgunShooter : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletsPerShot = 5;
    [SerializeField] private float shotgunArcDegrees = 45f;
    [SerializeField] private float spinnerRadius = 1.5f;
    [SerializeField] private float secondsBetweenShots =3f;
    private float nextShotTime;
    [SerializeField] private AudioClip shotgunSound;
    [SerializeField] private AudioSource shotgunSource;

    private void Update()
    {
        if (!GameController.hasStarted || GameController.Instance.IsGameWonOrLost() ||  GameController.Instance.IsMenuOpen())
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
        if (bulletsPerShot <= 0) return;
        shotgunSource.PlayOneShot(shotgunSound,0.7f);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 aimDirection = (mouseWorld - transform.position).normalized;

        float startAngle = -shotgunArcDegrees / 2f;
        float angleStep = bulletsPerShot == 1
            ? 0f
            : shotgunArcDegrees / (bulletsPerShot - 1);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 shotDirection = RotateVector(aimDirection, angle);

            Vector3 spawnPosition =
                transform.position + (Vector3)(shotDirection * spinnerRadius);

            BulletProjectile bullet = Instantiate(
                bulletPrefab,
                spawnPosition,
                Quaternion.identity
            );

            bullet.Launch(shotDirection);
            Destroy(bullet.gameObject, .2f);
        }
    }
   
    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }
}