using System.Collections.Generic;
using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbJumpBack", menuName = "Dias Games/Abilities/ClimbJumpBack", order = 0)]
    public class ClimbJumpBack : ClimbAbility
    {
        [SerializeField] private string _jumpBackAnimation;
        [SerializeField] private JumpParameters _jumpParameters;
        [SerializeField] private float _maxHeight = 1.0f;
        [SerializeField] private float _maxDistance = 7f;
        [SerializeField] private float _delayToPerformJump = 0.5f;
        [SerializeField] private float _acceptableAngle = 45.0f;
        
        [Header("Audio")]
        [SerializeField] private AudioClipContainer _jumpVocalClips;

        private List<ClimbCastData> _castResults;
        private LaunchComponent _launchComponent;
        private PredictedJumpComponent _predictedJumpComponent;
        private LaunchData _launchData;
        private Quaternion _targetRotation;
        private Vector3 _direction;
        private IRootMotion _rootMotion;
        private bool _foundLedge;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _launchComponent = ownerSystem.GameObject.GetComponent<LaunchComponent>();
            _predictedJumpComponent = ownerSystem.GameObject.GetComponent<PredictedJumpComponent>();
            _rootMotion = ownerSystem.GameObject.GetComponent<IRootMotion>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart() || Climbing.IsHanging)
            {
                return false;
            }

            _foundLedge = Climbing.FindBackLedges(_maxHeight, _maxDistance, out _castResults);

            return true;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            AnimationController.SetAnimationState(_jumpBackAnimation);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            _launchData = default;
            _direction = -transform.forward;
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime < _delayToPerformJump)
            {
                return;
            }

            if (!Movement.GravityEnabled)
            {
                LaunchCharacter();
                _rootMotion.StopRootMotion();
            }

            if (!_launchData.foundSolution)
            {
                if (!Tweener.IsRunningTween)
                {
                    StopAbility(OwnerSystem.GameObject);
                }
                return;
            }

            if (elapsedTime > _delayToPerformJump + _launchData.timeToTarget)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        private void LaunchCharacter()
        {
            Vector3 velocity = Vector3.up * _jumpParameters.GetVerticalSpeed(Movement.Gravity) +
                               _direction * _jumpParameters.horizontalSpeed;

            _targetRotation = Quaternion.LookRotation(_direction);
            float lerpDuration = 0.3f;
            if (!FindJumpDestination() && _foundLedge)
            {
                SelectBestLedge();
            }

            if (_launchData.foundSolution)
            {
                velocity = _launchData.initialVelocity;
                lerpDuration = _launchData.timeToTarget - 0.1f;
            }

            Movement.Move(velocity);
            Movement.SetGravityEnabled(true);
            Tweener.DoRotationLerp(_targetRotation, lerpDuration);
            AudioPlayer.PlayVoice(_jumpVocalClips);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
        }

        private void SelectBestLedge()
        {
            float highestHeight = transform.position.y - _maxHeight * 2.0f;
            float bestDot = -10;
            foreach (ClimbCastData castResult in _castResults)
            {
                Vector3 targetPosition = Climbing.GetCharacterPositionOnLedge(castResult);
                if (targetPosition.y < highestHeight)
                {
                    continue;
                }
                    
                float dot = Vector3.Dot((targetPosition - transform.position).normalized, _direction);
                if (dot < Mathf.Cos(_acceptableAngle * Mathf.Deg2Rad))
                {
                    continue;
                }

                if (dot < bestDot)
                {
                    continue;
                }

                bestDot = dot;
                    
                LaunchData launchData = _launchComponent.CalculateLaunchData(transform.position, targetPosition, _jumpParameters);
                if (!launchData.foundSolution)
                {
                    continue;
                }

                _launchData = launchData;
                _targetRotation = Climbing.GetCharacterRotationOnLedge(castResult);
                highestHeight = targetPosition.y;
            }
        }

        private bool FindJumpDestination()
        {
            var result = _predictedJumpComponent.GetLaunchParameters(_direction, _jumpParameters, 
                out _launchData, _acceptableAngle);

            return result == JumpType.JumpWithPrediction;
        }
    }
}