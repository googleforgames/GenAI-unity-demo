using Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PuzzleSolvingDemo : MonoBehaviour, IInteractable
{

    [SerializeField] private GameObject greenBox, blueBox, redBox;
    private int puzzleStep = 0;
    private int playersInteracting;
    private bool canInteract;
    [SerializeField] private GameObject escapeShip;
    [SerializeField] private GameObject pilot;

    // Start is called before the first frame update
    void Start()
    {
        greenBox.SetActive(false);
        blueBox.SetActive(false);
        redBox.SetActive(false);
    }

    private IEnumerator ShipTakeoffTrigger()
    {


        yield return new WaitForSecondsRealtime(1);

        greenBox.SetActive(false);
        blueBox.SetActive(false);
        redBox.SetActive(false);

        yield return new WaitForSecondsRealtime(1);
        pilot.SetActive(false);
        escapeShip.GetComponent<EscapeShip>().FlyAway();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canInteract)
        {
            switch (puzzleStep) 
            {
                case 0: greenBox.SetActive(true);
                    break;
                case 1: blueBox.SetActive(true);
                    break;
                case 2: redBox.SetActive(true); 
                    break;
            }

            puzzleStep++;
        }

        if (puzzleStep >= 2) 
        {
            _ = StartCoroutine(ShipTakeoffTrigger());
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player can interact with the puzzle");
            canInteract = true;
            playersInteracting++;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playersInteracting--;
            if (playersInteracting == 0)
            {
                canInteract = false;
            }
        }
    }

    public void OnInteract(string playerName, string objectName)
    {
        throw new System.NotImplementedException();
    }
}
