using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "CrouchAbility", menuName = "Dias Games/Abilities/CrouchAbility", order = 0)]
    public class CrouchAbility : Ability
    {
        [SerializeField] private string _crouchAnimState = "Crouch";
        [SerializeField] private LayerMask _obstaclesMask;
        [SerializeField] private float _capsuleHeightOnCrouch = 1f;
        [SerializeField] private float _crouchMoveSpeed = 3f;
        [SerializeField] private bool _holdKeyToCrouch;

        private float _defaultCapsuleHeight = 0;
        private float _defaultCapsuleRadius = 0;
        private bool _crouchingPressed = false;

        public void CrouchPressed(bool pressed)
        {
            if (_holdKeyToCrouch)
            {
                _crouchingPressed = pressed;
            }
            else
            {
                if (pressed)
                {
                    _crouchingPressed = !_crouchingPressed;
                }
            }
        }

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            Movement = OwnerSystem.GameObject.GetComponent<IMovement>();

            _defaultCapsuleRadius = Movement.CapsuleRadius;
            _defaultCapsuleHeight = Movement.CapsuleHeight;
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            return _crouchingPressed || ForceCrouchByHeight();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            Movement.SetCapsuleSize(_capsuleHeightOnCrouch, Movement.CapsuleRadius);
            Movement.Move(new Vector3(0, 0.5f, 0));
            Movement.SetMaxMoveSpeed(_crouchMoveSpeed);
            AnimationController.SetAnimationState(_crouchAnimState, transitionDuration: 0.25f);
        }

        public override void UpdateAbility(float deltaTime)
        {
            Movement.MoveByInput();
            Movement.RotateToMovementDirection();

            if (Movement.IsFalling)
            {
                StopAbility(OwnerSystem.GameObject);
            }

            if (!_crouchingPressed && !ForceCrouchByHeight())
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            Movement.ResetCapsuleSize();
        }

        private bool ForceCrouchByHeight()
        {
            if(Physics.SphereCast(transform.position, _defaultCapsuleRadius, Vector3.up, out RaycastHit hit, 
                   _defaultCapsuleHeight, _obstaclesMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.point.y - transform.position.y > _capsuleHeightOnCrouch)
                    return true;
            }

            return false;
        }
    }
}