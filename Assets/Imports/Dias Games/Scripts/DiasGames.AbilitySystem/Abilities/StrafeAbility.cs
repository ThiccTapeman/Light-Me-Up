using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "StrafeAbility", menuName = "Dias Games/Abilities/StrafeAbility", order = 0)]
    public class StrafeAbility : Ability
    {
        [SerializeField] private float _strafeSpeed = 1.6f;
        [SerializeField] private string _animState = "Strafe";

        private bool _isZooming;

        public void Zoom(bool isZooming)
        {
            _isZooming = isZooming;
        }

        public override bool CanStart()
        {
            if (Movement.IsFalling)
            {
                return false;
            }

            return base.CanStart() && _isZooming;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_animState, transitionDuration: 0.15f);
            Movement.SetMaxMoveSpeed(_strafeSpeed);
        }

        public override void UpdateAbility(float deltaTime)
        {
            Movement.MoveByInput();
            Movement.RotateToViewDirection();
            
            if (!_isZooming)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
        }
    }
}