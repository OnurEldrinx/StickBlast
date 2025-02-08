using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private SpriteRenderer fillSprite;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Draggable> draggablePrefabs;

    // Tracks the number of draggables currently in the tray.
    public int draggableCount;

    private void Start()
    { 
        SpawnDraggables();
    }

    /// <summary>
    /// Instantiates a fill sprite as a child of the specified cell,
    /// applies the provided color, and plays a scale tween for a fill effect.
    /// </summary>
    /// <param name="cell">The transform of the cell to fill.</param>
    /// <param name="c">The color to fill with.</param>
    public void FillTheCell(Transform cell, Color c)
    {
        if (fillSprite == null)
        {
            Debug.LogError("FillSprite is not assigned in SpawnManager!");
            return;
        }
        
        // Instantiate fillSprite as a child of the given cell.
        SpriteRenderer f = Instantiate(fillSprite, cell, true);
        f.color = c;
        f.transform.localPosition = Vector3.zero;
        f.transform.DOScale(Vector3.one * 1.175f, 0.5f);


        cell.GetComponent<Cell>().fill = f.gameObject;

    }

    /// <summary>
    /// Spawns draggable objects at each spawn point.
    /// The draggable objects are picked randomly from the list of prefabs,
    /// and their scale is tweened from zero to the desired spawn scale.
    /// </summary>
    public void SpawnDraggables()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned in SpawnManager!");
            return;
        }
        if (draggablePrefabs == null || draggablePrefabs.Count == 0)
        {
            Debug.LogError("No draggable prefabs assigned in SpawnManager!");
            return;
        }

        // Reset the draggable count before spawning.
        draggableCount = 0;
        foreach (var spawnPoint in spawnPoints)
        {
            Draggable prefab = draggablePrefabs[Random.Range(0, draggablePrefabs.Count)];
            if (prefab == null)
            {
                Debug.LogWarning("A draggable prefab is null in SpawnManager.");
                continue;
            }
            
            Draggable d = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            d.transform.localScale = Vector3.zero;
            
            // Tween to the spawn scale defined in the ShapeData.
            d.transform.DOScale(d.data.spawnScale * Vector3.one, 0.5f).SetEase(Ease.OutBack);
            draggableCount++;
        }
    }

    /// <summary>
    /// Called when a draggable piece is placed successfully.
    /// Decreases the draggable count and spawns new draggables if the tray is empty.
    /// </summary>
    public void UpdateTray()
    {
        // Ensure draggableCount does not drop below zero.
        draggableCount = Mathf.Max(0, draggableCount - 1);
        if (draggableCount <= 0)
        {
            draggableCount = 0;
            SpawnDraggables();
        }
    }
}
