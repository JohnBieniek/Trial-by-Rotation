using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemyCount = 3;
    [SerializeField] private Transform enemyParent;

    [Header("Spawn Rules")]
    [SerializeField] private float minSpawnRadius = 2f;
    [SerializeField] private float minDistanceFromPlayer = 3f;

    private Transform wheelOfJustice;
    private Transform player;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //private void Start()
    //{
    //    SpawnEnemies();
    //}

    public void SpawnEnemy()
    {
        wheelOfJustice = GameObject.Find("Wheel of Justice")?.transform;

        if (wheelOfJustice == null)
        {
            Debug.LogError("EnemySpawner could not find WheelOfJustice.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner has no enemy prefabs assigned.");
            return;
        }

        float maxSpawnRadius = 50;

       
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector3 spawnPosition = GetValidSpawnPosition(maxSpawnRadius);

        Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity,
            enemyParent
        );
        
    }


    public void SpawnEnemies()
    {
        SimpleAiMovement[] enemies = FindObjectsByType<SimpleAiMovement>(
            FindObjectsSortMode.None
        );
        //Debug.Log($"Found {enemies.Length} existing enemies. Destroying them before spawning new ones.");
        foreach (SimpleAiMovement enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        wheelOfJustice = GameObject.Find("Wheel of Justice")?.transform;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (wheelOfJustice == null)
        {
            Debug.LogError("EnemySpawner could not find WheelOfJustice.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner has no enemy prefabs assigned.");
            return;
        }

        float maxSpawnRadius = 50;

        for (int i = 0; i < StartPanelAccusation.Instance.GetPlaintiffCount(); i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            Vector3 spawnPosition = GetValidSpawnPosition(maxSpawnRadius-3);

            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity,
                enemyParent
            );
            //Debug.Log($"Spawned enemy {i} - {prefab.name} at {spawnPosition}");
        }
    }

    private Vector3 GetValidSpawnPosition(float maxSpawnRadius)
    {

        Vector3 spawnPosition = wheelOfJustice.position;
        int attempts = 0;

        do
        {
            Vector2 offset = Random.insideUnitCircle.normalized *
                             Random.Range(minSpawnRadius, maxSpawnRadius-3);

            spawnPosition = wheelOfJustice.position + (Vector3)offset;

            attempts++;
        }
        while (
            player != null &&
            Vector2.Distance(spawnPosition, player.position) < minDistanceFromPlayer &&
            attempts < 20
        );

        //Debug.Log(
        //    $"Spawn distance from wheel: {Vector2.Distance(spawnPosition, wheelOfJustice.position)} / max {maxSpawnRadius}"
        //);

        return spawnPosition;
    }
}