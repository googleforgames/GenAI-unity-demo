using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();

    // TODO: move to the UIManager
    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private Dictionary<string, PickUpItem> _inventory = new Dictionary<string, PickUpItem>();
    private ChatManager _chatManager;
    private bool _isHintShown = false;

    private void Start()
    {
        _chatManager = GetComponent<ChatManager>();

        if (!_interactableObjects.Any())
        {
            return;
        }

        foreach (var io in _interactableObjects)
        {
            io.SubscribeOnShowHint(ShowInteractionCallout);
            io.SubscribeOnShowChatBox(_chatManager.OnShowChatBox, this);
        }
    }

    public bool IsChatBoxShown()
    {
        return _chatManager.IsChatOpen;
    }

    public void HideChatBox()
    {
        _chatManager.OnHideChatBox();
    }

    public void PutItemToInventory(PickUpItem item)
    {
        Debug.Log($"{item.Name} is added to the inventory");
        _inventory.Add(item.Name, item);

        // TODO: implement UI stuff
    }

    public void UseItemFromInventory(PickUpItem item)
    {
        Debug.Log($"{item.Name} from the inventory is used");
        _inventory.Remove(item.Name);

        // TODO: add the logic of usage
    }

    private void ShowInteractionCallout(bool showHint)
    {
        if (_isHintShown == showHint)
        {
            return;
        }

        _isHintShown = showHint;
        if (_interactionCallout)
        {
            _interactionCallout.gameObject.SetActive(showHint);
        }
    }
}
