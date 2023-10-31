using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManagerExtension : MonoBehaviour
{
    [SerializeField] private List<InteractableObject> _interactableObjects = new List<InteractableObject>();
    // TODO: move to the UIManager
    [SerializeField] private TextMeshProUGUI _interactionCallout;

    private bool _isHintShown = false;

    private void Start()
    {
        if (_interactableObjects.Any())
        {
            foreach (var io in _interactableObjects)
            {
                io.SubscribeOnShowHint(ShowInteractionCallout);
            }
        }
    }

    private void ShowInteractionCallout(bool showHint)
    {
        if (_isHintShown == showHint)
        {
            return;
        }

        _isHintShown = showHint;
        if (_interactionCallout)
        {
            _interactionCallout.gameObject.SetActive(showHint);
        }
    }
}
