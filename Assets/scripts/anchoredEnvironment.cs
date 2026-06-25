
using UnityEngine;

public class AnchoredEnvironment : MonoBehaviour
{
    private Transform originalParent;

    private void Awake()
    {
        originalParent = transform.parent;
    }

    public void AttachToWheel(Transform wheel)
    {
        if (wheel == null) return;

        transform.SetParent(wheel, true);
    }

    public void ReturnToLayout()
    {
        if (originalParent == null) return;

        transform.SetParent(originalParent, true);
    }
}