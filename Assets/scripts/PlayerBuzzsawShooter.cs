using UnityEngine;
using UnityEngine.UI;

public class PlayerBuzzsawShooter : MonoBehaviour
{
    [Header("Buzzsaw")]
    [SerializeField] private BuzzsawProjectile buzzsawPrefab;
    [SerializeField] private Transform firePoint;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShootBuzzsaw();
        }
    }
    private void ShootBuzzsaw()
    {
        if (buzzsawPrefab == null) return;

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - transform.position).normalized;
        //CircleCollider2D circle = GetComponent<CircleCollider2D>();

        float spinnerRadius = 2f; // adjust to your wheel size

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