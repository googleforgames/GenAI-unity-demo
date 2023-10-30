using System;

public interface IInteractable
{
    public void SetInteractable(bool isInteractable);

    public void SubscribeOnInteract(Action callback);

    public void OnInteract();
}