using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float defaultDuration = 1.0f;

    private Coroutine currentAnimation;

    public void SetText(string finalText, float duration = -1)
    {
        if (duration < 0) duration = defaultDuration;

        // Stop any ongoing animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateText(text, finalText, duration));
    }

    private IEnumerator AnimateText(TextMeshProUGUI textElement, string finalText, float duration)
    {
        textElement.text = "";
        int totalCharacters = finalText.Length;

        // Guard against division by zero
        if (totalCharacters <= 0 || duration <= 0)
        {
            textElement.text = finalText;
            yield break;
        }

        // Calculate delay between characters
        float delayBetweenChars = duration / totalCharacters;

        for (int i = 0; i < totalCharacters; i++)
        {
            // Add the next character
            textElement.text += finalText[i];

            // Wait before showing the next character
            yield return new WaitForSeconds(delayBetweenChars);
        }

        currentAnimation = null;
    }
}
