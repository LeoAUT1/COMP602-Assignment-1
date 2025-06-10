using UnityEngine;

public class DebugButtonsManager : MonoBehaviour
{
    // Assign the GameObject that serves as the parent for your debug buttons here.
    public GameObject debugButtonsContainer;

    void Start()
    {
        if (debugButtonsContainer == null)
        {
            Debug.LogError("DebugButtonsManager: 'debugButtonsContainer' is not assigned. This script won't do much without it.", this);
            this.enabled = false; // Disable this component; it's useless without its target.
            return;
        }

        // If not running in the Unity Editor, deactivate the debug buttons.
        if (!Application.isEditor)
        {
            debugButtonsContainer.SetActive(false);
        }
        // If in the editor, the buttons' initial state is whatever you set in the scene.
    }

    void Update()
    {
        // Toggle the debug buttons container's active state when KeyCode.BackQuote (usually tilde/backtick) is pressed.
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            // This check is technically only needed if you remove the 'this.enabled = false;' line in Start.
            // But, defensive programming rarely hurts.
            if (debugButtonsContainer != null)
            {
                debugButtonsContainer.SetActive(!debugButtonsContainer.activeSelf);
            }
        }
    }
}
