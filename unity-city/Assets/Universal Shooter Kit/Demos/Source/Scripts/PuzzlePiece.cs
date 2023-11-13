using UnityEngine;

public class PuzzlePiece : PickUpItemBase, IUsable
{
    private Sprite Icon;

    [SerializeField] private GameObject _gameObjectToActivate;

    public new void Use()
    {
        if (_interactableObject.IsPlayerLookingAt)
        {
            // Implement the logic here
            _gameObjectToActivate.SetActive(true);
        }
    }
}
