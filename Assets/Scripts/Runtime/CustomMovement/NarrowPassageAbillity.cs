using DiasGames.AbilitySystem.Components;
using DiasGames.AbilitySystem.Core;
using DiasGames.AbilitySystem.Traversal;
using UnityEngine;

namespace DiasGames.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "NarrowPassageAbility", menuName = "Dias Games/Abilities/NarrowPassageAbility", order = 0)]
    public class NarrowPassageAbility : Ability
    {
        private const float InputThreshold = 0.05f;
        private const float EndThreshold = 0.98f;

        [Header("Animation")]
        [SerializeField] private string _animationState = "Narrow Passage";
        [SerializeField] private string _speedParam = "Speed";

        [Header("Settings")]
        [SerializeField] private float _facingYawOffset = 0f;
        [SerializeField] private float _exitOffset = 0.25f;
        [SerializeField] private float _reverseInputThreshold = 0.25f;

        [SerializeField] private float _reenterCooldown = 0.25f;

        private NarrowPassageDetector _detector;
        private NarrowPassage _currentPassage;
        private NarrowPassage _blockedPassageAfterExit;
        private Vector3 _pathDirection;
        private float _currentProgress;

        private float _lastStopTime;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);

            Movement = ownerSystem.GameObject.GetComponent<IMovement>();
            _detector = ownerSystem.GameObject.GetComponent<NarrowPassageDetector>();
        }

        public override bool CanStart()
        {
            if (!base.CanStart())
            {
                return false;
            }

            if (_detector != null)
            {
                if (_detector.CurrentPassage == null)
                {
                    _blockedPassageAfterExit = null;
                }
                else if (_detector.CurrentPassage == _blockedPassageAfterExit)
                {
                    return false;
                }
            }

            if (Time.time - _lastStopTime < _reenterCooldown)
            {
                return false;
            }

            if (Movement.IsFalling || _detector == null || _detector.CurrentPassage == null)
            {
                return false;
            }

            NarrowPassage passage = _detector.CurrentPassage;
            Vector3 input = Movement.GetWorldDirectionInput();

            if (input.sqrMagnitude <= InputThreshold * InputThreshold)
            {
                return false;
            }

            Vector3 forward = passage.Forward;
            float dot = Vector3.Dot(input.normalized, forward);

            return Mathf.Abs(dot) > 0.5f;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            _currentPassage = _detector.CurrentPassage;
            if (_currentPassage == null)
            {
                StopAbility(instigator);
                return;
            }

            Vector3 forward = _currentPassage.Forward;
            float startProgress = _currentPassage.GetNormalizedProgress(transform.position);
            _pathDirection = startProgress <= 0.5f ? forward : -forward;

            // If entering very close to center, fallback to input to resolve intended side.
            if (Mathf.Abs(startProgress - 0.5f) <= 0.05f)
            {
                Vector3 input = Movement.GetWorldDirectionInput();
                if (input.sqrMagnitude > InputThreshold * InputThreshold)
                {
                    float dot = Vector3.Dot(input.normalized, forward);
                    _pathDirection = dot >= 0f ? forward : -forward;
                }
            }

            AnimationController.SetAnimationState(_animationState, transitionDuration: 0.15f);
            AnimationController.Animator.SetFloat(_speedParam, 0f);

            Movement.SetCapsuleSize(_currentPassage.CapsuleHeight, Movement.CapsuleRadius);
            Movement.SetMaxMoveSpeed(_currentPassage.MoveSpeed);
            Movement.StopMovement();
            _currentProgress = startProgress;
            Vector3 startLockedPos = _currentPassage.GetPointOnWallPath(_currentProgress, transform.position.y);
            Movement.SetPosition(startLockedPos);
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (_currentPassage == null || (_detector != null && _detector.CurrentPassage != _currentPassage))
            {
                StopAbility(OwnerSystem.GameObject);
                return;
            }

            Vector3 input = Movement.GetWorldDirectionInput();
            float moveSign = ResolveMoveSign(input);
            bool reachedHighEnd = _currentProgress >= EndThreshold;
            bool reachedLowEnd = _currentProgress <= 1f - EndThreshold;
            float alongPassageForward = input.sqrMagnitude > InputThreshold * InputThreshold
                ? Vector3.Dot(input.normalized, _currentPassage.Forward)
                : 0f;

            // If player is already at an edge and is pushing out of the passage, exit immediately.
            if ((reachedHighEnd && alongPassageForward > InputThreshold) ||
                (reachedLowEnd && alongPassageForward < -InputThreshold))
            {
                SnapToExitPosition(reachedHighEnd);
                StopAbility(OwnerSystem.GameObject);
                return;
            }

            if (Mathf.Abs(moveSign) <= InputThreshold)
            {
                AnimationController.Animator.SetFloat(_speedParam, 0f, 0.1f, deltaTime);
                Vector3 idlePos = _currentPassage.GetPointOnWallPath(_currentProgress, transform.position.y);
                Movement.SetPosition(idlePos);

                // At edges, release ability immediately instead of leaving a locked middle state.
                if (reachedHighEnd || reachedLowEnd)
                {
                    SnapToExitPosition(reachedHighEnd);
                    StopAbility(OwnerSystem.GameObject);
                }

                return;
            }

            float blendSpeed = Mathf.Clamp(moveSign, -1f, 1f);
            AnimationController.Animator.SetFloat(_speedParam, blendSpeed, 0.1f, deltaTime);
            float pathLength = Mathf.Max(_currentPassage.PathLength, 0.01f);
            float normalizedStep = (_currentPassage.MoveSpeed * deltaTime) / pathLength;
            float direction = Mathf.Sign(moveSign);
            float previousProgress = _currentProgress;
            _currentProgress = Mathf.Clamp01(_currentProgress + direction * normalizedStep);

            Vector3 travelDirection = _pathDirection * direction;
            bool movingTowardHighEnd = Vector3.Dot(travelDirection, _currentPassage.Forward) > 0f;
            bool movingTowardLowEnd = !movingTowardHighEnd;

            Vector3 nextPos = _currentPassage.GetPointOnWallPath(_currentProgress, transform.position.y);
            Movement.SetPosition(nextPos);

            reachedHighEnd = _currentProgress >= EndThreshold;
            reachedLowEnd = _currentProgress <= 1f - EndThreshold;
            bool shouldExit =
                (movingTowardHighEnd && reachedHighEnd) ||
                (movingTowardLowEnd && reachedLowEnd);

            if (shouldExit)
            {
                SnapToExitPosition(reachedHighEnd);
                StopAbility(OwnerSystem.GameObject);
                return;
            }

            // Safety net: if clamped at edge while still trying to move outward, force exit.
            bool stuckAtEdge = Mathf.Approximately(previousProgress, _currentProgress) &&
                               ((reachedHighEnd && movingTowardHighEnd) || (reachedLowEnd && movingTowardLowEnd));
            if (stuckAtEdge)
            {
                SnapToExitPosition(reachedHighEnd);
                StopAbility(OwnerSystem.GameObject);
                return;
            }

            Vector3 rotationDirection = _currentPassage.Forward;
            Quaternion target = Movement.GetRotationFromDirection(rotationDirection);
            target *= Quaternion.Euler(0f, _facingYawOffset, 0f);
            Movement.SetRotation(target);
        }

        private float ResolveMoveSign(Vector3 input)
        {
            if (input.sqrMagnitude <= InputThreshold * InputThreshold)
            {
                return 0f;
            }

            float aligned = Vector3.Dot(input.normalized, _pathDirection);

            // Default to forward traversal when any movement input is pressed.
            // Require stronger opposite intent to walk back.
            if (aligned <= -_reverseInputThreshold)
            {
                return -1f;
            }

            return 1f;
        }

        private void SnapToExitPosition(bool reachedHighEnd)
        {
            if (_currentPassage == null)
            {
                return;
            }

            Transform edgePoint = reachedHighEnd ? _currentPassage.ExitPoint : _currentPassage.EntryPoint;
            if (edgePoint == null)
            {
                return;
            }

            Vector3 outward = reachedHighEnd ? _currentPassage.Forward : -_currentPassage.Forward;
            Vector3 exitPos = edgePoint.position + outward * _exitOffset;
            exitPos.y = transform.position.y;
            Movement.SetPosition(exitPos);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            _blockedPassageAfterExit = _currentPassage;
            AnimationController.Animator.SetFloat(_speedParam, 0f);
            Movement.ResetCapsuleSize();
            Movement.SetMaxMoveSpeed(0f); // remove if locomotion sets it on start
            _currentPassage = null;
            _lastStopTime = Time.time;
        }
    }
}
