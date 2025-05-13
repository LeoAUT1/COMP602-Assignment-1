using UnityEngine;
using UnityEngine.SceneManagement; // Needed for DontDestroyOnLoad scene check

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false; // Flag to prevent issues during shutdown
    private static readonly object _lock = new object(); // Lock for thread safety (good practice, though less critical if only main thread accesses)

    // --- Enhanced Logging ---
    private static bool _detailedLoggingEnabled = false; // Set to false to reduce console spam once diagnosed

    public static T Instance
    {
        get
        {
            // Check if the application is quitting to prevent creating instances during shutdown
            if (_applicationIsQuitting)
            {
                if (_detailedLoggingEnabled) Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
                return null;
            }

            // Use lock for potential multi-threaded access, although rare in typical Unity usage
            lock (_lock)
            {
                if (_instance == null)
                {
                    if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Instance getter for {typeof(T).Name}: _instance is null. Trying FindObjectOfType.");

                    // Try finding an existing instance in the scene(s)
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Instance getter for {typeof(T).Name}: FindObjectOfType found nothing. Creating new GameObject.");
                        // If none found, create a new GameObject and add the component
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).Name + " (Singleton)";

                        // Important: Mark the *newly created* instance's GameObject to persist
                        // This handles the edge case where Instance is accessed *before* any Awake runs.
                        DontDestroyOnLoad(singletonObject);
                        if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Instance getter for {typeof(T).Name}: Created and marked DontDestroyOnLoad for new instance ID: {_instance.GetInstanceID()}");
                    }
                    else
                    {
                        if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Instance getter for {typeof(T).Name}: FindObjectOfType found existing instance ID: {_instance.GetInstanceID()} on GameObject '{_instance.gameObject.name}'.");
                        // Optional: Check if the found instance is actually marked DontDestroyOnLoad already
                        // if (_instance.gameObject.scene.buildIndex == -1) { // Scene index -1 is DontDestroyOnLoad
                        //     Debug.Log($"[Singleton] Found instance {typeof(T).Name} ID: {_instance.GetInstanceID()} is in DontDestroyOnLoad scene.");
                        // }
                    }
                }
                else
                {
                    // Optional log to see when the getter returns an already existing instance
                    // if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Instance getter for {typeof(T).Name}: Returning existing instance ID: {_instance.GetInstanceID()}");
                }
                return _instance;
            }
        }
    }

    // Removed the IsFirstLoad property as it wasn't relevant to the persistence issue
    // and could cause confusion. Static bools are tricky with domain reloading disabled.

    protected virtual void Awake()
    {
        int currentInstanceID = GetInstanceID();
        if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Awake: {typeof(T).Name} running for instance ID: {currentInstanceID} on GameObject '{gameObject.name}' in scene '{gameObject.scene.name}'.");

        lock (_lock) // Use lock here too for consistency
        {
            if (_instance == null)
            {
                // If no instance exists yet, this one becomes the singleton instance
                _instance = this as T;
                // Mark this GameObject to persist across scenes
                DontDestroyOnLoad(gameObject);
                if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Awake: Instance was null. Assigning this instance (ID: {currentInstanceID}) and setting DontDestroyOnLoad.");
            }
            else if (_instance != this)
            {
                // If an instance *already exists* and it's *not this one*, then destroy this duplicate.
                if (_detailedLoggingEnabled)
                {
                    Debug.LogWarning($"[Singleton] Awake: An instance already exists (ID: {_instance.GetInstanceID()} on GameObject '{_instance.gameObject.name}'). Destroying this duplicate instance (ID: {currentInstanceID} on GameObject '{gameObject.name}').");
                    // Log if the existing instance is the one marked DontDestroyOnLoad
                    if (_instance.gameObject.scene.buildIndex == -1)
                    {
                        Debug.Log($"[Singleton] Awake: The existing instance (ID: {_instance.GetInstanceID()}) is correctly in the DontDestroyOnLoad scene.");
                    }
                    else
                    {
                        Debug.LogWarning($"[Singleton] Awake: The existing instance (ID: {_instance.GetInstanceID()}) is NOT in the DontDestroyOnLoad scene! This might indicate an execution order issue where the scene instance was assigned first.");
                    }
                }
                Destroy(gameObject); // Destroy the current GameObject, as it's a duplicate
            }
            else
            {
                // This case means _instance is not null and _instance == this.
                // This can happen if Awake runs again after the Instance getter already found and assigned this object.
                if (_detailedLoggingEnabled) Debug.Log($"[Singleton] Awake: This instance (ID: {currentInstanceID}) is already the assigned singleton. Ensuring DontDestroyOnLoad is set.");
                // Ensure it's marked correctly even if Awake runs multiple times (shouldn't happen often)
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        int currentInstanceID = GetInstanceID();
        // Check if the instance being destroyed *is* the singleton instance
        if (_instance == this)
        {
            // Use the flag to prevent the Instance getter from trying to re-create during shutdown
            if (!_applicationIsQuitting) // Only log warning if not quitting intentionally
            {
                Debug.LogWarning($"[Singleton] OnDestroy: The main singleton instance {typeof(T).Name} (ID: {currentInstanceID}) is being destroyed!", this.gameObject);
            }
            _instance = null; // Clear the static reference
        }
        else
        {
            if (_detailedLoggingEnabled) Debug.Log($"[Singleton] OnDestroy: Duplicate instance {typeof(T).Name} (ID: {currentInstanceID}) is being destroyed as intended or due to scene unload.", this.gameObject);
        }
    }
    public virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
        if (_detailedLoggingEnabled) Debug.Log($"[Singleton] OnApplicationQuit: Setting flag for {typeof(T).Name}.");
    }
}
