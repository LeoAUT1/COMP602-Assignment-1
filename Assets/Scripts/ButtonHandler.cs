using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public enum SceneType
    {
        GameBoard,
        Combat,
        MainMenu,
        GameEnd,
        NewGame
    }

    public SceneType targetScene;

    private void Start()
    {
        // Get the Button component and add a listener
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Call the appropriate method on the SceneLoader singleton
        switch (targetScene)
        {
            case SceneType.GameBoard:
                SceneLoader.Instance.LoadGameScene();
                break;
            case SceneType.Combat:
                SceneLoader.Instance.LoadCombatScene();
                break;
            case SceneType.MainMenu:
                SceneLoader.Instance.LoadMainMenu();
                break;
            case SceneType.GameEnd:
                SceneLoader.Instance.LoadGameEnd();
                break;
            case SceneType.NewGame:
                SceneLoader.Instance.LoadGameScene(newGame: true);
                break;
        }
    }
}
