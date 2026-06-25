using UnityEngine;

public class StopSpinnerButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StopSpinnerFromButton()
    {
        WeaponSpinner spinner = FindFirstObjectByType<WeaponSpinner>();

        if (spinner == null)
        {
            Debug.LogError("No WeaponSpinner found in scene.");
            return;
        }

        spinner.StartTrialButtonPressed();
    }
}
