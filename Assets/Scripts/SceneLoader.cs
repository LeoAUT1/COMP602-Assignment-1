using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : Singleton<SceneLoader>
{

    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fadeToBlackImage;
    [SerializeField] private float fadeDuration = 0.3f;

    private void Awake()
    {
        base.Awake(); // Call Singleton's Awake

        if (Instance != this)
        {
            return;
        }

        // Create fade canvas and image if not assigned
        if (canvas == null)
        {
            canvas = new GameObject("canvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Make sure it renders on top

            fadeToBlackImage = new GameObject("fadeToBlackImage").AddComponent<Image>();
            fadeToBlackImage.transform.SetParent(canvas.transform, false);
            fadeToBlackImage.color = Color.black;
            fadeToBlackImage.rectTransform.anchorMin = Vector2.zero;
            fadeToBlackImage.rectTransform.anchorMax = Vector2.one;
            fadeToBlackImage.rectTransform.sizeDelta = Vector2.zero;

            // Start with transparent black
            Color transparent = Color.black;
            transparent.a = 0;
            fadeToBlackImage.color = transparent;

            DontDestroyOnLoad(canvas.gameObject);
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        //Fade out our music
        AudioManager.Instance.FadeOutMusic(fadeDuration);

        // Fade to black
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeToBlackImage.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we're fully black
        fadeToBlackImage.color = Color.black;

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // Fade back in
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeToBlackImage.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we're fully transparent
        fadeToBlackImage.color = new Color(0, 0, 0, 0);
    }

    public void LoadGameScene(bool newGame = false)
    {
        Debug.Log("attempting to load main game scene");
        if (newGame)
        {
            Debug.Log("Is newgame reset the player");
            Player.Instance.ResetPlayer();
        }

        StartCoroutine(FadeAndLoadScene("GameBoard"));
    }

    public void LoadCombatScene()
    {
        StartCoroutine(FadeAndLoadScene("Combat"));
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameEnd()
    {

        StartCoroutine(FadeAndLoadScene("GameEnd"));
    }
}
