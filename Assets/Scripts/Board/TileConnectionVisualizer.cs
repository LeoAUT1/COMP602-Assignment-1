using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileConnectionVisualizer : MonoBehaviour
{
    private BoardTile tile;
    [SerializeField] private GameObject pathPrefab;
    // This list still helps track managed paths, e.g., for HasAnyNextTilePositionChanged
    private List<GameObject> pathObjects = new List<GameObject>();

    private Vector3 lastPosition;
    private bool needsUpdate = true;
    // PATH_PREFIX is less critical now for cleanup but still useful for naming
    private const string PATH_PREFIX = "Path_";

    private void Awake()
    {
        // Ensure tile reference is available early if possible
        if (tile == null)
            tile = GetComponent<BoardTile>();

        // Clean up any existing path objects under the designated transform
        CleanupExistingPaths();
    }

    private void Start()
    {
        // Redundant if Awake runs, but safe to keep
        if (tile == null)
            tile = GetComponent<BoardTile>();

        if (tile != null)
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

        if (tile != null)
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
        // Clear the internal tracking list first
        pathObjects.Clear();

        if (tile == null || tile.pathVisualiser == null)
        {
            // Can't clean children if the target doesn't exist
            return;
        }

        // Find and destroy any child objects under the pathVisualiser transform
        // Use a temporary list to avoid issues modifying collection while iterating (esp. with DestroyImmediate)
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in tile.pathVisualiser)
        {
            if (child != null) // Basic null check
            {
                // Optional: Only destroy objects matching the prefix if you want to be specific
                // if (child.name.StartsWith(PATH_PREFIX))
                // {
                //     childrenToDestroy.Add(child.gameObject);
                // }
                // Or, destroy *all* children regardless of name:
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject objToDestroy in childrenToDestroy)
        {
            if (objToDestroy == null) continue; // Check again before destroying

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(objToDestroy);
            }
            else
            {
                Destroy(objToDestroy);
            }
        }
    }


    private void Update()
    {
        // Essential checks before proceeding
        if (tile == null || tile.pathVisualiser == null || pathPrefab == null) return;

        // Only update when position changes, connected tile moves, or explicitly needed
        if (transform.position != lastPosition || HasAnyNextTilePositionChanged() || needsUpdate)
        {
            UpdatePathVisuals();
            lastPosition = transform.position;
            needsUpdate = false;
        }
    }

    private bool HasAnyNextTilePositionChanged()
    {
        // Check if tile references are valid
        if (tile == null || tile.nextTiles == null) return false; // Or true if you want an update on null

        // If the number of tracked objects doesn't match expected, assume change
        if (pathObjects.Count != tile.nextTiles.Count)
            return true;

        // Check if any connected tile has moved relative to its path visual
        for (int i = 0; i < tile.nextTiles.Count; i++)
        {
            BoardTile nextTile = tile.nextTiles[i];
            if (nextTile != null && i < pathObjects.Count)
            {
                GameObject pathObject = pathObjects[i];
                if (pathObject != null)
                {
                    // Check if the path's current position matches the expected midpoint
                    Vector3 expectedMidPoint = (tile.transform.position + nextTile.transform.position) / 2;
                    if (Vector3.Distance(pathObject.transform.position, expectedMidPoint) > 0.001f)
                        return true;

                    // Check if the path's current orientation points to the next tile
                    // Note: Floating point comparisons require tolerance
                    Quaternion expectedRotation = Quaternion.LookRotation(nextTile.transform.position - tile.transform.position);
                    if (Quaternion.Angle(pathObject.transform.rotation, expectedRotation) > 0.1f) // Allow small tolerance
                        return true;
                }
                else
                {
                    // If a path object is unexpectedly null, trigger update
                    return true;
                }
            }
            else if (nextTile != null && i >= pathObjects.Count)
            {
                // More nextTiles than pathObjects tracked, needs update
                return true;
            }
            // If nextTile is null, we don't expect a path, so no change detected here
        }

        return false;
    }

    private void UpdatePathVisuals()
    {
        // Ensure prerequisites are met
        if (tile == null || tile.pathVisualiser == null || pathPrefab == null)
        {
            Debug.LogWarning($"TileConnectionVisualizer on {gameObject.name}: Missing tile, pathVisualiser, or pathPrefab reference. Cannot update visuals.", this);
            return;
        }

        // --- New Clearing Logic ---
        // Clear old paths by destroying all children of the target transform
        // Use a temporary list to avoid modifying collection while iterating
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in tile.pathVisualiser)
        {
            if (child != null)
                childrenToDestroy.Add(child.gameObject);
        }

        foreach (GameObject objToDestroy in childrenToDestroy)
        {
            if (objToDestroy == null) continue;
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(objToDestroy);
            else
                Destroy(objToDestroy);
        }
        // --- End New Clearing Logic ---

        // Clear the tracking list now that objects are destroyed
        pathObjects.Clear();

        // Create new paths if connections exist
        if (tile.nextTiles != null)
        {
            for (int i = 0; i < tile.nextTiles.Count; i++)
            {
                BoardTile nextTile = tile.nextTiles[i];
                if (nextTile != null)
                {
                    // --- Instantiate under the designated parent ---
                    GameObject path = Instantiate(pathPrefab, tile.pathVisualiser);
                    // --- End Change ---

                    // Name the path for easier identification in hierarchy
                    path.name = PATH_PREFIX + tile.name + "_to_" + nextTile.name;

                    // Position at midpoint
                    Vector3 midPoint = (tile.transform.position + nextTile.transform.position) / 2;
                    path.transform.position = midPoint;

                    // Orient toward target
                    path.transform.LookAt(nextTile.transform);

                    // Scale to span the distance
                    float distance = Vector3.Distance(tile.transform.position, nextTile.transform.position);
                    // Assuming the prefab is 1 unit long along Z
                    path.transform.localScale = new Vector3(path.transform.localScale.x, path.transform.localScale.y, distance);

                    // Add to tracking list
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

    // If the BoardTile component changes its connections (e.g., in Inspector), flag for update
    private void OnValidate()
    {
        // Ensure tile reference is updated if component is added/removed in editor
        if (tile == null)
            tile = GetComponent<BoardTile>();

        needsUpdate = true;
    }
}
