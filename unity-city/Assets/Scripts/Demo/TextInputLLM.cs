//"https://us-central1-stealth-air-23412.cloudfunctions.net/text-endpoint"

using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;
using TMPro;

public class TextInputLLM : MonoBehaviour
{
    public TMP_InputField inputField;

    private const string EndpointURL = "https://us-central1-stealth-air-23412.cloudfunctions.net/text-endpoint";

    // Use this for initialization
    private void Start()
    {
        //inputField.onValidateInput.AddListener(SendJson);
        inputField.onEndEdit.AddListener(SendJsonAsync);
    }

    private async void SendJsonAsync(string text)
    {
        //JsonObject jsonPayload = new JsonObject();
        //jsonPayload["prompt"] = text;

        // Create a dictionary to hold the key-value pair
        Dictionary<string, string> jsonPayload = new Dictionary<string, string>();
        jsonPayload["prompt"] = text;

        // Convert the dictionary to JSON format
        string jsonPayloadStr = JsonConvert.SerializeObject(jsonPayload);

        // Create a new HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, EndpointURL);

        // Set the request content type to JSON
        request.Content = new StringContent(jsonPayloadStr, Encoding.UTF8, "application/json");

        // Send the request
        using (var client = new HttpClient())
        {
            HttpResponseMessage response = await client.SendAsync(request);
            Debug.Log("Response status code: " + response.StatusCode);
            Debug.Log("Response content: " + await response.Content.ReadAsStringAsync());
        }

        // Clear the text input
        inputField.text = "";

    }
}
