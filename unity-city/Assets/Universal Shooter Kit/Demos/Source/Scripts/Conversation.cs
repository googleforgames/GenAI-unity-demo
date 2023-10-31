using System;
using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour, IInteractable
{
    [SerializeField] private ChatBoxController _chatBoxController;
    [SerializeField] private List<string> _dummyDialogue;

    private InteractableObject _interactableObject;
    private bool _isChatBoxShown = false;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(OnInteract);
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        throw new NotImplementedException();
    }

    public void OnInteract()
    {
        ShowChatBox(true);
    }

    private void ShowChatBox(bool showChatBox)
    {
        if (_isChatBoxShown == showChatBox)
        {
            return;
        }

        _isChatBoxShown = showChatBox;
        if (_chatBoxController)
        {
            _chatBoxController.gameObject.SetActive(showChatBox);
        }
    }
}
