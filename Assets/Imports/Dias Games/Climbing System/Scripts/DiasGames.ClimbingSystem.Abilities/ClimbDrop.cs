using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbDrop", menuName = "Dias Games/Abilities/ClimbDrop", order = 0)]
    public class ClimbDrop : ClimbAbility
    {
        private const float BlockDuration = 1.5f;

        [SerializeField] private ClimbJumpAnimation _dropJumpAnimation;
        [SerializeField] private string _dropToFallAnimState = "Climb.Drop";
        [SerializeField] private float _dropToFallDuration = 0.5f;
        [SerializeField] private float _maxDropDistance = 1.0f;
        [Header("Audio")]
        [SerializeField] private AudioClipContainer _grabAudioClips;
        [SerializeField] private AudioClipContainer _dropVocalClips;

        private ClimbCastData _castData;
        private float _duration;
        private bool _playedGrab;

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);
            _playedGrab = false;

            if (Climbing.FindLedgeOnJumpDirection(-transform.forward, _maxDropDistance, 
                    out _castData, LedgeSelectionRule.Closest))
            {
                if (_castData.Ledge != Climbing.CurrentLedge)
                {
                    DoLerpToTarget(_castData, _dropJumpAnimation);
                    _duration = _dropJumpAnimation.AnimDuration;
                    return;
                }
            }

            AnimationController.SetAnimationState(_dropToFallAnimState);
            _duration = _dropToFallDuration;
            Climbing.BlockLedgeTemporaly(Climbing.CurrentLedge, BlockDuration);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            AudioPlayer.PlayVoice(_dropVocalClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsed = Time.time - StartTime;
            if (elapsed > _duration)
            {
                StopAbility(OwnerSystem.GameObject);
                if (_castData.HasLedge && _castData.Ledge != Climbing.CurrentLedge)
                {
                    Climbing.SetCurrentLedge(_castData);
                    OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
                }
            }

            if (elapsed >= _dropJumpAnimation.JumpDuration && _castData.HasLedge)
            {
                PlayGrabClips();
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Movement.Move(transform.forward);
        }
        
        private void PlayGrabClips()
        {
            if (!_playedGrab)
            {
                AudioPlayer.PlayEffect(_grabAudioClips);
                _playedGrab = true;
            }
        }
    }
}