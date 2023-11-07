using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    public string InteractionCalloutText = "Press E";

    [SerializeField] private float _reactionRadius = 5.0f;

    private Action _interact;
    private Action _release;

    private const string playerTag = "Player";

    public bool CanInteract { get; private set; } = false;

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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        CanInteract = true;
        InteractionHandler.Instance.OnPlayerEntered(other, this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        CanInteract = false;
        InteractionHandler.Instance.OnPlayerExited();
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
