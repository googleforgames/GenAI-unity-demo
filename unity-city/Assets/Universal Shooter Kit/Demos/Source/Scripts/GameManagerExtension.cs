using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();
    // TODO: move to the UIManager
    [SerializeField] private TextMeshProUGUI _uiHint;
    private bool _isHintShown = false;

    private void Start()
    {
        if (_interactableObjects.Any())
        {
            foreach (var interactableObject in _interactableObjects)
            {
                interactableObject.SubscribeOnShowHint(ShowInteractHint);
            }
        }
    }

    private void ShowInteractHint(bool showHint)
    {
        if (_isHintShown == showHint)
        {
            return;
        }

        _isHintShown = showHint;
        if (_uiHint)
        {
            _uiHint.gameObject.SetActive(showHint);
        }
    }
}