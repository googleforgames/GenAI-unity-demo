using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using GercStudio.USK.Scripts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private string _playerName = "Diego";

    public GameObject chatBox;

    public bool IsChatOpen { get; private set; } = false;

    private GameManager _gameManager;
    private ChatBoxController _chatBoxController;
    private string _endpointUrl = "http://34.83.85.41/npc";

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

        SetUri();
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

    private void SetUri()
    {
        var uri = VarManager.varEndpointURI_LLM;
        if (string.IsNullOrEmpty(uri))
        {
            return;
        }

        _endpointUrl = "http://" + uri + "/npc";
        Debug.Log($"The LLM URI is changed to {_endpointUrl}");
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
        SendJsonAsync(question);
    }

    private async void SendJsonAsync(string text)
    {
        // Create a dictionary to hold the key-value pair
        Dictionary<string, string> jsonPayload = new()
        {
            ["prompt"] = text
        };

        // Convert the dictionary to JSON format
        string jsonPayloadStr = JsonConvert.SerializeObject(jsonPayload);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonPayloadStr);

        // Create a new HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            // Set the request content type to JSON
            Content = new StringContent(jsonPayloadStr, Encoding.UTF8, "application/json")
        };

        // Send the request
        using var client = new HttpClient();
        HttpResponseMessage response = await client.SendAsync(request);
        Debug.Log("Response status code: " + response.StatusCode);
        Debug.Log("Response content: " + await response.Content.ReadAsStringAsync());
        _chatBoxController.ShowNPCAnswer(await response.Content.ReadAsStringAsync());
    }

    private IEnumerator GetAnswer(string question)
    {
        string jsonPayload = JsonUtility.ToJson(new { prompt = question });
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new (_endpointUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            float time1 = Time.realtimeSinceStartup;
            yield return webRequest.SendWebRequest();
            float time2 = Time.realtimeSinceStartup;

#pragma warning disable CS0618 // Type or member is obsolete
            Debug.Log("Request seconds: " + (time2 - time1) + "\nisError: " + webRequest.isNetworkError + " error: " + webRequest.error + "\nresponseCode: " + webRequest.responseCode + "\nresponseContent: " + webRequest.downloadHandler.text);
#pragma warning restore CS0618 // Type or member is obsolete

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

                Debug.Log($"Text from download handler: " + webRequest.downloadHandler.text);
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