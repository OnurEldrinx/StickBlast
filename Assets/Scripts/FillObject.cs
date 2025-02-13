using UnityEngine;

public class FillObject : MonoBehaviour
{
    public Material material;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        Invoke(nameof(DisableShine),1.5f);
    }

    private void DisableShine()
    {
        material.SetFloat("_WaveSpeed", 0);
    }
    
}
