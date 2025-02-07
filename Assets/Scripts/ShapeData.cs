using UnityEngine;

[CreateAssetMenu(fileName = "ShapeData", menuName = "Scriptable Objects/ShapeData")]
public class ShapeData : ScriptableObject
{
    public ThemeData theme;
    public Sprite sprite;
    public LayerMask gridLayer;
    public float dragOffset;

}
