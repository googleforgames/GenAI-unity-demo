using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour, IInteractable
{
    [SerializeField] private List<string> _dummyDialogue;

    private InteractableObject _interactableObject;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(OnInteract);
        }
    }

    public void OnInteract()
    {
        _interactableObject.ShowChatBox?.Invoke();
    }
}
