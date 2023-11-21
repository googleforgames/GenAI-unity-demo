using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;

public class ChatBoxController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _npcNameLabel;
    [SerializeField] private TextMeshProUGUI _lastNPCReplicLabel;
    [SerializeField] private TextMeshProUGUI _playerOutputLabel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] public Button _closeButton;

    private string _playerName;

    private void OnEnable()
    {
        _lastNPCReplicLabel.text = string.Empty;
        _playerOutputLabel.text = string.Empty;
        EnableInput();
    }

    private void OnDestroy()
    {
        _inputField.onSubmit.RemoveAllListeners();
        _closeButton.onClick.RemoveAllListeners();
    }

    public void SubscribeOnMessageSent(UnityAction<string> action)
    {
        _inputField.onSubmit.AddListener(action);
    }

    public void SubscribeOnCloseButton(UnityAction action)
    {
        _closeButton.onClick.AddListener(action);
    }

    public void SetNames(string playerName, string objectName)
    {
        _playerName = playerName;
        _playerOutputLabel.text = playerName;
        _npcNameLabel.text = objectName;
    }

    public void SendMessageToChat(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        var playerMessage = _playerName + ": " + message;
        PostPlayerMessage(playerMessage);
        DisableInput();
    }

    public void ShowNPCAnswer(string answer)
    {
        Debug.Log("Added Answer to chatbox");
        _lastNPCReplicLabel.text = answer;
        EnableInput();
    }

    private void EnableInput()
    {
        ShowUIElement(_inputField.gameObject);
        _inputField.text = "";
        _inputField.Select();
    }

    private void DisableInput()
    {
        _inputField.DeactivateInputField();
        HideUIElement(_inputField.gameObject);
    }

    private void PostPlayerMessage(string message)
    {
        _playerOutputLabel.text = message;
    }

    private void ShowUIElement(GameObject uiElement)
    {
        uiElement.SetActive(true);
    }

    private void HideUIElement(GameObject uiElement)
    {
        uiElement.SetActive(false);
    }
}
