using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatEntityHud : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI levelText;
    public Slider hpSlider;

    [SerializeField] private float hpAnimationDuration = 0.5f;

    private Coroutine hpAnimationCoroutine;

    public void SetHUD(CombatEntity entity)
    {
        nameText.text = entity.GetName();

        int maxHp = entity.GetMaxHealth();
        int hp = entity.GetHealth();
	int level = entity.GetLevel();

        hpSlider.maxValue = maxHp;
        hpSlider.value = hp;

        UpdateHealthText(hp, maxHp);
        levelText.text = $"LV. {level}";
    }

    public void SetHP(int hp, int maxHp)
    {
        // Stop any ongoing animation
        if (hpAnimationCoroutine != null)
        {
            StopCoroutine(hpAnimationCoroutine);
        }

        // Start new animation
        hpAnimationCoroutine = StartCoroutine(AnimateHPSlider(hp, maxHp));
    }

    private IEnumerator AnimateHPSlider(int targetHp, int maxHp)
    {
        float startValue = hpSlider.value;
        float elapsedTime = 0f;

        UpdateHealthText(targetHp, maxHp);

        while (elapsedTime < hpAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hpAnimationDuration;
            hpSlider.value = Mathf.Lerp(startValue, targetHp, t);
            yield return null;
        }

        // Ensure we end at exactly the target value
        hpSlider.value = targetHp;
        hpAnimationCoroutine = null;
    }

    private void UpdateHealthText(int hp, int maxHp)
    {
        healthText.text = $"HP: {hp}/{maxHp}";
    }
}
