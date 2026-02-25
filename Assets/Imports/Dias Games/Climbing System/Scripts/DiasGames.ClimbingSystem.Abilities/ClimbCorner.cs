using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbCorner", menuName = "Dias Games/Abilities/ClimbCorner", order = 0)]
    public class ClimbCorner : ClimbAbility
    {
        [SerializeField] private ClimbJumpAnimation _animCornerRight;
        [SerializeField] private ClimbJumpAnimation _animCornerLeft;

        [Header("Audio")]
        [SerializeField] private AudioClipContainer _grabAudioClips;
        [SerializeField] private AudioClipContainer _cornerVocalClips;

        private ClimbJumpAnimation _selectedJump;

        private ClimbCastData _castData;
        private float _cornerDirection = 0;
        private bool _playedGrab;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            float dot = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.right);
            _cornerDirection = dot > 0 ? 1.0f : -1.0f;
            return Climbing.FindCorner(transform.right * _cornerDirection, out _castData);
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            _selectedJump = _cornerDirection > 0 ? _animCornerRight : _animCornerLeft;
            
            DoLerpToTarget(_castData, _selectedJump);

            AudioPlayer.PlayVoice(_cornerVocalClips);
            _playedGrab = false;
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsed = Time.time - StartTime;
            if (elapsed > _selectedJump.AnimDuration)
            {
                StopAbility(OwnerSystem.GameObject);
                OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
            }
            
            if (elapsed >= _selectedJump.AnimDuration && !_playedGrab)
            {
                PlayGrabClips();
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Climbing.SetCurrentLedge(_castData);
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