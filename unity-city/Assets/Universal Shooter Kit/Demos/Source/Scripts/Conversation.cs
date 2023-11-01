using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour, IInteractable
{
    public List<string> _npcAnswerList;

    private InteractableObject _interactableObject;
    private Dialogue _dummyDialogue;

    private void Start()
    {
        _dummyDialogue = new Dialogue(_npcAnswerList);
        _interactableObject = GetComponent<InteractableObject>();
        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(OnInteract);
        }
    }

    public void OnInteract(string playerName, string objectName)
    {
        _interactableObject.ShowChatBox?.Invoke(_dummyDialogue, playerName, objectName);
    }
}
