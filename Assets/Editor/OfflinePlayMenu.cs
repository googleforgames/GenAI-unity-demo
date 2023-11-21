using UnityEditor;
using UnityEngine;

public class OfflinePlayMenu : MonoBehaviour
{
    [MenuItem("Tools/Offline testing/Enable offline testing", priority=-1000)]
    private static void EnableOfflinePlay()
    {
        PlayerPrefs.SetInt("EnableOfflinePlay", 1);
        Debug.Log("Offline testing enabled. Now you can run any scene directly.");
    }

    [MenuItem("Tools/Offline testing/Disable offline testing", priority=-1000)]
    private static void DisableOfflinePlay()
    {
        PlayerPrefs.SetInt("EnableOfflinePlay", 0);
        Debug.Log("Offline testing enabled. Running the game will go through the network bootstrap scene.");
    }
}