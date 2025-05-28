using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PowerUpIcon : MonoBehaviour
{
    [SerializeField] private Image _sprite;
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    public void SetSprite(Sprite sprite)
    {
        Debug.Log(sprite);
        _sprite.sprite = sprite;
    }


    public void SetText(string text)
    {
        _textMeshPro.text = text;
    }
}
