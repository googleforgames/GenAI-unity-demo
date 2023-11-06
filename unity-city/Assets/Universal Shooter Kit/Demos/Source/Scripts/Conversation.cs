using UnityEngine;

public class Conversation : MonoBehaviour, IInteractable
{
    private InteractableObject _interactableObject;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(OnInteract);
        }
    }

    // TODO: make an action registration and unregistration in the 
    // InteractionHandler class (openChatBox, pickUpObject, playAnimation, etc.)
    public void OnInteract(string playerName, string objectName)
    {
        _interactableObject.ShowChatBox?.Invoke(playerName, objectName);
    }
}
