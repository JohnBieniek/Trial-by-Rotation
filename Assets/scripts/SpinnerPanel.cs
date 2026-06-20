using System.Collections;
using UnityEngine;

public class SpinnerPanel : MonoBehaviour
{
    [SerializeField] private RectTransform wheel;
    [SerializeField] private GameObject[] topPrefabs;
    [SerializeField] private Transform playerSpawnPoint;

    [SerializeField] private float spinSpeed = 180f;
    [SerializeField] private float stopTime = 2f;

    private bool spinning = true;
    private bool stopping;

    private void Update()
    {
        if (spinning)
        {
            wheel.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }

    public void StopWheel()
    {
        if (!stopping)
        {
            StartCoroutine(StopRoutine());
        }
    }

    private IEnumerator StopRoutine()
    {
        stopping = true;

        float startSpeed = spinSpeed;
        float timer = 0f;

        while (timer < stopTime)
        {
            timer += Time.deltaTime;

            spinSpeed = Mathf.Lerp(
                startSpeed,
                0f,
                timer / stopTime);

            yield return null;
        }

        spinSpeed = 0f;
        spinning = false;

        SelectTop();
    }

    private void SelectTop()
    {
        float angle = wheel.eulerAngles.z;

        float sliceSize = 360f / 6f;

        int index =
            Mathf.FloorToInt(
                ((360f - angle) % 360f) / sliceSize);

        Debug.Log("Selected: " + index);

        Instantiate(
            topPrefabs[index],
            playerSpawnPoint.position,
            Quaternion.identity);
    }
}