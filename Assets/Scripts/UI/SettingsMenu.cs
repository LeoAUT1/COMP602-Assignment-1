using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider volumeSlider;

    void Start()
    {
        settingsPanel.SetActive(false);
        volumeSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.SetMusicVolume(volumeSlider.value); });
        volumeSlider.value = AudioManager.Instance.GetMusicVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}
