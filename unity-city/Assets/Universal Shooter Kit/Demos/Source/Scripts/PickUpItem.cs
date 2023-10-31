using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    private bool canInteract = false;
    private int playersInteracting = 0;


    public void OnInteract()
    {
    
    }

    public void SetInteractable(bool isInteractable)
    {
        throw new NotImplementedException();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player can interact with object");
            canInteract = true;
            playersInteracting++;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player away from interact distance with object");
            playersInteracting--;
            if (playersInteracting == 0)
            {
                canInteract = false;
            }
        }
    }

    public void SubscribeOnInteract(Action callback)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            gameObject.SetActive(false);
        }
    }
}
