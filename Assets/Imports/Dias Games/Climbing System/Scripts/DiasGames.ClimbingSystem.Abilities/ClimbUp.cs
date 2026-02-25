using System;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbUp", menuName = "Dias Games/Abilities/ClimbUp", order = 0)]
    public class ClimbUp : ClimbAbility
    {
        [Serializable]
        private class ClimbUpAnimation
        {
            [SerializeField] private string _animationStateName;
            [SerializeField] private float _climbUpDuration = 0.8f;
            [SerializeField] private Vector3 _targetOffset = new Vector3(0, 1.5f, 0.5f);
            [SerializeField] private float _matchStartNormalizedTime = 0.5f;
            [SerializeField] private float _matchEndNormalizedTime = 0.9f;

            [Header("Audio")]
            [SerializeField] private AudioClipContainer _climbUpVocalClips;

            public string AnimationStateName => _animationStateName;
            public float Duration => _climbUpDuration;
            public Vector3 Offset => _targetOffset;
            public float StartNormalizedTime => _matchStartNormalizedTime;
            public float EndNormalizedTime => _matchEndNormalizedTime;
            public AudioClipContainer VocalClips => _climbUpVocalClips;
        }
        
        private const float SqrtInputThreshold = 0.05f;
        
        [SerializeField] private ClimbUpAnimation _climbUpState;
        [SerializeField] private ClimbUpAnimation _hangClimbUpState;

        private Animator animator => AnimationController.Animator;
        private bool _matchingTarget = false;
        private Vector3 _targetPosition;
        private ClimbUpAnimation _selectedClimbUp;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (Climbing.CurrentLedge == null)
            {
                return false;
            }

            Vector3 input = Movement.GetWorldDirectionInput();
            if (input.sqrMagnitude > SqrtInputThreshold)
            {
                float up = Vector3.Dot(input, transform.forward);
                if (up < 0.1f)
                {
                    return false;
                }
            }

            if (Climbing.CurrentLedge.TryGetComponent(out ILedge ledge))
            {
                return ledge.CanClimbUp;
            }

            _selectedClimbUp = Climbing.IsHanging ? _hangClimbUpState : _climbUpState;
            Vector3 targetPosition = GetClimbUpPosition();
            Vector3 cp1 = targetPosition + Vector3.up * Movement.CapsuleRadius;
            Vector3 cp2 = cp1 + Vector3.up * (Movement.CapsuleHeight - Movement.CapsuleRadius);
            if (Physics.CheckCapsule(cp1, cp2, Movement.CapsuleRadius,
                    Climbing.BlockMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return Climbing.CanClimbUp();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);
            _selectedClimbUp = Climbing.IsHanging ? _hangClimbUpState : _climbUpState;
            AnimationController.SetAnimationState(_selectedClimbUp.AnimationStateName);
            Movement.StopMovement();
            Movement.DisableCollision();
            Movement.SetGravityEnabled(false);
            _matchingTarget = false;

            _targetPosition = GetClimbUpPosition();
            AudioPlayer.PlayVoice(_selectedClimbUp.VocalClips);
        }

        private Vector3 GetClimbUpPosition()
        {
            Vector3 targetOffset = _selectedClimbUp.Offset;
            return transform.position + transform.forward * targetOffset.z + 
                   Vector3.up * targetOffset.y;
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime > _selectedClimbUp.Duration)
            {
                StopAbility(OwnerSystem.GameObject);
                return;
            }
            
            if (elapsedTime < 0.1f || animator.IsInTransition(0) || _matchingTarget)
            {
                return;
            }

            CastDebug.Instance.DrawCapsule(_targetPosition + Vector3.up * Movement.CapsuleRadius, 
                _targetPosition + Vector3.up * (Movement.CapsuleHeight - Movement.CapsuleRadius),
               Movement.CapsuleRadius,
                Color.green,
                2f);

            animator.MatchTarget(_targetPosition, 
                transform.rotation, 
                AvatarTarget.Root, 
                new MatchTargetWeightMask(Vector3.one, 1f),
                _selectedClimbUp.StartNormalizedTime,
                _selectedClimbUp.EndNormalizedTime);

            _matchingTarget = true;
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Movement.EnableCollision();
            Climbing.SetCurrentLedge(new ClimbCastData());
            Movement.StopMovement();
        }
    }
}