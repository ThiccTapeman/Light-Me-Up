using System.Collections.Generic;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "DropToLedge", menuName = "Dias Games/Abilities/DropToLedge", order = 0)]
    public class DropToLedge : ClimbAbility
    {
        private const float CapsuleCastHeight = 1f; 
        
        [SerializeField] private string _dropAnimState = "Climb.Drop to Ledge";
        [SerializeField][UnityTag] private List<string> _allowedTags = new List<string>{"Ledge", "Ladder"};
        [SerializeField] private float _animDuration = 1.0f;
        [SerializeField] private float _forwardCastDistance = 0.75f;
        [SerializeField] private float _matchTargetNormalizedStartTime = 0.3f;
        [SerializeField] private float _matchTargetNormalizedEndTime = 0.7f;
        
        [Header("Audio")]
        [SerializeField] private AudioClipContainer _grabAudioClips;

        private ClimbCastData _castData;
        private bool _matchingTarget = false;
        private bool _holdingDrop;
        private Animator _animator => AnimationController.Animator;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (Movement.IsFalling || !_holdingDrop)
            {
                return false;
            }

            Vector3 start = transform.position + transform.forward * _forwardCastDistance;
            if (Climbing.FindLedge(start, CapsuleCastHeight, -transform.forward,
                    _forwardCastDistance, out List<ClimbCastData> results, _allowedTags))
            {
                foreach (ClimbCastData castData in results)
                {
                    float dot = Vector3.Dot(transform.forward, castData.ForwardHit.normal);
                    if (dot < 0.7f)
                    {
                        continue;
                    }
                    
                    _castData = castData;
                    return true;
                }
            }

            return false;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_dropAnimState);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            Movement.DisableCollision();
            _matchingTarget = false;
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime >= _animDuration)
            {
                StopAbility(OwnerSystem.GameObject);
                OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
                return;
            }

            if (!_matchingTarget && elapsedTime > 0.15f)
            {
                Vector3 targetPosition = Climbing.GetCharacterPositionOnLedge(_castData);
                Quaternion targetRotation = Climbing.GetCharacterRotationOnLedge(_castData);
                MatchTargetWeightMask weightMask = new MatchTargetWeightMask(Vector3.one, 1.0f);
                
                _animator.MatchTarget(targetPosition, targetRotation, AvatarTarget.Root, 
                    weightMask, _matchTargetNormalizedStartTime, _matchTargetNormalizedEndTime);

                _matchingTarget = true;
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Movement.EnableCollision();
            Climbing.SetCurrentLedge(_castData);
            AudioPlayer.PlayEffect(_grabAudioClips);
        }
        
        public void SetHoldDrop(bool holding)
        {
            _holdingDrop = holding;
        }
    }
}