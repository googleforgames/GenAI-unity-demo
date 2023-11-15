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

    public void OnInteract()
    {
        InteractionHandler.Instance.ShowChatBox();
    }
}
