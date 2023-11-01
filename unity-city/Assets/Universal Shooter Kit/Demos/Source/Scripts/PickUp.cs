using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableObject _interactableObject;

    public List<PickUp> Inventory = new List<PickUp>();

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(OnInteract);
        }
    }

    public void OnInteract(string playerName, string objectName)
    {
        Inventory.Add(this);
        gameObject.SetActive(false);
    }
}