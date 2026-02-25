using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "JumpAbility", menuName = "Dias Games/Abilities/JumpAbility", order = 0)]
    public class JumpAbility : Ability
    {
        private const float InputThreshold = 0.05f;
        
        [Header("Animation State")]
        [SerializeField] private string _animJumpState = "Air.Jump";
        [Header("Jump parameters")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _speedOnAir = 6f;
        [SerializeField] private float _airControl = 0.5f;
        [SerializeField] private string _fallAbilityName = "Fall";
        [Header("Sound FX")]
        [SerializeField] private AudioClipContainer jumpEffort;
 
        private IAudioPlayer _audioPlayer;

        private Vector3 _startDirection = Vector3.zero;
        private Vector3 _vel = Vector3.zero;
        private float _startSpeed;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            Movement = ownerSystem.GameObject.GetComponent<IMovement>();
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        public override bool CanStart()
        {
            if (Movement.IsFalling)
            {
                return false;
            }

            return base.CanStart();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            _startDirection = Movement.GetWorldDirectionInput();
            AnimationController.SetAnimationState(_animJumpState, transitionDuration: 0.1f);
            PerformJump();
        }

        public override void UpdateAbility(float deltaTime)
        {
            Vector3 inputDirection = Movement.GetWorldDirectionInput();
            _startDirection = Vector3.SmoothDamp(_startDirection, inputDirection, 
                ref _vel, 0.12f * (1.0f / _airControl));

            Movement.Move(_startDirection * _startSpeed);
            Movement.RotateToMovementDirection(_airControl);

            if (!Movement.IsFalling || Movement.Velocity.y < 0.0f)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            if (Movement.IsFalling)
            {
                OwnerSystem.StartAbilityByName(_fallAbilityName, instigator);
            }
        }

        private void PerformJump()
        {
            float jumpSpeed = Mathf.Sqrt(_jumpHeight * -2f * Movement.Gravity);

            Movement.Jump(jumpSpeed);
            _startSpeed = _speedOnAir;

            if (_startDirection.sqrMagnitude > InputThreshold)
            {
                _startDirection.Normalize();
            }
            else
            {
                _startSpeed = 0;
            }

            _audioPlayer?.PlayVoice(jumpEffort);
        }
    }
}