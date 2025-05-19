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
        // 1. Initial checks for the tile itself
        if (tile == null)
        {
            // If the main tile is gone, and we still have path objects, it's a change.
            // Or, if there's no tile, there's nothing to check, so no "change" in its connections.
            // Let's assume if tile is null, we can't determine change, or it implies a cleanup is needed.
            // If pathObjects exist, it means they are orphaned.
            return pathObjects.Count > 0;
        }

        BoardTile nextTile = tile.GetNextTile(); // Or tile.nextTile

        // 2. Scenario: No next tile is expected
        if (nextTile == null)
        {
            // If no next tile is defined, but we have a path object, it means the path is outdated.
            return pathObjects.Count > 0;
        }

        // 3. Scenario: A next tile is expected
        // At this point, nextTile is NOT null.

        // If no path object exists, but one is expected, it's a change.
        if (pathObjects.Count == 0)
        {
            return true;
        }

        // If there's more than one path object, something is wrong (should only be 0 or 1).
        if (pathObjects.Count > 1)
        {
            Debug.LogWarning($"TileConnectionVisualizer for {tile.name}: Found {pathObjects.Count} path objects, expected 0 or 1. Forcing update.", this);
            return true;
        }

        // At this point, nextTile is NOT null, and pathObjects.Count IS 1.
        GameObject pathObject = pathObjects[0];

        // If the tracked path object has been destroyed externally.
        if (pathObject == null)
        {
            // pathObjects list might still contain a null reference if Destroy was called elsewhere.
            // UpdatePathVisuals will clear and rebuild, so this indicates a change.
            return true;
        }

        // 4. Check position of the existing path object
        Vector3 expectedMidPoint = (tile.transform.position + nextTile.transform.position) / 2;
        if (Vector3.Distance(pathObject.transform.position, expectedMidPoint) > 0.001f)
        {
            return true; // Position has changed
        }

        // 5. Check orientation of the existing path object
        // Ensure tile and nextTile are not at the same position to avoid LookRotation issues
        if (tile.transform.position == nextTile.transform.position)
        {
            // If they are at the same spot, the current rotation is fine unless it's NaN,
            // but LookRotation would give an error. We can skip this check or handle it.
            // For simplicity, if they are at the same spot, we assume orientation doesn't need an update
            // unless the path visual itself has a default "zero-length" orientation.
            // This edge case might need specific handling based on visual requirements.
        }
        else
        {
            Quaternion expectedRotation = Quaternion.LookRotation(nextTile.transform.position - tile.transform.position);
            if (Quaternion.Angle(pathObject.transform.rotation, expectedRotation) > 0.1f) // Allow small tolerance
            {
                return true; // Orientation has changed
            }
        }

        // If all checks pass, no change detected
        return false;
    }

    public void UpdatePathVisuals()
    {
        // Ensure prerequisites are met
        if (tile == null || tile.pathVisualiser == null || pathPrefab == null)
        {
            // It's good practice to specify which tile is having the issue if 'tile' is not null
            string tileName = tile != null ? tile.name : "UNKNOWN_TILE";
            Debug.LogWarning($"TileConnectionVisualizer on {gameObject.name} (for tile: {tileName}): Missing tile reference, tile.pathVisualiser, or pathPrefab. Cannot update visuals.", this);
            return;
        }

        // --- Clearing Logic (remains the same) ---
        // Clear old paths by destroying all children of the target transform
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in tile.pathVisualiser)
        {
            if (child != null) // Ensure child is not already destroyed or null
                childrenToDestroy.Add(child.gameObject);
        }

        foreach (GameObject objToDestroy in childrenToDestroy)
        {
            if (objToDestroy == null) continue; // Double check, in case it was destroyed by another process
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(objToDestroy);
            else
                Destroy(objToDestroy);
        }
        // --- End Clearing Logic ---

        // Clear the tracking list now that objects are destroyed
        pathObjects.Clear();

        // Get the single next tile
        BoardTile nextTile = tile.GetNextTile(); // Or tile.nextTile directly if public access is fine

        // Create a new path if a connection exists
        if (nextTile != null)
        {
            // Instantiate under the designated parent (tile.pathVisualiser)
            GameObject pathInstance = Instantiate(pathPrefab, tile.pathVisualiser);

            // Name the path for easier identification in hierarchy
            pathInstance.name = PATH_PREFIX + tile.name + "_to_" + nextTile.name;

            
            // and then scale/rotate. For a centered pivot:
            Vector3 midPoint = (tile.transform.position + nextTile.transform.position) / 2;
            pathInstance.transform.position = midPoint;

            // Orient toward target
            // Ensure the path prefab is oriented correctly (e.g., its "forward" is along its length)
            pathInstance.transform.LookAt(nextTile.transform);

            // Scale to span the distance
            float distance = Vector3.Distance(tile.transform.position, nextTile.transform.position);
            // Assuming the pathPrefab is 1 unit long along its local Z-axis (or whichever axis points along the path)
            pathInstance.transform.localScale = new Vector3(pathInstance.transform.localScale.x, pathInstance.transform.localScale.y, distance);

            // Add to tracking list (optional, but good if you need to reference it later)
            pathObjects.Add(pathInstance);
        }
        // If nextTile is null, no path is created, which is the desired behavior.
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
