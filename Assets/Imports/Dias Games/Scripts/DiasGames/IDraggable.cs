using UnityEngine;

namespace DiasGames
{
    public interface IDraggable : IInteractable, IHandIKTarget
    {
        void StartDrag();
        void StopDrag();
        bool Move(Vector3 velocity);
    }
}