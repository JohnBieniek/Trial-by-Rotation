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

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraFollowSpeed = 5f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

    private bool isGameOver = false;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (maxDistanceFromWheelCenter <= 0f)
        {
            maxDistanceFromWheelCenter = GetWheelRadius();
        }
    }

    private void Update()
    {
        if (isGameOver || player == null || wheelOfJustice == null)
        {
            return;
        }

        float distance = Vector2.Distance(player.position, wheelOfJustice.position);

        if (distance > maxDistanceFromWheelCenter + wheelEdgeBuffer)
        {
            GameOver();
        }
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
        Debug.Log("GAME OVER TRIGGERED");

        isGameOver = true;

        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void LateUpdate()
    {
        if (isGameOver || mainCamera == null || player == null)
        {
            return;
        }

        Vector3 targetPosition = player.position + cameraOffset;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );
    }
}