using UnityEngine;
using UnityEngine.UI;

public class DebugButtons : MonoBehaviour
{
    public enum ActionType
    {
        ExitCombat,
        AddExp
    }

    public ActionType actionType;
    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Call the appropriate method on the SceneLoader singleton
        switch (actionType)
        {
            case ActionType.ExitCombat:
                GameManager.Instance.ExitCombat();
                break;

            case ActionType.AddExp:
                Player.Instance.AddExperience(10);
                break;
        }
    }
}
