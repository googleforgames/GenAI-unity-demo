using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour, IInteractable
{
    public List<string> _npcAnswerList;

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
        var dummyDialogue = new Dialogue("", "", _npcAnswerList);
        _interactableObject.ShowChatBox?.Invoke(dummyDialogue);
    }
}
