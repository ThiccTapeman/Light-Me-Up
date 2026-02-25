using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "RollAbility", menuName = "Dias Games/Abilities/RollAbility", order = 0)]
    public class RollAbility : Ability
    {
        [SerializeField] private string _animRollState = "Roll";
        [SerializeField] private float _rollSpeed = 7f;
        [SerializeField] private float _rollDuration = 0.9f;
        [SerializeField] private float _capsuleHeightOnRoll = 1f;
        [SerializeField] private AudioClipContainer _rollClips;

        private Vector3 _rollDirection = Vector3.forward;
        private IAudioPlayer _audioPlayer;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_animRollState);
            
            Movement.SetCapsuleSize(_capsuleHeightOnRoll, Movement.CapsuleRadius);

            _rollDirection = transform.forward;
            Vector3 inputDirection = Movement.GetWorldDirectionInput();
            if(inputDirection != Vector3.zero)
            {
                _rollDirection = inputDirection;
            }

            _rollDirection.Normalize();
            _audioPlayer.PlayEffect(_rollClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            Movement.Move(_rollDirection * _rollSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                Movement.GetRotationFromDirection(_rollDirection), 5f * deltaTime);

            if (Time.time - StartTime > _rollDuration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            Movement.ResetCapsuleSize();
        }
    }
}