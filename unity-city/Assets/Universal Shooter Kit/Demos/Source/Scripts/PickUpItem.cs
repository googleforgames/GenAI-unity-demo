using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public string Name => gameObject.name;
    public Sprite Icon;

    private InteractableObject _interactableObject;

    private int playersInteracting = 0;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (!_interactableObject)
        {
            return;
        }

        _interactableObject.SubscribeOnInteract(OnInteract);
    }

    public void OnInteract()
    {
        InteractionHandler.Instance.PutItemToInventory(this);
    }
}
