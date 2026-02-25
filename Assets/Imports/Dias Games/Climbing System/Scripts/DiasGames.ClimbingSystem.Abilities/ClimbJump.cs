using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbJump", menuName = "Dias Games/Abilities/ClimbJump", order = 0)]
    public class ClimbJump : ClimbAbility
    {
        private const float DotRangeForVerticalHop = 0.95f;
        
        [SerializeField] private ClimbJumpAnimation _hopRightAnimation;
        [SerializeField] private ClimbJumpAnimation _hopLeftAnimation;
        [SerializeField] private ClimbJumpAnimation _hopUpAnimation;
        [SerializeField] private ClimbJumpAnimation _hopDownAnimation;
        [SerializeField] private float _maxJumpDistance = 2.0f;
        [Header("Audio")]
        [SerializeField] private AudioClipContainer _grabAudioClips;
        [SerializeField] private AudioClipContainer _climbJumpVocalClips;

        private ClimbCastData _castData;
        private ClimbJumpAnimation _jumpSelected;
        private bool _playedGrab;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            return Climbing.FindLedgeOnJumpDirection(Movement.GetWorldDirectionInput(), _maxJumpDistance, out _castData);
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            Vector3 targetPosition = Climbing.GetCharacterPositionOnLedge(_castData);
            Vector3 direction = (targetPosition - transform.position).normalized;
            float rightDot = Vector3.Dot(transform.right, direction);
            float upDot = Vector3.Dot(Vector3.up, direction);
            _jumpSelected = GetJumpAnimationState(rightDot, upDot);
            
            DoLerpToTarget(_castData, _jumpSelected);

            AudioPlayer.PlayVoice(_climbJumpVocalClips);
            _playedGrab = false;
        }

        private ClimbJumpAnimation GetJumpAnimationState(float rightDot, float upDot)
        {
            if (upDot > DotRangeForVerticalHop)
            {
                return _hopUpAnimation;
            }

            if (upDot < -DotRangeForVerticalHop)
            {
                return _hopDownAnimation;
            }

            if (rightDot >= 0.0f)
            {
                return _hopRightAnimation;
            }

            return _hopLeftAnimation;
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;

            if (elapsedTime > _jumpSelected.AnimDuration * 0.5f)
            {
                Climbing.SetCurrentLedge(_castData);
            }

            if (elapsedTime > _jumpSelected.JumpDuration)
            {
                PlayGrabClips();
            }
            
            if (elapsedTime > _jumpSelected.AnimDuration)
            {
                StopAbility(OwnerSystem.GameObject);
                OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
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