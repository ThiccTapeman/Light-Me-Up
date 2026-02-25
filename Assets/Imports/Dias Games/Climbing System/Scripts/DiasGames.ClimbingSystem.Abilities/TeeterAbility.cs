using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "Teeter", menuName = "Dias Games/Abilities/Teeter", order = 0)]
    public class TeeterAbility : Ability
    {
        [SerializeField] private string _teeterAnimState = "Teeter";
        [SerializeField][UnityTag] private string _teeterTag = "Teeter";

        private TweenerComponent _tweenerComponent;
        private Transform _target;

        public override bool CanStart()
        {
            if (!base.CanStart() || 
                Movement.Velocity.y > 1.0f ||
                Time.time - StopTime < 0.2f)
            {
                return false;
            }

            _target = GetTeeterTarget();
            return _target != null && IsValidPosition();
        }

        private bool IsValidPosition()
        {
            return transform.position.y >= _target.position.y - 0.5f;
        }

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _tweenerComponent = ownerSystem.GameObject.GetComponent<TweenerComponent>();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_teeterAnimState);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);

            _tweenerComponent.DoLerp(_target.position, transform.rotation, 0.15f);
        }

        public override void UpdateAbility(float deltaTime)
        {
            Movement.RotateToMovementDirection();
        }

        protected override void OnStopAbility(GameObject instigator)
        { 
            Movement.SetGravityEnabled(true);
        }

        private Transform GetTeeterTarget()
        {
            Vector3 cp1 = transform.position + Vector3.up * Movement.CapsuleRadius;
            Vector3 cp2 = cp1 + Vector3.up * (Movement.CapsuleHeight - Movement.CapsuleRadius * 2);

            Collider[] overlapped = new Collider[10];
            int size = Physics.OverlapCapsuleNonAlloc(cp1, cp2, Movement.CapsuleRadius, overlapped, Physics.AllLayers);
            for (int i = 0; i < size; i++)
            {
                Collider coll = overlapped[i];
                if (coll.CompareTag(_teeterTag))
                {
                    return coll.gameObject.transform;
                }
            }

            return null;
        }
    }
}