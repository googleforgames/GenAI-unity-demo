using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public static InteractionHandler Instance { get ; private set; }

    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private ChatManager _chatManager;

    private Dictionary<string, PickUpItem> _inventory = new Dictionary<string, PickUpItem>();
    private InteractableObject _currentInteractionObject;

    private string _interactorName;
    private string _interactionObjectName;

    private bool _isCalloutShown = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _chatManager = GetComponent<ChatManager>();
    }

    private void Update()
    {
        if (!_currentInteractionObject || !_currentInteractionObject.CanInteract || IsChatBoxShown())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _currentInteractionObject.OnInteract();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            _currentInteractionObject.OnRelease();
        }
    }

    public void RegisterInteractableObject(InteractableObject interactableObject)
    {
        
    }

    private void SubscribeToInteraction(IInteractable interactable)
    {
        switch (interactable)
        {
            case AnimationPlayer:
                break;
            case Conversation:
                break;
            case PickUpItem:
                break;
        }
    }

    public void OnPlayerEntered(Collider player, InteractableObject interactableObject)
    {
        Debug.Log($"{_interactorName} entered interaction area of {_interactionObjectName}");

        _interactorName = player.name;
        _interactionObjectName = interactableObject.name;
        _currentInteractionObject = interactableObject;

        var calloutText = _currentInteractionObject.InteractionCalloutText;
        if (!string.IsNullOrEmpty(calloutText))
        {
            _interactionCallout.text = calloutText;
        }
        else
        {
            Debug.LogWarning($"Interaction callout text is null or empty for {_interactionObjectName}");
        }

        ShowInteractionCallout(true);
    }
    public void OnPlayerExited()
    {
        Debug.Log($"{_interactorName} exited interaction area of {_interactionObjectName}");

        _interactorName = "";
        _interactionObjectName = "";
        _currentInteractionObject = null;
        ShowInteractionCallout(false);
    }

    private void ShowInteractionCallout(bool show)
    {
        if (_isCalloutShown == show)
        {
            return;
        }

        _isCalloutShown = show;
        if (_interactionCallout)
        {
            _interactionCallout.gameObject.SetActive(show);
        }
    }

    public void ShowChatBox()
    {
        _chatManager.OnShowChatBox(_interactorName, _interactionObjectName);
    }

    public void HideChatBox()
    {
        _chatManager.OnHideChatBox();
    }

    public bool IsChatBoxShown()
    {
        return _chatManager.IsChatOpen;
    }

    public void PutItemToInventory(PickUpItem item)
    {
        Debug.Log($"{item.Name} is added to the inventory");
        _inventory.Add(item.Name, item);

        item.gameObject.SetActive(false);
    }

    public void UseItemFromInventory(PickUpItem item)
    {
        Debug.Log($"{item.Name} from the inventory is used");
        _inventory.Remove(item.Name);

        // TODO: add the logic of usage
    }
}
