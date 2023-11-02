using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public string Name => _interactableObject.ObjectName;

    private InteractableObject _interactableObject;
    private GameManagerExtension _gameManager;

    private int playersInteracting = 0;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (!_interactableObject)
        {
            return;
        }

        _interactableObject.SubscribeOnInteract(OnInteract);
        _gameManager = _interactableObject.GameManager;
    }

    public void OnInteract(string playerName, string objectName)
    {
        _gameManager.PutItemToInventory(this);
        gameObject.SetActive(false);
    }
}
