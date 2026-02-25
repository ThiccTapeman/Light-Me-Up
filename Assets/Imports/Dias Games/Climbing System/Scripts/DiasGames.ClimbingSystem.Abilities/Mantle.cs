using System.Collections.Generic;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "Mantle", menuName = "Dias Games/Abilities/Mantle", order = 0)]
    public class Mantle : ClimbAbility
    {
        [SerializeField][UnityTag] private string _mantleTag = "Mantle";
        [SerializeField] private string _mantleAnimState = "Mantle";
        [SerializeField] private float _abilityDuration = 1.0f;
        [SerializeField] private float _minHeight = 0.5f;
        [SerializeField] private float _maxHeight = 1.3f;
        [SerializeField] private float _maxDistance = 0.5f;
        [SerializeField] private Vector3 _mantleTargetOffset;
        [SerializeField] private float _matchStartNormalizedTime = 0.35f;
        [SerializeField] private float _matchEndNormalizedTime = 0.85f;

        [Header("Audio")]
        [SerializeField] private AudioClipContainer _mantleClips;

        private Vector3 _targetMantlePos;
        private Quaternion _targetMantleRot;
        private bool _isMatching = false;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (!Movement.IsFalling)
            {
                return false;
            }

            return FindMantle();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_mantleAnimState);
            Movement.DisableCollision();
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            _isMatching = false;
            
            AudioPlayer.PlayEffect(_mantleClips);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime >= _abilityDuration)
            {
                StopAbility(OwnerSystem.GameObject);
                return;
            }

            if (_isMatching)
            {
                return;
            }

            if (elapsedTime >= 0.15f)
            {
                AnimationController.Animator.MatchTarget(_targetMantlePos, _targetMantleRot, AvatarTarget.Root, 
                    new MatchTargetWeightMask(Vector3.one, 1.0f), _matchStartNormalizedTime, _matchEndNormalizedTime);
                _isMatching = true;
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            Movement.EnableCollision();
            Movement.SetGravityEnabled(true);
        }

        private bool FindMantle()
        {
            float capsuleHeight = _maxHeight - _minHeight;
            Vector3 castOrigin = transform.position + Vector3.up * ((_minHeight + _maxHeight) / 2);
            if (Climbing.FindLedge(castOrigin, capsuleHeight*2, transform.forward,
                    _maxDistance, out List<ClimbCastData> results, new List<string>{ _mantleTag }, debug:true, checkFreeToClimb:false))
            {
                foreach (ClimbCastData result in results)
                {
                    Vector3 mantlePos = GetMantleFinalPos(result);

                    if (IsMantleValid(mantlePos, result.Ledge))
                    {
                        _targetMantlePos = mantlePos;
                        _targetMantleRot = Climbing.GetCharacterRotationOnLedge(result);
                        return true;
                    }
                }
            }

            return false;
        }

        private Vector3 GetMantleFinalPos(ClimbCastData castData)
        {
            Vector3 targetPos = castData.ForwardHit.point - castData.ForwardHit.normal * _mantleTargetOffset.z;
            targetPos.y = castData.TopHit.point.y + _mantleTargetOffset.y;

            return targetPos;
        }

        private bool IsMantleValid(Vector3 mantlePos, GameObject mantleObject)
        {
            if (!mantleObject.CompareTag(_mantleTag))
            {
                return false;
            }
            
            float distance = mantlePos.y - transform.position.y;
            if (distance < _minHeight || distance > _maxHeight)
            {
                return false;
            }

            Vector3 cp1 = mantlePos + Vector3.up * (Movement.CapsuleRadius + 0.1f);
            Vector3 cp2 = cp1 + Vector3.up * (Movement.CapsuleHeight - Movement.CapsuleRadius * 2.0f);
            bool foundObstacle = Physics.CheckCapsule(cp1, cp2, Movement.CapsuleRadius,
                Climbing.BlockMask, QueryTriggerInteraction.Ignore);

            CastDebug.Instance.DrawCapsule(cp1, cp2, Movement.CapsuleRadius, foundObstacle ? Color.red : Color.green);

            return !foundObstacle;
        }
    }
}