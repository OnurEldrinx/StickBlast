using UnityEngine;

public class DotSensor : MonoBehaviour
{
    private Vector3 _snapPosition;

    public void SetSnapPosition(Vector3 p)
    {
        _snapPosition = p;
    }

    public Vector3 GetSnapPosition()
    {
        return _snapPosition;
    }
}
