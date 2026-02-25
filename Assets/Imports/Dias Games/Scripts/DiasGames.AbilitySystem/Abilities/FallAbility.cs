using DiasGames.AbilitySystem.Core;
using DiasGames.AbilitySystem.Effects;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "FallAbility", menuName = "Dias Games/Abilities/FallAbility", order = 0)]
    public class FallAbility : Ability
    {
        [SerializeField] private string animFallState = "Air.Falling";
        [SerializeField] private float _airControl = 0.5f;
        [Header("Landing")]
        [SerializeField] private float heightForHardLand = 3f;
        [SerializeField] private FallingHealthDamage _damageEffect;

        private float _startSpeed = 0;
        private float _highestPosition = 0;
        private bool _hardLanding = false;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            Movement = ownerSystem.GameObject.GetComponent<IMovement>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            return Movement.GravityEnabled && Movement.IsFalling;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(animFallState, transitionDuration: 0.25f);
            _startSpeed = Vector3.Scale(Movement.Velocity, new Vector3(1, 0, 1)).magnitude;
            _highestPosition = transform.position.y;
            _hardLanding = false;
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (!Movement.IsFalling)
            {
                if(_highestPosition - transform.position.y >= heightForHardLand)
                {
                    _hardLanding = true;
                }

                StopAbility(OwnerSystem.GameObject);
                return;
            }

            if (transform.position.y > _highestPosition)
            {
                _highestPosition = transform.position.y;
            }

            Movement.Move(transform.forward * _startSpeed);
            Movement.RotateToMovementDirection(_airControl);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            if (_hardLanding)
            {
                // cause damage
                _damageEffect.SetHighestPosition(_highestPosition);
                OwnerSystem.AddAbility(_damageEffect, instigator);

                OwnerSystem.StartAbilityByName("HardLand", instigator);
            }
        }
    }
}