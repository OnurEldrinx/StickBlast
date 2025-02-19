using UnityEngine;

public class RelativePositionAdjuster : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private Vector3 offset;

    private void OnEnable()
    {
        UIManager.Instance.OnPlayButtonClicked += (_, _) =>
        {
            Invoke(nameof(Apply),0.25f);
        };
    }

    public void Apply()
    {
        transform.position = anchor.position + offset;
    }
}
