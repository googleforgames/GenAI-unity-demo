using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    public string InteractionCalloutText = "Press E";

    [SerializeField] private float _reactionRadius = 5.0f;

    private Action _interact;
    private Action _release;
    private Collider _trigger;

    private const string playerTag = "Player";

    public bool CanInteract { get; private set; } = false;
    public bool IsPlayerLookingAt { get; private set; } = false;

    private void Start()
    {
        SetReactionArea();
    }

    private void Update()
    {
        if (!CanInteract)
        {
            return;
        }

        if (!_trigger)
        {
            return;
        }

        if (Physics.Raycast(_trigger.transform.position, _trigger.transform.TransformDirection(Vector3.forward), out var hit, _reactionRadius) && hit.collider.name == gameObject.name)
        {
            IsPlayerLookingAt = true;
        }
        else
        {
            IsPlayerLookingAt = false;
        }
    }

    private void SetReactionArea()
    {
        var reactionArea = GetComponent<SphereCollider>();
        reactionArea.radius = _reactionRadius;
        reactionArea.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        CanInteract = true;
        InteractionHandler.Instance.OnPlayerEntered(other, this);
        _trigger = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        CanInteract = false;
        InteractionHandler.Instance.OnPlayerExited();
        _trigger = null;
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
        _interact?.Invoke();
    }

    public void OnRelease()
    {
        _release?.Invoke();
    }
}
