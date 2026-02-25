using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.Puzzle
{
    public class GrabTrigger : MonoBehaviour, IDraggable
    {
        [SerializeField] private Ability _dragAbility;
        [SerializeField] private Transform targetCharacterTransform = null;
        [SerializeField] private Transform rightIK = null;
        [SerializeField] private Transform leftIK = null;
        [Space]
        [SerializeField] private Collider characterRefCollider;

        public Transform InteractionPoint => targetCharacterTransform;

        private DraggableObject _block = null;
        private IAbilitySystem _charAbilitySystem;
        private bool _interacting = false;

        private void Awake()
        {
            _block = GetComponentInParent<DraggableObject>();
            characterRefCollider.enabled = false;
        }

        protected virtual bool CanInteract(GameObject instigator)
        {
            return instigator.TryGetComponent(out _charAbilitySystem);
        }

        public void Interact(GameObject instigator)
        {
            if (!CanInteract(instigator))
            {
                return;
            }

            _interacting = !_interacting;
            if (_interacting)
            {
                _charAbilitySystem.AddAbility(_dragAbility, gameObject);
                _charAbilitySystem.StartAbilityByName(_dragAbility.AbilityName, gameObject);
            }
            else
            {
                _charAbilitySystem.StopAbilityByName(_dragAbility.AbilityName, gameObject);
            }
        }

        public void InteractionCallback(GameObject instigator)
        {
            StartDrag();
        }

        public void StartDrag()
        {
            _block.EnablePhysics();
            characterRefCollider.enabled = true;
        }

        public void StopDrag()
        {
            _block.DisablePhysics();
            characterRefCollider.enabled = false;
        }

        public bool Move(Vector3 velocity)
        {
            return _block.Move(velocity);
        }

        #region IHandIk

        public Transform GetLeftHandTarget()
        {
            return leftIK;
        }

        public Transform GetRightHandTarget()
        {
            return rightIK;
        }

        #endregion
    }
}