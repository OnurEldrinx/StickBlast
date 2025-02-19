using UnityEngine;

[CreateAssetMenu(fileName = "ShapeData", menuName = "Scriptable Objects/ShapeData")]
public class ShapeData : ScriptableObject
{
    public int id;
    public ThemeData theme;
    public Sprite sprite;
    public LayerMask gridLayer;
    public float dragOffset;
    public int defaultSortingOrder = 10;
    public int ghostSortingOrder = 3;
    public float spawnScale;
    public Vector2Int[] offsets;
}
