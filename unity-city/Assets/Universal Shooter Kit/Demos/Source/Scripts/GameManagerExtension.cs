using System.Collections.Generic;
using System.Linq;
using GercStudio.USK.Scripts;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();

    // TODO: move to the UIManager
    [SerializeField] private ChatBoxController _chatBox;
    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private Dictionary<string, PickUpItem> _inventory = new Dictionary<string, PickUpItem>();
    private GameManager _gameManager;
    private Dialogue _dummyDialogue;
    private bool _isHintShown = false;

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();
        _chatBox.SubscribeOnCloseButton(HideChatBox);

        if (!_interactableObjects.Any())
        {
            return;
        }

        foreach (var io in _interactableObjects)
        {
            io.SubscribeOnShowHint(ShowInteractionCallout);
            io.SubscribeOnShowChatBox(OnShowChatBox, this);
        }
    }

    private void Update()
    {
        // TODO Input:
        if (_chatBox.IsInputEnabled && _chatBox.HasPlayerInput && Input.GetKeyDown(KeyCode.Return))
        {
            _chatBox.SendMessageToChat(_dummyDialogue.GetAnswer(""));
        }
    }

    public bool IsChatBoxShown()
    {
        return _chatBox && _chatBox.gameObject.activeInHierarchy;
    }

    public void HideChatBox()
    {
        _gameManager.Pause(false);
        if (_chatBox)
        {
            _chatBox.gameObject.SetActive(false);
        }
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

    private void OnShowChatBox(Dialogue dialogue, string playerName, string npcName)
    {
        if (!_chatBox)
        {
            return;
        }

        _gameManager.Pause(false);
        _dummyDialogue = dialogue;
        _chatBox.SetNames(playerName, npcName);
        _chatBox.gameObject.SetActive(true);
    }
}
