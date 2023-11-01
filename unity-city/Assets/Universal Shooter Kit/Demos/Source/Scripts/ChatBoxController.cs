using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class ChatBoxController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _npcNameLabel;
    [SerializeField] private TextMeshProUGUI _lastNPCReplicLabel;
    [SerializeField] private TextMeshProUGUI _playerNameLabel;
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

    public void ShowNPCAnswer(string answer)
    {
        _lastNPCReplicLabel.text = answer;
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
