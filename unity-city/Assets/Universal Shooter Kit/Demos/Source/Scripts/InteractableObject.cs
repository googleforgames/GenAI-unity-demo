using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _reactionRadius = 5.0f;

    private const string playerTag = "Player";
    private Action _onInteract;
    private Action<bool> _onPlayerEntered;
    private SphereCollider _reactionZone;
    private bool _isInteractable = true;

    private void Start()
    {
        _reactionZone = GetComponent<SphereCollider>();
        _reactionZone.radius = _reactionRadius;
        _reactionZone.isTrigger = true;
    }

    private void Update()
    {
        if (_isInteractable && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            _onPlayerEntered?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            _onPlayerEntered?.Invoke(false);
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
    }

    public void SubscribeOnShowHint(Action<bool> callback)
    {
        _onPlayerEntered = callback;
    }

    public void SubscribeOnInteract(Action callback)
    {
        _onInteract = callback;
    }

    public void OnInteract()
    {
        Debug.Log($"An object {this} is activated");
        _onInteract?.Invoke();
    }
}
