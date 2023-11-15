using UnityEngine;

public class AnimationPlayer : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableObject _interactableObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _forwardAnimationTrigger;
    [SerializeField] private string _backwardAnimationTrigger;

    private void Start()
    {
        _interactableObject = GetComponent<InteractableObject>();
        if (!_interactableObject)
        {
            return;
        }

        _interactableObject.SubscribeOnInteract(OnInteract);
        _interactableObject.SubscribeOnRelease(OnRelease);
    }

    public void OnInteract()
    {
        if (_animator)
        {
            _animator.SetTrigger(_forwardAnimationTrigger);
        }
    }

    private void OnRelease()
    {
        if (_animator)
        {
            _animator.SetTrigger(_backwardAnimationTrigger);
        }
    }
}