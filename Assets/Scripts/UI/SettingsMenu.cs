using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider volumeSlider;

    void Start()
    {
        settingsPanel.SetActive(false);
        volumeSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.SetVolume(volumeSlider.value); });
        volumeSlider.value = AudioManager.Instance.musicSource.volume;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SetVolume(float value)
   {
       AudioManager.Instance.SetVolume(value);
   }

}
