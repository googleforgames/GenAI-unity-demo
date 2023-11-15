using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    [FormerlySerializedAs("serverIpField")] [SerializeField]
    private TMP_InputField _serverIpField;

    [FormerlySerializedAs("serverPortField")] [SerializeField]
    private TMP_InputField _serverPortField;

    private void Start()
    {
        if (ClientSingleton.Instance == null)
        {
            return;
        }

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        _serverIpField.text = PlayerPrefs.GetString("LastIp");
        _serverPortField.text = PlayerPrefs.GetString("LastPort");
    }

    public void StartClient()
    {
        string ip = _serverIpField.text;
        string port = _serverPortField.text;
        
        PlayerPrefs.SetString("LastIp", ip);
        PlayerPrefs.SetString("LastPort", port);
        
        ClientSingleton.Instance.GameManager.StartClient(ip, int.Parse(port));
    }
}