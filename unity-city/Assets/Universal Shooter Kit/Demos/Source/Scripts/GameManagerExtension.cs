using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();

    // TODO: move to the UIManager
    [SerializeField] private ChatBoxController _chatBoxController;
    [SerializeField] private TextMeshProUGUI _interactionCallout;
    [SerializeField] private float _minAnswerDelay = 0.2f;
    [SerializeField] private float _maxAnswerDelay = 0.9f;

    private Dialogue _dummyDialogue;
    private bool _isHintShown = false;

    private void Start()
    {
        if (!_interactableObjects.Any())
        {
            return;
        }

        foreach (var io in _interactableObjects)
        {
            io.SubscribeOnShowHint(ShowInteractionCallout);
            io.SubscribeOnShowChatBox(OnShowChatBox);
        }
    }

    private void Update()
    {
        // TODO Input:
        if (_chatBoxController.IsInputEnabled && Input.GetKeyDown(KeyCode.Return))
        {
            var answerDelay = Random.Range(_minAnswerDelay, _maxAnswerDelay);
            StartCoroutine(ShowAnswerCoroutine("", answerDelay));
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

    private IEnumerator ShowAnswerCoroutine(string question, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _chatBoxController.ShowNPCAnswer(_dummyDialogue.GetAnswer(question));
    }

    public bool IsChatBoxShown()
    {
        return _chatBoxController && _chatBoxController.gameObject.activeInHierarchy;
    }

    public void HideChatBox()
    {
        if (_chatBoxController)
        {
            _chatBoxController.gameObject.SetActive(false);
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
        if (!_chatBoxController)
        {
            return;
        }

        _dummyDialogue = dialogue;
        _chatBoxController.SetNames(playerName, npcName);
        _chatBoxController.gameObject.SetActive(true);
    }
}
