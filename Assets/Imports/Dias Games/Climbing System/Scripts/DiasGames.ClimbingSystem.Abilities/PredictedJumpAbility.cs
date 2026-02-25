using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "PredictedJump", menuName = "Dias Games/Abilities/PredictedJump", order = 0)]
    public class PredictedJumpAbility : Ability
    {
        [SerializeField] private string _jumpAnimState;
        [SerializeField] private string _jumpToLedgeAnimState;
        [SerializeField] private float _maxAcceptableAngle = 60.0f;
        [SerializeField] private JumpParameters _jumpParameters;
        [Header("Sound FX")]
        [SerializeField] private AudioClipContainer _jumpEffort;

        private PredictedJumpComponent _jumpLauncher;
        private TweenerComponent _tweener;
        private JumpType _selectedJump;
        private LaunchData _launchData;
        private float _duration;
        private IAudioPlayer _audioPlayer;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _jumpLauncher = ownerSystem.GameObject.GetComponent<PredictedJumpComponent>();
            _tweener = ownerSystem.GameObject.GetComponent<TweenerComponent>();
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (Movement.GetWorldDirectionInput().sqrMagnitude < 0.02f)
            {
                return false;
            }

            Vector3 moveDirection = Movement.GetWorldDirectionInput();
            _selectedJump = _jumpLauncher.GetLaunchParameters(moveDirection, _jumpParameters, out _launchData, _maxAcceptableAngle);
            return _selectedJump != JumpType.None;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_selectedJump == JumpType.JumpWithPrediction ? _jumpAnimState : _jumpToLedgeAnimState);
            _duration = _launchData.timeToTarget;

            Movement.Jump(_launchData.initialVelocity.y);
            Movement.Move(_launchData.initialVelocity);
            Quaternion targetRotation = Movement.GetRotationFromDirection(_launchData.initialVelocity.normalized);
            _tweener.DoRotationLerp(targetRotation, 0.1f);
            _audioPlayer?.PlayVoice(_jumpEffort);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Time.time - StartTime >= _duration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        { }
    
    }
}