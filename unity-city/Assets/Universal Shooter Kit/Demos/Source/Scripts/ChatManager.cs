using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using GercStudio.USK.Scripts;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private string _playerName = "Diego";

    public GameObject chatBox;

    public bool IsChatOpen { get; private set; } = false;

    private GameManager _gameManager;
    private ChatBoxController _chatBoxController;
    private string endpointUrl = "http://34.105.107.165/npc";

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();
        _chatBoxController = chatBox.GetComponent<ChatBoxController>();
        if (!_chatBoxController)
        {
            return;
        }

        _chatBoxController.SubscribeOnMessageSent(OnSendQuestion);
        _chatBoxController.SubscribeOnCloseButton(OnHideChatBox);
    }

    public void OnShowChatBox(string playerName = "", string npcName = "")
    {
        if (!_chatBoxController)
        {
            return;
        }

        _gameManager.Pause(false);

        IsChatOpen = true;

        // TODO: decide where to take the player's name from
        _chatBoxController.SetNames(_playerName, npcName);
        ShowChatBox(true);
    }

    public void OnHideChatBox()
    {
        if (chatBox)
        {
            ShowChatBox(false);
        }

        IsChatOpen = false;
        _gameManager.Pause(false);
    }

    private void ShowChatBox(bool show)
    {
        _chatBoxController.gameObject.SetActive(show);
    }

    private void OnSendQuestion(string question)
    {
        if (string.IsNullOrEmpty(question))
        {
            return;
        }

        _chatBoxController.SendMessageToChat(question);
        StartCoroutine(GetAnswer(question));
    }

    private IEnumerator GetAnswer(string question)
    {
        string jsonPayload = JsonUtility.ToJson(new { prompt = question });
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(endpointUrl, "POST"))
        {
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            // catch error - this is fine for now, but more code needed here to handle errors.
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                var errorMessage = webRequest.error;
                Debug.LogError("[ChatManager.GetAnswer: " + errorMessage);

                _chatBoxController.ShowNPCAnswer("Sorry, my system is broken:\n" + errorMessage);
                // respond with some appropriate response on error. (I need to do add additional code here)
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<ResponseData>(webRequest.downloadHandler.text);
                _chatBoxController.ShowNPCAnswer(jsonResponse.response);
                // chatOutput.text += "\n" + jsonResponse.response;
            }
        }
    }

    [System.Serializable]
    private class ResponseData
    {
        public string response;
    }
}