using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour
{
    public Action<Dialogue, string, string> ShowChatBox;

    public string PlayerName { get; private set; }
    public string ObjectName { get; private set; }

    [SerializeField] private float _reactionRadius = 5.0f;

    private Action<string, string> _interact;
    private Action _release;
    private Action<bool> _playerEntered;

    [HideInInspector] public GameManagerExtension GameManager;
    private const string playerTag = "Player";

    private bool _canInteract = false;
    private bool _isApplied = false;

    private void Start()
    {
        ObjectName = gameObject.name;
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
        var isInteractable = _canInteract && !_isApplied && !GameManager.IsChatBoxShown();
        if (isInteractable && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        if (isInteractable && Input.GetKeyUp(KeyCode.E))
        {
            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        Debug.Log($"Player entered interaction area of {ObjectName}");

        PlayerName = other.name;
        SetInteractable(true);
        _playerEntered?.Invoke(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag(playerTag))
        {
            return;
        }

        Debug.Log($"Player exited interaction area of {ObjectName}");

        SetInteractable(false);
        _playerEntered?.Invoke(false);
    }

    private void SetInteractable(bool isInteractable)
    {
        _canInteract = isInteractable;
    }

    public void SubscribeOnShowHint(Action<bool> callback)
    {
        _playerEntered = callback;
    }

    public void SubscribeOnInteract(Action<string, string> callback)
    {
        _interact = callback;
    }

    public void SubscribeOnRelease(Action callback)
    {
        _release = callback;
    }

    public void SubscribeOnShowChatBox(Action<Dialogue, string, string> callback, GameManagerExtension gameManagerExtension)
    {
        GameManager ??= gameManagerExtension;
        ShowChatBox = callback;
    }

    public void Interact()
    {
        _interact?.Invoke(PlayerName, ObjectName);
    }

    private void Release()
    {
        _release?.Invoke();
    }
}
