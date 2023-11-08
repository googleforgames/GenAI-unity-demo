using GercStudio.USK.Scripts;
using TMPro;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public static InteractionHandler Instance { get ; private set; }

    [SerializeField] private TextMeshProUGUI _interactionCallout;
    [SerializeField] private InventoryDemo _inventory;

    private GameManager _gameManager;
    private ChatManager _chatManager;

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
        _gameManager = GetComponent<GameManager>();
        _chatManager = GetComponent<ChatManager>();
        _inventory.SetupSlots();
    }

    private void Update()
    {
        if (_currentInteractionObject && _currentInteractionObject.CanInteract && !IsChatBoxShown())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _currentInteractionObject.OnInteract();
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                _currentInteractionObject.OnRelease();
            }
        }

        if (!IsChatBoxShown())
        {
            if (Input.GetKeyUp(KeyCode.I))
            {
                ToggleInventory();
            }
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

    public bool IsInventoryShown()
    {
        return _inventory.gameObject.activeInHierarchy;
    }

    public void PutItemToInventory(PickUpItem item)
    {
        Debug.Log($"{item.Name} is added to the inventory");

        _inventory.PutItem(item);
        item.gameObject.SetActive(false);
    }

    public void ToggleInventory()
    {
        var inventory = _inventory.gameObject;
        inventory.SetActive(!inventory.activeInHierarchy);
        _gameManager.Pause(false);
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
}
