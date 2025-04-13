using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // Updates in editor mode too
public class TileConnectionVisualizer : MonoBehaviour
{
    private BoardTile tile;
    [SerializeField] private GameObject pathPrefab; // A plane or cylinder
    private List<GameObject> pathObjects = new List<GameObject>();

    private void Start()
    {
        tile = GetComponent<BoardTile>();
    }

    private void OnEnable()
    {
        // This ensures it works in edit mode too when the component is added
        if (tile == null)
            tile = GetComponent<BoardTile>();
    }

    private void Update()
    {
        if (tile != null)
            UpdatePathVisuals();

        Console.Write(tile.name);
    }

    private void UpdatePathVisuals()
    {
        // Clear old paths
        foreach (GameObject path in pathObjects)
        {
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(path);
            else
                Destroy(path);
        }
        pathObjects.Clear();

        // Create new paths
        if (tile.nextTiles != null)
        {
            foreach (BoardTile nextTile in tile.nextTiles)
            {
                if (nextTile != null)
                {
                    GameObject path = Instantiate(pathPrefab, transform);
                    Vector3 midPoint = (tile.transform.position + nextTile.transform.position) / 2;
                    path.transform.position = midPoint;

                    // Orient toward target
                    path.transform.LookAt(nextTile.transform);

                    // Scale to reach target
                    float distance = Vector3.Distance(tile.transform.position, nextTile.transform.position);
                    path.transform.localScale = new Vector3(1, 1, distance);

                    pathObjects.Add(path);
                }
            }
        }
    }
}
