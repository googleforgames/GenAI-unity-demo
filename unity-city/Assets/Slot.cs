using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public bool IsOccupied { get; private set; } = false;

    private Button _button;
    private Image _icon;
    private PickUpItem _storedItem;

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void Setup()
    {
        _icon = GetComponent<Image>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(UseItem);
        _button.interactable = false;
    }

    public void PutItem(PickUpItem item)
    {
        _storedItem = item;
        _icon.sprite = item.Icon;
        _button.interactable = true;
        IsOccupied = true;
    }

    private void UseItem()
    {
        _storedItem.gameObject.SetActive(true);
        Clean();
    }

    private void Clean()
    {
        _icon.sprite = null;
        _storedItem = null;
        _button.interactable = false;
        IsOccupied = false;
    }
}
