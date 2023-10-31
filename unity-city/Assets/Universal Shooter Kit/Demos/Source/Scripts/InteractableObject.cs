using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _reactionRadius = 5.0f;

    public Action OnInteractAction;

    private const string playerTag = "Player";
    private Action _showChatBox;
    private Action<Animator> _playAnimation;
    private Action<bool> _onPlayerEntered;
    
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
        OnInteractAction = callback;
    }

    // 
    public void OnInteract()
    {
        Debug.Log($"An object {this} is activated");
        OnInteractAction?.Invoke();
    }
}
