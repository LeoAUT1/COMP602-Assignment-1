using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    // Include one SceneLoader prefab per scene
    // The singleton pattern will ensure that any duplicates self-destruct

    public void LoadGameScene()
    {
        Debug.Log("attempting to load main game scene");
        SceneManager.LoadScene("GameBoard");
    }

    public void LoadCombatScene()
    {
        SceneManager.LoadScene("Combat");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void LoadVictoryScene()
    {
        SceneManager.LoadScene("Victory");
    }
}
