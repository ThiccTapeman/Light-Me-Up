using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "CrawlAbility", menuName = "Dias Games/Abilities/CrawlAbility", order = 0)]
    public class CrawlAbility : Ability
    {
        [SerializeField] private float _crawlSpeed = 1f;
        [SerializeField] private float _capsuleHeightOnCrawl = 0.5f;
        [Header("Animation States")]
        [SerializeField] private string _startCrawlAnimationState = "Stand to Crawl";
        [SerializeField] private string _stopCrawlAnimationState = "Crawl to Stand";

        [Header("Cast Parameters")]
        [SerializeField] private LayerMask _obstaclesMask;
        [Tooltip("This is the height that sphere cast can reach to know when should force crawl state")]
        [SerializeField] private float _maxHeightToStartCrawl = 0.75f;

        private bool _startingCrawl = false;
        private bool _stoppingCrawl = false;
        private bool _crawlPressed = false;

        private float _defaultCapsuleRadius = 0;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _defaultCapsuleRadius = Movement.CapsuleRadius;
        }

        public void ToggleCrawl()
        {
            if (_crawlPressed && ForceCrawlByHeight())
            {
                return;
            }

            _crawlPressed = !IsRunning;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_startCrawlAnimationState);
            Movement.StopMovement();
            Movement.SetCapsuleSize(_capsuleHeightOnCrawl, Movement.CapsuleRadius);
            Movement.SetMaxMoveSpeed(_crawlSpeed);

            _startingCrawl = true;
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (_startingCrawl)
            {
                if (AnimationController.Animator.IsInTransition(0)) return;

                if (!AnimationController.Animator.GetCurrentAnimatorStateInfo(0).IsName(_startCrawlAnimationState))
                {
                    _startingCrawl = false;
                }

                return;
            }

            if (!_crawlPressed)
            {
                if (!_stoppingCrawl)
                {
                    _stoppingCrawl = true;
                    AnimationController.SetAnimationState(_stopCrawlAnimationState);
                    Movement.StopMovement();
                }

                if (AnimationController.HasFinishedAnimation(_stopCrawlAnimationState))
                {
                    StopAbility(OwnerSystem.GameObject);
                }
                
                return;
            }

            Movement.MoveByInput();
            Movement.RotateToMovementDirection(0.75f);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            _startingCrawl = false;
            _stoppingCrawl = false;

            Movement.ResetCapsuleSize();
        }

        private bool ForceCrawlByHeight()
        {
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, _defaultCapsuleRadius, Vector3.up, out hit,
                    _maxHeightToStartCrawl, _obstaclesMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.point.y - transform.position.y > _capsuleHeightOnCrawl)
                    return true;
            }

            return false;
        }
    }
}