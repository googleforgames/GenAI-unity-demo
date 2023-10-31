using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private ChatBoxController _chatBoxController;
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();
    // TODO: move to the UIManager
    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private bool _isHintShown = false;

    private void Start()
    {
        if (!_interactableObjects.Any())
        {
            return;
        }

        foreach (var io in _interactableObjects)
        {
            io.SubscribeOnShowHint(ShowInteractionCallout);
            io.SubscribeOnShowChatBox(ShowChatBox);
        }
    }

    public bool IsChatBoxShown()
    {
        return _chatBoxController && _chatBoxController.gameObject.activeInHierarchy;
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

    public void ShowChatBox()
    {
        if (_chatBoxController)
        {
            _chatBoxController.gameObject.SetActive(true);
        }
    }

    public void HideChatBox()
    {
        if (_chatBoxController)
        {
            _chatBoxController.gameObject.SetActive(false);
        }
    }
}
