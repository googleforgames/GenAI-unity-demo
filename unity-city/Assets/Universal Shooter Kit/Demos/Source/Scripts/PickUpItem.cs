using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    private bool canInteract = false;
    private int playersInteracting = 0;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnInteract(string playerName, string objectName)
    {
        
    }
}
