using System.Collections.Generic;
using System.Linq;
using GercStudio.USK.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();

    // TODO: move to the UIManager
    [FormerlySerializedAs("_chatBoxController")] [SerializeField] private ChatBoxController _chatBox;
    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private GameManager _gameManager;
    private Dialogue _dummyDialogue;
    private bool _isHintShown = false;

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();
        _chatBox.SubscribeOnCloseButton(HideChatBox);

        if (!_interactableObjects.Any())
        {
            return;
        }

        foreach (var io in _interactableObjects)
        {
            io.SubscribeOnShowHint(ShowInteractionCallout);
            io.SubscribeOnShowChatBox(OnShowChatBox, this);
        }
    }

    private void Update()
    {
        // TODO Input:
        if (_chatBox.IsInputEnabled && _chatBox.HasPlayerInput && Input.GetKeyDown(KeyCode.Return))
        {
            _chatBox.SendMessageToChat(_dummyDialogue.GetAnswer(""));
        }
        // + When opened, the chat box takes input from the keyboard to write the text of what the player is saying.
        // Pressing enter ends the line and sends the text and waits for the reply.
        // When the text is sent, it is shown at the bottom with the following format:
        // PlayerName: TEXT.
        // The reply from the alien is shown at the top of the chat box with the following format:
        // AlienName: RESPONSE.
            
        // Text box showing the text written by the player.
        // Previous player message shown above the text box.
        // Top of the Conversation widget:
        // Last NPC message (As shown in the following mockup)

    }

    public bool IsChatBoxShown()
    {
        return _chatBox && _chatBox.gameObject.activeInHierarchy;
    }

    public void HideChatBox()
    {
        _gameManager.Pause(false);
        if (_chatBox)
        {
            _chatBox.gameObject.SetActive(false);
        }
    }

    private void ShowInteractionCallout(bool showHint)
    {
        if (_isHintShown == showHint)
        {
            return;
        }

        _isHintShown = showHint;
        if (_interactionCallout)
        {
            _interactionCallout.gameObject.SetActive(showHint);
        }
    }

    private void OnShowChatBox(Dialogue dialogue, string playerName, string npcName)
    {
        if (!_chatBox)
        {
            return;
        }

        _gameManager.Pause(false);
        _dummyDialogue = dialogue;
        _chatBox.SetNames(playerName, npcName);
        _chatBox.gameObject.SetActive(true);
    }
}
