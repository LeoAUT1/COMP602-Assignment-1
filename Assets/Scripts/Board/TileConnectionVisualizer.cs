using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileConnectionVisualizer : MonoBehaviour
{
    private BoardTile tile;
    [SerializeField] private GameObject pathPrefab;
    private List<GameObject> pathObjects = new List<GameObject>();

    private Vector3 lastPosition;
    private bool needsUpdate = true;
    private const string PATH_PREFIX = "Path_";

    private void Awake()
    {
        // Clean up any existing path objects that might have been serialized
        CleanupExistingPaths();
    }

    private void Start()
    {
        tile = GetComponent<BoardTile>();
        lastPosition = transform.position;
        needsUpdate = true;
    }

    private void OnEnable()
    {
        // Guard against transitions
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (tile == null)
            tile = GetComponent<BoardTile>();

        lastPosition = transform.position;
        needsUpdate = true;

        // Only clean up paths when not transitioning between play modes
        if (Time.frameCount > 0) // Skip during initialization frames
        {
            CleanupExistingPaths();
        }
    }


    private void CleanupExistingPaths()
    {
        // Clear the list first
        pathObjects.Clear();

        // Find and destroy any paths with our naming convention
        // We need to be extra careful with null checking during transitions
        try
        {
            // Check if transform is valid first
            if (this == null || transform == null)
                return;

            Transform[] allChildren = GetComponentsInChildren<Transform>();
            if (allChildren == null)
                return;

            foreach (Transform child in allChildren)
            {
                // Check for null before accessing properties
                if (child == null)
                    continue;

                // Additional null check before accessing name
                string childName = null;
                try
                {
                    childName = child.name;
                }
                catch (MissingReferenceException)
                {
                    continue;
                }

                if (childName != null && childName.StartsWith(PATH_PREFIX))
                {
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        if (child.gameObject != null)
                            DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        if (child.gameObject != null)
                            Destroy(child.gameObject);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            // During transitions between play modes, some exceptions might occur
            // Just silently catch them to prevent error spam
            // Debug.LogWarning("CleanupExistingPaths exception: " + ex.Message);
        }
    }


    private void Update()
    {
        if (tile == null) return;

        // Only update when position changes or we explicitly need to
        if (transform.position != lastPosition || HasAnyNextTilePositionChanged() || needsUpdate)
        {
            UpdatePathVisuals();
            lastPosition = transform.position;
            needsUpdate = false;
        }
    }

    private bool HasAnyNextTilePositionChanged()
    {
        if (tile.nextTiles == null || pathObjects.Count != tile.nextTiles.Count)
            return true;

        // Check if any connected tile has moved
        for (int i = 0; i < tile.nextTiles.Count; i++)
        {
            if (tile.nextTiles[i] != null &&
                i < pathObjects.Count &&
                pathObjects[i] != null)
            {
                // The midpoint and orientation would change if the target tile moved
                Vector3 expectedMidPoint = (tile.transform.position + tile.nextTiles[i].transform.position) / 2;
                if (Vector3.Distance(pathObjects[i].transform.position, expectedMidPoint) > 0.001f)
                    return true;
            }
        }

        return false;
    }

    private void UpdatePathVisuals()
    {
        // Clear old paths
        foreach (GameObject path in pathObjects)
        {
            if (path != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(path);
                else
                    Destroy(path);
            }
        }
        pathObjects.Clear();

        // Create new paths
        if (tile.nextTiles != null)
        {
            for (int i = 0; i < tile.nextTiles.Count; i++)
            {
                BoardTile nextTile = tile.nextTiles[i];
                if (nextTile != null)
                {
                    // Name the path with a consistent prefix and index for easier cleanup
                    GameObject path = Instantiate(pathPrefab, transform);
                    path.name = PATH_PREFIX + tile.name + "_to_" + nextTile.name;

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

    // Force update when hierarchy changes - like when connections are modified
    public void SetNeedsUpdate()
    {
        needsUpdate = true;
    }

    // If the BoardTile component changes its connections, call this method
    private void OnValidate()
    {
        needsUpdate = true;
    }
}
