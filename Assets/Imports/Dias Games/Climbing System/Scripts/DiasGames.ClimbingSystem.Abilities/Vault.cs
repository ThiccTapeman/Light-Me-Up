using System.Collections.Generic;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "Vault", menuName = "Dias Games/Abilities/Vault", order = 0)]
    public class Vault : ClimbAbility
    {
        [SerializeField][UnityTag] private string _vaultObjectTag = "Vault";
        [SerializeField] private string _vaultAnimState = "Vault";
        [SerializeField] private float _maxVaultCastDistance = 0.5f;
        [SerializeField] private float _maxVaultHeight = 1.2f;
        [SerializeField] private float _maxVaultDepth = 1.2f;
        [SerializeField] private float _vaultDuration = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipContainer _vaultVocals;

        private ClimbCastData _frontCastData;
        private ClimbCastData _backCastData;

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (Movement.IsFalling)
            {
                return false;
            }

            return HasVault();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            Vector3 target = _backCastData.ForwardHit.point + _backCastData.ForwardHit.normal * Movement.CapsuleRadius;
            target.y = GetGroundYPos(target);
            Movement.DisableCollision();
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);

            Vector3 direction = (target - transform.position).GetNormal2D();
            float distance = Vector3.Distance(target, transform.position);

            Vector3 midPoint = transform.position + direction * (distance * 0.5f) + Vector3.up * 0.5f; 
            
            Tweener.DoBezier(target, transform.rotation, midPoint,_vaultDuration);
            AnimationController.SetAnimationState(_vaultAnimState);
            AudioPlayer.PlayVoice(_vaultVocals);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float elapsedTime = Time.time - StartTime;
            if (elapsedTime >= _vaultDuration)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            Movement.EnableCollision();
            Movement.SetGravityEnabled(true);
        }

        private bool HasVault()
        {
            Vector3 frontOrigin = transform.position + Vector3.up * (_maxVaultHeight * 0.5f);

            if (!Climbing.FindLedge(frontOrigin, _maxVaultHeight, transform.forward,
                    _maxVaultCastDistance, out List<ClimbCastData> frontResults,
                    new List<string> { _vaultObjectTag }, checkFreeToClimb: false))
            {
                return false;
            }


            foreach (ClimbCastData frontResult in frontResults)
            {
                if (IsVaultValid(frontResult, frontOrigin))
                {
                    _frontCastData = frontResult;
                    return true;
                }
            }

            return false;
        }

        private bool IsVaultValid(ClimbCastData frontCastData, Vector3 frontOrigin)
        {
            float height = frontCastData.TopHit.point.y - transform.position.y;
            if (height > _maxVaultHeight)
            {
                return false;
            }
            
            Vector3 backOrigin = frontOrigin + transform.forward * (_maxVaultCastDistance * 2.0f + _maxVaultDepth);
            if (!Climbing.FindLedge(backOrigin, _maxVaultHeight, -transform.forward,
                    _maxVaultCastDistance * 2.0f + _maxVaultDepth, out List<ClimbCastData> backResults,
                    new List<string> { _vaultObjectTag }, checkFreeToClimb: false))
            {
                return false;
            }


            const float castRadius = 0.15f;
            Vector3 startCast = transform.position + Vector3.up * (_maxVaultHeight + castRadius);
            foreach (ClimbCastData backResult in backResults)
            {
                if (backResult.Ledge != frontCastData.Ledge)
                {
                    continue;
                }

                float distance = Vector3.Distance(backResult.ForwardHit.point, frontCastData.ForwardHit.point);
                if (distance > _maxVaultDepth)
                {
                   continue;
                }

                Vector3 direction = backResult.ForwardHit.point - frontCastData.ForwardHit.point;
                float castDistance = _maxVaultDepth + Movement.CapsuleRadius;
                if (Physics.SphereCast(startCast, castRadius, direction.normalized, out RaycastHit info, 
                        castDistance, Climbing.BlockMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                _backCastData = backResult;
                return true;
            }

            return false;
        }

        private float GetGroundYPos(Vector3 origin)
        {
            float targetY = transform.position.y;
            if (Physics.Raycast(new Vector3(origin.x, _backCastData.TopHit.point.y, origin.z), Vector3.down,
                    out RaycastHit groundHit,_maxVaultHeight * 2.0f, Movement.GroundLayerMask))
            {
                targetY = groundHit.point.y;
            }

            return targetY;
        }

    }
}