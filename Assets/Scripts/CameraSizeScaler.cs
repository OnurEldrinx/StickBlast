using System;
using UnityEngine;

public class CameraSizeScaler : MonoBehaviour
{
    [SerializeField] private  float offset = 1f;
    private Camera _camera;
    private float _screenAspect;
    
    public static Action<int, int> OnAdjust;
    
    private void Awake()
    {
        _camera = Camera.main;
        _screenAspect = (float)Screen.width / Screen.height;
    }

    private void OnEnable()
    {
        OnAdjust += AdjustScale;
    }

    private void OnDisable()
    {
        OnAdjust -= AdjustScale;
    }

    private void AdjustScale(int gridWidth,int gridHeight)
    {
        gridWidth = gridWidth < 5 ? 5 : gridWidth;
        gridHeight = gridHeight < 5 ? 5 : gridHeight;
        
        
        var sizeBasedOnHeight = (gridHeight / 2f) + offset;

        var sizeBasedOnWidth = (gridWidth / 2f) / _screenAspect + offset;

        var requiredSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);

        _camera.orthographicSize = requiredSize;
    }

}
