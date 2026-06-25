using UnityEngine;

public class PlayerGunShooter : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private float secondsBetweenShots =3f;
    private float nextShotTime;

    private void Update()
    {
        if (!GameController.hasStarted)
            return;

        if (Input.GetMouseButton(0) && Time.time >= nextShotTime)
        {
            Debug.Log($"Shot from {gameObject.name} at {Time.time}");
            ShootBullet();
            nextShotTime = Time.time + secondsBetweenShots;
        }
    }
    private void ShootBullet()
    {
        if (bulletPrefab == null) return;

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - transform.position).normalized;
        //CircleCollider2D circle = GetComponent<CircleCollider2D>();

        float spinnerRadius = 2f; // adjust to your wheel size

        Vector3 spawnPosition =
            transform.position + (Vector3)(direction * spinnerRadius);

        BulletProjectile saw = Instantiate(
            bulletPrefab,
           spawnPosition,
            Quaternion.identity
        );



        saw.Launch(
            direction
        );
    }
}