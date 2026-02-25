using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "DieAbility", menuName = "Dias Games/Abilities/DieAbility", order = 0)]
    public class DieAbility : Ability
    {
        [SerializeField] private AudioClip _dieClip;
        [SerializeField] private bool _keepVerticalSpeed;
        [SerializeField] private float _timeToRestartLevel = 4.0f;

        private Ragdoll _ragdoll;
        private IAudioPlayer _audioPlayer;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _ragdoll = ownerSystem.GameObject.GetComponent<Ragdoll>();
            _audioPlayer = ownerSystem.GameObject.GetComponent<IAudioPlayer>();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            float verticalSpeed = Movement.Velocity.y;
            
            Movement.StopMovement();
            if (_keepVerticalSpeed)
            {
                Movement.Move(Vector3.up * verticalSpeed);
            }

            _ragdoll.ActivateRagdoll();
            _audioPlayer?.PlayVoice(_dieClip);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsed = Time.time - StartTime;
            if (elapsed >= _timeToRestartLevel)
            {
                // Restart level here
                IPlayerController playerController = OwnerSystem.GameObject.GetComponent<IPlayerController>();
                playerController.LevelController.RestartLevel();
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
        }
    }
}