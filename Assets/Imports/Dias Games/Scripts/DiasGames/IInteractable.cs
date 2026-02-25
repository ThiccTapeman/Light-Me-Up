using UnityEngine;

namespace DiasGames
{
    public interface IInteractable
    {
        Transform InteractionPoint { get; }
        void Interact(GameObject instigator);
        void InteractionCallback(GameObject instigator);
    }
}