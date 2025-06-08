using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void QuitButton()
    {
        // If running in the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running in a build
        Application.Quit();
        #endif
    }
}