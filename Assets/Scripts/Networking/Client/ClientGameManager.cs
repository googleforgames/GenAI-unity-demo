using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private const string GameSceneName = "Single Demo";
    private const string MainMenuSceneName = "MainMenu";

    public void GoToGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void StartClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);

        ConnectClient();
    }

    private async Task ConnectClient()
    {
        bool isStarted = NetworkManager.Singleton.StartClient();
        if (isStarted)
        {
            // GoToGame();
            Debug.Log("Client connection started.");
        }
        else
        {
            Debug.LogError("Client wasn't started when expected.");
        }
    }


    public void Dispose()
    {
    }
}