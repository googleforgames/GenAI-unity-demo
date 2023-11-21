using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] GameObject inventoryUI;
    private bool isInventoryOpen = false;

    void Start()
    {
        inventoryUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            Debug.Log("Key I was pressed");
            isInventoryOpen = !(isInventoryOpen);
            inventoryUI.SetActive(isInventoryOpen);
        }
    }
}
