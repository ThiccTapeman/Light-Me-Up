using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "HardLandAbility", menuName = "Dias Games/Abilities/HardLandAbility", order = 0)]
    public class HardLandAbility : Ability
    {
        private const float LAND_TRANSITION = 0.02f;

        [SerializeField] private float _minVerticalSpeed;
        [SerializeField] private string _landAnimState = "Hard Land";
        [SerializeField] private float _duration = 1.5f;
        [SerializeField] private AudioClipContainer _hardLandClips;
        [SerializeField] private AudioClipContainer _hurtClips;

        private IAudioPlayer _audioPlayer;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        public override bool CanStart()
        {
            return !Movement.IsFalling && base.CanStart() && Movement.Velocity.y <= _minVerticalSpeed;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_landAnimState, transitionDuration: LAND_TRANSITION);
            _audioPlayer?.PlayEffect(_hardLandClips);
            _audioPlayer?.PlayVoice(_hurtClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime >= _duration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
        }
    }
}