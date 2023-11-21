using UnityEngine;

/// <summary>
/// TODO: Make generic behavior.
/// This trigger calls the fence door and keeps it open as long as the trigger is pressed.
/// </summary>
public class TriggerBehavior: MonoBehaviour, IInteractable
{

    public GameObject targetDoor;
    private Vector3 doorOrigin;
    private Vector3 leverReleasedState;
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

    // Start is called before the first frame update
    void Start()
    {
        doorOrigin = targetDoor.transform.position;
        leverReleasedState = leverReleasedState = GetComponentInParent<Transform>().eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E) && canInteract)
        {
            GetComponentInParent<Transform>().eulerAngles = new Vector3(leverReleasedState.x, leverReleasedState.y, leverReleasedState.z + 90);
            targetDoor.transform.position = new Vector3(doorOrigin.x, doorOrigin.y, doorOrigin.z + 3);
        }
        else
        {
            GetComponentInParent<Transform>().eulerAngles = leverReleasedState;
            targetDoor.transform.position = doorOrigin;
        }
    }

    public void OnInteract()
    {
        GetComponentInParent<Transform>().eulerAngles = new Vector3(GetComponentInParent<Transform>().rotation.x, GetComponentInParent<Transform>().rotation.y, GetComponentInParent<Transform>().rotation.z + 90);
        targetDoor.transform.position = new Vector3(doorOrigin.x, doorOrigin.y, doorOrigin.z + 3);
    }
}
