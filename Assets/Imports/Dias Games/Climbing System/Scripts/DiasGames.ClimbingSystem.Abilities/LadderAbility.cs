using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "LadderAbility", menuName = "Dias Games/Abilities/LadderAbility", order = 0)]
    public class LadderAbility : ClimbAbility
    {
        private const float Threshold = 0.05f;
        
        [SerializeField] private string _ladderAnimState = "Ladder";
        [SerializeField] private string _ladderVerticalParam = "Vertical Ladder";
        [SerializeField][UnityTag] private string _ladderTag = "Ladder";

        private ClimbCastData _castData;
        private Ladder _ladder;
        private Ladder _lastLadder;

        private Animator Animator => AnimationController.Animator;
        private bool _stoppedByItself = false;

        public override bool CanStart()
        {
            if (_lastLadder)
            {
                if (Climbing.CurrentLedge == null || 
                    Climbing.CurrentLedge != _lastLadder.gameObject)
                {
                    _lastLadder = null;
                }
            }
            
            if (Time.time - StopTime < 0.2f)
            {
                return false;
            }
            
            if (!base.CanStart() || !Movement.IsFalling)
            {
                return false;
            }

            bool found = Climbing.FindLadder(out _castData, _ladderTag);
            if (found &&_lastLadder && _castData.Ledge == _lastLadder.gameObject)
            {
                found = false;
            }

            return found;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            AnimationController.SetAnimationState(_ladderAnimState);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            _ladder = _castData.ForwardHit.collider.GetComponent<Ladder>();
            
            Tweener.DoLerp(Climbing.GetCharacterPositionOnLedge(_castData),
                Climbing.GetCharacterRotationOnLedge(_castData),
                0.1f);
            Climbing.SetCurrentLedge(_castData);
            _stoppedByItself = false;
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Tweener.IsRunningTween)
            {
                return;
            }

            float vertical = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.forward);
            if (vertical > 0)
            {
                float distanceToTop = _ladder.TopLimit.transform.position.y - Climbing.CastOrigin.y;
                if (distanceToTop <= Threshold)
                {
                    vertical = 0;
                    if (_ladder.CanClimbUp)
                    {
                        _stoppedByItself = true;
                        StopAbility(transform.gameObject);
                        OwnerSystem.StartAbilityByName("Climb Up", transform.gameObject);
                        return;
                    }
                }
            }
            else if (vertical < 0)
            {
                bool hasGround = Physics.Raycast(transform.position, Vector3.down, 0.25f, Climbing.BlockMask);
                if (!hasGround)
                {
                    float distanceToBottom = Climbing.CastOrigin.y - _ladder.BottomLimit.transform.position.y;
                    if (distanceToBottom <= Threshold)
                    {
                        vertical = 0;
                    }
                }

                if (!Movement.IsFalling)
                {
                    _stoppedByItself = true;
                    StopAbility(transform.gameObject);
                    return;
                }
            }

            UpdateAnimParameter(vertical);
            UpdatePosition();
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            UpdateAnimParameter(0.0f);
            Movement.SetGravityEnabled(true);

            if (!_stoppedByItself)
            {
                _lastLadder = _ladder;
            }
        }

        private void UpdateAnimParameter(float verticalValue)
        {
            verticalValue = Mathf.Round(verticalValue);
            
            Animator.SetFloat(_ladderVerticalParam, 
                verticalValue,
                0.1f, Time.deltaTime);
        }

        private void UpdatePosition()
        {
            Vector3 position = Climbing.GetCharacterPositionOnLedge(_castData);
            position.y = transform.position.y;
            Movement.SetPosition(position);
        }
    }
}