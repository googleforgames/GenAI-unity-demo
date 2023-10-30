using UnityEngine;

public class Conversation : MonoBehaviour
{
    [SerializeField] private TextAsset _dummyDialogueJSON;

    private InteractableObject _interactableObject;
    private string _dialogue;

    private void Awake()
    {
        _interactableObject = GetComponent<InteractableObject>();
    }

    private void Start()
    {
        if (_dummyDialogueJSON)
        {
            _dialogue = JsonUtility.FromJson<string>(_dummyDialogueJSON.text);
        }

        if (_interactableObject)
        {
            _interactableObject.SubscribeOnInteract(ShowChatBox);
        }
        else
        {
            Debug.LogError($"The IteractableObject script must be attached to the {gameObject.name} game object.");
        }
    }

    private void ShowChatBox()
    {
        Debug.Log("Show Chat Box!");
    }
}
