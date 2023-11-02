using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class ChatBoxController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _npcNameLabel;
    [SerializeField] private TextMeshProUGUI _lastNPCReplicLabel;
    [SerializeField] private TextMeshProUGUI _playerNameLabel;
    [SerializeField] private TextMeshProUGUI _lastPlayerReplicLabel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] public Button _closeButton;

    [SerializeField] private float _minAnswerDelay = 0.2f;
    [SerializeField] private float _maxAnswerDelay = 0.9f;

    private string _playerInput;

    public bool IsInputEnabled { get; private set; } = false;
    public bool HasPlayerInput { get; private set; } = false;

    private void Start()
    {
        _inputField.onValueChanged.AddListener(UpdatePlayerInput);
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

    public void SubscribeOnCloseButton(UnityAction action)
    {
        _closeButton.onClick.AddListener(action);
    }

    public void SetNames(string playerName, string objectName)
    {
        _playerNameLabel.text = "Diego";
        _npcNameLabel.text = objectName;
    }

    public void SendMessageToChat(string answer)
    {
        _lastPlayerReplicLabel.text = _playerInput;
        DisableInput();

        StartCoroutine(ShowNPCAnswer(answer));
    }

    private IEnumerator ShowNPCAnswer(string answer)
    {
        yield return new WaitForSecondsRealtime(Random.Range(_minAnswerDelay, _maxAnswerDelay));
        Debug.Log("Added Answer to chatbox");
        _lastNPCReplicLabel.text = answer;
        EnableInput();
    }

    private void EnableInput()
    {
        IsInputEnabled = true;
        _playerInput = "";
        _inputField.text = "";
        _inputField.ActivateInputField();
        ShowUIElement(_inputField.gameObject);
        HideUIElement(_lastPlayerReplicLabel.gameObject);
    }

    private void DisableInput()
    {
        IsInputEnabled = false;
        _inputField.DeactivateInputField();
        HideUIElement(_inputField.gameObject);
        ShowUIElement(_lastPlayerReplicLabel.gameObject);
    }

    private void OnCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }

    private void ShowUIElement(GameObject uiElement)
    {
        uiElement.SetActive(true);
    }

    private void HideUIElement(GameObject uiElement)
    {
        uiElement.SetActive(false);
    }

    private void UpdatePlayerInput(string input)
    {
        _playerInput = input;
        HasPlayerInput = !string.IsNullOrEmpty(input);
    }
}
