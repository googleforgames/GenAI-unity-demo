using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class ChatBoxController : MonoBehaviour
{
    public Dialogue DummyDialogue { get; set; }

    [SerializeField] private TextMeshProUGUI _npcName;
    [SerializeField] private TextMeshProUGUI _lastNPCReplicLabel;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _lastPlayerReplicLabel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _closeButton;

    private string _lastNPCReplic = "";
    private string _lastPlayerReplic = "";

    private bool _isInputEnabled = false;
    private int _nextAnswerIndex = 0;

    private void Start()
    {
        _inputField.onValueChanged.AddListener(SendMessageToChat);
        _closeButton.onClick.AddListener(OnCloseButtonPressed);
    }

    private void OnEnable()
    {
        EnableInput();
    }

    private void Update()
    {
        // TODO Input:
        if (_isInputEnabled && Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextAnswer();
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

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
        _inputField.onValueChanged.RemoveAllListeners();
    }

    private void ShowNextAnswer()
    {
        _lastNPCReplicLabel.text = DummyDialogue.AnswerList[_nextAnswerIndex++];
    }

    private void OnCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Must be called when the chat box is shown
    /// </summary>
    /// <param name="npcName"></param>
    /// <param name="playerName"></param>
    public void Setup(string npcName, string playerName)
    {
        if (!string.IsNullOrEmpty(npcName))
        {
            _npcName.text = npcName;
        }

        if (!string.IsNullOrEmpty(playerName))
        {
            _playerName.text = playerName;
        }
    }

    private void EnableInput()
    {
        _isInputEnabled = true;
        ShowUIElement(_inputField.gameObject);
        HideUIElement(_lastPlayerReplicLabel.gameObject);
    }

    private void DisableInput()
    {
        _isInputEnabled = false;
        HideUIElement(_inputField.gameObject);
        ShowUIElement(_lastPlayerReplicLabel.gameObject);
    }

    private void ShowUIElement(GameObject uiElement)
    {
        uiElement.SetActive(true);
    }

    private void HideUIElement(GameObject uiElement)
    {
        uiElement.SetActive(false);
    }

    private void SendMessageToChat(string messageText)
    {
        _lastPlayerReplicLabel.text = messageText;
        DisableInput();
    }}
