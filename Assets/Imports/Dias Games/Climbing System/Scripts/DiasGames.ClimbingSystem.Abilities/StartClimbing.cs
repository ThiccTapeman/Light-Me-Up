using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "StartClimbing", menuName = "Dias Games/Abilities/StartClimbing", order = 0)]
    public class StartClimbing : ClimbAbility
    {
        [SerializeField] private string _animState = "Climb.Start Climb";
        [SerializeField] private float _lerpDuration = 0.2f;
        [SerializeField] private float _abilityDuration = 0.5f;
        [SerializeField] private AudioClipContainer _grabAudioClips;

        private ClimbCastData _castData;
        private Transform _target;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _target = new GameObject("(Start Climb) Aux Transform. Don't Destroy it").transform;
        }

        public override bool CanStart()
        {
            if (!Movement.IsFalling)
            {
                return false;
            }

            if (!base.CanStart())
            {
                return false;
            }

            return Climbing.FindLedge(out _castData);
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);
            Movement.SetGravityEnabled(false);
            Movement.StopMovement();
            _target.position = Climbing.GetCharacterPositionOnLedge(_castData);
            _target.rotation = Climbing.GetCharacterRotationOnLedge(_castData);
            _target.SetParent(_castData.Ledge.transform);
            Tweener.DoLerp(_target.position, _target.rotation, _lerpDuration, _castData.Ledge.transform);
            AnimationController.SetAnimationState(_animState);
            Climbing.SetCurrentLedge(_castData);
            AudioPlayer.PlayEffect(_grabAudioClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Time.time - StartTime > _abilityDuration)
            {
                OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
            }

            if (!Tweener.IsRunningTween)
            {
                Movement.SetPosition(_target.position);
                Movement.SetRotation(_target.rotation);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Tweener.StopTween();
            _target.SetParent(null);
        }
    }
}