using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Components
{
    public class InteractionComponent : MonoBehaviour, IInteractionComponent
    {
        private readonly List<IInteractable> _triggeredInteractables = new List<IInteractable>(5);
        private int _currentIndex = 0;
        private IMovement _movement;

        private void Awake()
        {
            _movement = GetComponent<IMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                if (_triggeredInteractables.Contains(interactable))
                {
                    return;
                }

                _triggeredInteractables.Add(interactable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                _triggeredInteractables.Remove(interactable);
            }
        }

        public void Interact()
        {
            if (_triggeredInteractables.Count == 0)
            {
                return;
            }

            if (_currentIndex >= _triggeredInteractables.Count)
            {
                _currentIndex = 0;
            }

            IInteractable current = _triggeredInteractables[_currentIndex];
            if (!IsValidInteractable(current))
            {
                _triggeredInteractables.RemoveAt(_currentIndex);
                Interact();
                return;
            }

            current.Interact(gameObject);
            _currentIndex++;
        }

        private bool IsValidInteractable(IInteractable current)
        {
            Vector3 cp1 = transform.position + Vector3.up * _movement.CapsuleRadius;
            Vector3 cp2 = cp1 + Vector3.up * (_movement.CapsuleHeight - _movement.CapsuleRadius * 2);

            Collider[] overlapped = new Collider[10];
            int size = Physics.OverlapCapsuleNonAlloc(cp1, cp2, _movement.CapsuleRadius, overlapped, Physics.AllLayers);
            for (int i = 0; i < size; i++)
            {
                Collider coll = overlapped[i];
                if (coll.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable == current)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}