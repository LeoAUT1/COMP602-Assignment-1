using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Persistent Scene Loader for the entire game

    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameBoard"); // Replace with your game scene name
    }

    public void LoadCombatScene()
    {
        SceneManager.LoadScene("Combat"); // Replace with your menu scene name
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your menu scene name
    }
}
