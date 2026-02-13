using UnityEngine;

public interface IInteractable
{
    public string InteractMessage { get; }
    public void Interact();
}
