using UnityEngine;

public class PickUpItemBase : MonoBehaviour, IInteractable, IUsable
{
    public string Name => gameObject.name;
    public Sprite Icon;

    protected InteractableObject _interactableObject;

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

    public void Use()
    {
        // Implement some logic here or define it in the descendant classes
    }
}