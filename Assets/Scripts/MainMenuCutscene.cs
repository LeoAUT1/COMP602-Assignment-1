using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCutscene : MonoBehaviour
{
    [SerializeField] private GameObject[] buttons;
    [SerializeField] private float fadeToBlackDuration = 0.3f;
    [SerializeField] private Image canvasBackground; //should be solid black

    [SerializeField] private float timePerChar = 0.01f; //How much time between each char printed
    private TextMeshProUGUI displayText;
    [SerializeField] private GameObject displayTextObj;
    private string introText = "";
    // Start is called before the first frame update

    private bool cutsceneStarted = false;
    void Start()
    {
        // Grab the component from our object
        displayText = displayTextObj.GetComponent<TextMeshProUGUI>();

        //Store our text for printing
        introText = displayText.text;

        //Set our text element blank
        displayText.text = "";

        //Disable the game object for later
        displayTextObj.SetActive(false);
    }

    void Update()
    {
        // This is not a very robust solution, but given the scope of the project it is an acceptable hack
        if (cutsceneStarted && Input.anyKey) 
        {
            SkipCutScene();
        }
    }

    private IEnumerator StartCutscene()
    {
        cutsceneStarted = true;
        //Turn off the buttons
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }
        // Fade to black
        float elapsedTime = 0;
        while (elapsedTime < fadeToBlackDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / fadeToBlackDuration);
            canvasBackground.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //set to zero
        canvasBackground.color = new Color(0, 0, 0, 1);

        displayTextObj.SetActive(true);

        foreach (char c in introText)
        {
            displayText.text += c;
            yield return new WaitForSeconds(timePerChar);
        }
    }

    public void StartOpeningCutscene()
    {
        Debug.Log("Starting cutscene");
        StartCoroutine(StartCutscene());
    }

    public void SkipCutScene()
    {
        SceneLoader.Instance.LoadGameScene(true);
    }
}
