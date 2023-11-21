using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryDemo : MonoBehaviour
{
    [SerializeField] private List<Slot> _slots = new List<Slot>();

    public void PutItem(PickUpItemBase item)
    {
        var slot = _slots.FirstOrDefault(slot => !slot.IsOccupied);
        if (slot)
        {
            slot.PutItem(item, item.Icon);
        }
    }

    public void SetupSlots()
    {
        foreach (var slot in _slots)
        {
            slot.Setup();
        }
    }
}
