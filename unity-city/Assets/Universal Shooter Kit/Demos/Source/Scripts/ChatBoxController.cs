using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Button = UnityEngine.UI.Button;

public class ChatBoxController : MonoBehaviour
{
    [FormerlySerializedAs("_npcName")] [SerializeField] private TextMeshProUGUI _npcNameLabel;
    [SerializeField] private TextMeshProUGUI _lastNPCReplicLabel;
    [FormerlySerializedAs("_playerName")] [SerializeField] private TextMeshProUGUI _playerNameLabel;
    [SerializeField] private TextMeshProUGUI _lastPlayerReplicLabel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _closeButton;

    public bool IsInputEnabled { get; private set; } = false;

    private void Start()
    {
        _inputField.onValueChanged.AddListener(SendMessageToChat);
        _closeButton.onClick.AddListener(OnCloseButtonPressed);
    }

    private void OnEnable()
    {
        _lastNPCReplicLabel.text = string.Empty;
        _lastPlayerReplicLabel.text = string.Empty;
        EnableInput();
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
        _inputField.onValueChanged.RemoveAllListeners();
    }

    public void SetNames(string playerName, string objectName)
    {
        _playerNameLabel.text = playerName;
        _npcNameLabel.text = objectName;
    }

    public void ShowAnswer(string answer)
    {
        _lastNPCReplicLabel.text = answer;
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
            _npcNameLabel.text = npcName;
        }

        if (!string.IsNullOrEmpty(playerName))
        {
            _playerNameLabel.text = playerName;
        }
    }

    private void EnableInput()
    {
        IsInputEnabled = true;
        ShowUIElement(_inputField.gameObject);
        HideUIElement(_lastPlayerReplicLabel.gameObject);
    }

    private void OnCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }

    private void DisableInput()
    {
        IsInputEnabled = false;
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
