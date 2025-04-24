using UnityEngine;
using UnityEngine.UI;

public class PlayerAddExpDebugButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Player.Instance.AddExperience(10);
    }
}
