
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DynamicText : MonoBehaviour 
{
    public TMP_Text textObject;
    public string TextMessage;
    
    public GameObject objectToActivate1;
    public GameObject objectToActivate2;
    public GameObject objectToActivate3;
    public GameObject objectToActivate4;
    public GameObject objectToActivate5;
    public GameObject objectToActivate6;
    public GameObject objectToActivate7;
    public GameObject objectToActivate8;
    public GameObject objectToActivate9;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "swat:Head")
        {
            StartCoroutine(DelayUntilNotification(0.1f));
        }
    }

    IEnumerator DelayUntilNotification(float delay)
    {
        yield return new WaitForSeconds(delay);

        textObject.text = TextMessage;
        
        objectToActivate1.SetActive(true);
        objectToActivate2.SetActive(true);
        objectToActivate3.SetActive(true);
        objectToActivate4.SetActive(true);
        objectToActivate5.SetActive(true);
        objectToActivate6.SetActive(true);
        objectToActivate7.SetActive(true);
        objectToActivate8.SetActive(true);
        objectToActivate9.SetActive(true);

        // Display Notification for X seconds
        StartCoroutine(DeactivateAfterDelay(7f));

    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Deactivate the object
        textObject.text = "";
    }

}


/*
public class DynamicText : MonoBehaviour        
{   

    public TMP_Text textObject;
    public string TextMessage;

    private void OnTriggerEnter(Collider other)
    {
        textObject.text = TextMessage;
    }

    private void OnTriggerExit(Collider other)
    {
        textObject.text = "";
    }
}
*/

