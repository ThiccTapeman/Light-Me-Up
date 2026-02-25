using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbFreeSideJump", menuName = "Dias Games/Abilities/ClimbFreeSideJump", order = 0)]
    public class ClimbFreeSideJump : ClimbAbility
    {
        [SerializeField] private string _rightJumpAnimState = "Climb.Jump Right";
        [SerializeField] private string _leftJumpAnimState = "Climb.Jump Left";
        [SerializeField] private float _jumpPower = 3.5f;
        [SerializeField] private float _jumpHorSpeed = 6.0f;
        [SerializeField] private float _abilityDuration = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipContainer _climbJumpVocalClips;

        protected Vector3 _direction;
        private Quaternion _targetRotation;
        private float _dot;

        public override bool CanStart()
        {
            if (!base.CanStart() || Climbing.IsHanging)
            {
                return false;
            }

            if (Climbing.CurrentLedge.TryGetComponent(out ILedge ledge))
            {
                if (!ledge.CanJumpSide)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            Vector3 currentInputDirection = Movement.GetWorldDirectionInput();
            if (currentInputDirection.sqrMagnitude <= 0.05f)
            {
                return false;
            }

            _dot = Vector3.Dot(currentInputDirection, transform.right);
            return Mathf.Abs(_dot) > 0.5f;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            bool isRight = _dot > 0.0f;
            _direction = isRight ? transform.right : -transform.right;
            string animation = isRight ? _rightJumpAnimState : _leftJumpAnimState;
            
            AnimationController.SetAnimationState(animation);
            Movement.StopMovement();
            LaunchCharacter();
            AudioPlayer.PlayVoice(_climbJumpVocalClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Time.time - StartTime >= _abilityDuration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
        }

        protected virtual void LaunchCharacter()
        {
            Vector3 velocity = Vector3.up * _jumpPower + _direction * _jumpHorSpeed;

            _targetRotation = Quaternion.LookRotation(_direction);

            Movement.SetGravityEnabled(false);
            Movement.Move(velocity);
            Movement.SetGravityEnabled(true);

            const float lerpDuration = 0.3f;
            Tweener.DoRotationLerp(_targetRotation, lerpDuration);
        }
    }
}