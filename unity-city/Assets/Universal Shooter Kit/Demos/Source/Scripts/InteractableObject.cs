using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _reactionRadius = 5.0f;

    private Action _interact;
    private Action _release;
    private Action<bool> _playerEntered;

    private const string playerTag = "Player";

    private bool _isInteractable = true;

    private void Start()
    {
        SetReactionArea();
    }

    private void SetReactionArea()
    {
        var reactionArea = GetComponent<SphereCollider>();
        reactionArea.radius = _reactionRadius;
        reactionArea.isTrigger = true;
    }

    private void Update()
    {
        if (_isInteractable && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }

        if (_isInteractable && Input.GetKeyUp(KeyCode.E))
        {
            OnRelease();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            _playerEntered?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            _playerEntered?.Invoke(false);
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
    }

    public void SubscribeOnShowHint(Action<bool> callback)
    {
        _playerEntered = callback;
    }

    public void SubscribeOnInteract(Action callback)
    {
        _interact = callback;
    }

    public void SubscribeOnRelease(Action callback)
    {
        _release = callback;
    }

    public void OnInteract()
    {
        Debug.Log($"An object {this} is activated");
        _interact?.Invoke();
    }

    private void OnRelease()
    {
        Debug.Log($"An object {this} is deactivated");
        _release?.Invoke();
    }
}
