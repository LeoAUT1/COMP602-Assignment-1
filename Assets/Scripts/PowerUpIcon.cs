using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpIcon : MonoBehaviour
{
    [SerializeField] private Image _sprite;
    [SerializeField] private TextMeshProUGUI _count;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _desc;

    public void SetSprite(Sprite sprite)
    {
        if (_sprite == null)
        {
            Debug.LogError($"PowerUpIcon on '{gameObject.name}': _sprite (Image) is not assigned. Cannot set sprite.", gameObject);
            return;
        }
        Debug.Log($"Setting sprite on '{gameObject.name}' to: {(sprite != null ? sprite.name : "null")}");
        _sprite.sprite = sprite;
    }

    public void SetCount(string text)
    {
        if (_count == null)
        {
            Debug.LogError($"PowerUpIcon on '{gameObject.name}': _count (TextMeshProUGUI) is not assigned. Cannot set count text.", gameObject);
            return;
        }
        _count.text = text;
    }

    public void SetTitle(string text)
    {
        if (_title == null)
        {
            Debug.LogError($"PowerUpIcon on '{gameObject.name}': _title (TextMeshProUGUI) is not assigned. Cannot set title text.", gameObject);
            return;
        }
        _title.text = text;
    }

    public void SetDesc(string text)
    {
        if (_desc == null)
        {
            Debug.LogError($"PowerUpIcon on '{gameObject.name}': _desc (TextMeshProUGUI) is not assigned. Cannot set description text.", gameObject);
            return;
        }
        _desc.text = text;
    }
}
