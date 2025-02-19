using UnityEngine;

public class RelativePositionAdjuster : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        transform.position = anchor.position + offset;
    }
}
