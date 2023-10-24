using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerID : MonoBehaviour
{
    public string PlayerID;

    public static string playerIDGlobal;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Playing as PlayerID: " + PlayerID);
        playerIDGlobal = PlayerID;
    }

}
