using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbIdle", menuName = "Dias Games/Abilities/ClimbIdle", order = 0)]
    public class ClimbIdle : ClimbAbility
    {
        private const float AllowedTimeWithoutLedge = 0.1f;

        [SerializeField] private string _climbIdleAnimState = "Climb.Idle";
        [SerializeField] private string _charRelativeHorParam = "CharRelativeHorizontal";
        [SerializeField] private string _charRelativeVerParam = "CharRelativeVertical";

        [Header("Climb Abilities Names")] 
        [SerializeField] private string _climbCornerAbilityName = "Climb Corner";
        [SerializeField] private string _climbLookBackAbilityName = "Climb Look Back";

        private float _lastTimeWithLedge;
        private Vector2 _relativeInput;
        private IRootMotion _rootMotion;
        private Transform _charCached;
        private bool _adjustPosition;
        private ClimbCastData _castData;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _rootMotion = ownerSystem.GameObject.GetComponent<IRootMotion>();
            _charCached = new GameObject("Character Cached GO (Don't destroy it!!)").transform;
        }

        public override bool CanStart()
        {
            return base.CanStart() && Climbing.HasCurrentLedge(out _castData);
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            _adjustPosition = false;
            AnimationController.SetAnimationState(_climbIdleAnimState);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);

            if (_castData.HasLedge)
            {
                Vector3 targetPosition = Climbing.GetCharacterPositionOnLedge(_castData);
                Quaternion targetRotation = Climbing.GetCharacterRotationOnLedge(_castData);
                Tweener.DoLerp(targetPosition, targetRotation, 0.2f, _castData.Ledge.transform);
                Climbing.SetCurrentLedge(_castData);
                UpdateLedgeChild(_castData);
                _adjustPosition = true;
            }

            _charCached.position = transform.position;
            _charCached.rotation = transform.rotation;
            _lastTimeWithLedge = Time.time;
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (Tweener.IsRunningTween)
            {
                _charCached.position = transform.position;
                _charCached.rotation = transform.rotation;
                return;
            }

            _relativeInput.x = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.right);
            _relativeInput.y = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.forward);

            if (Mathf.Abs(_relativeInput.x) < 0.2f && _relativeInput.y < -0.5f)
            {
                if (OwnerSystem.StartAbilityByName(_climbLookBackAbilityName, OwnerSystem.GameObject))
                {
                    return;
                }
            }

            if (!Mathf.Approximately(_relativeInput.x, 0f))
            {
                bool canShimmy = Climbing.CanShimmy(_relativeInput.x);
                if (!canShimmy)
                {
                    _rootMotion.StopRootMotion();
                    if (OwnerSystem.StartAbilityByName(_climbCornerAbilityName, OwnerSystem.GameObject))
                    {
                        return;
                    }
                }

                _relativeInput.x = canShimmy ? _relativeInput.x : 0;
            }

            UpdateAnimParameters(_relativeInput);

            if (_adjustPosition)
            {
                Vector3 deltaPos = transform.position - _charCached.position;
                Movement.SetPosition(Climbing.LedgeChild.position + deltaPos);
                Movement.SetRotation(Quaternion.Euler(0, Climbing.LedgeChild.eulerAngles.y, 0));
                _charCached.position = transform.position;
                _charCached.rotation = transform.rotation;
            }

            _adjustPosition = false;
            if (Climbing.HasCurrentLedge(out _castData))
            {
                UpdateLedgeChild(_castData);
                Climbing.SetCharacterTransformOnLedge(_castData);
                _lastTimeWithLedge = Time.time;
                _adjustPosition = true;
                Climbing.SetCurrentLedge(_castData);
            }
            else if(Time.time - _lastTimeWithLedge > AllowedTimeWithoutLedge)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        private void UpdateAnimParameters(Vector2 relativeInput, float dampTime = 0.1f)
        {
            AnimationController.Animator.SetFloat(_charRelativeHorParam,
                relativeInput.x,
                dampTime, Time.deltaTime);

            AnimationController.Animator.SetFloat(_charRelativeVerParam,
                relativeInput.y,
                dampTime, Time.deltaTime);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            UpdateAnimParameters(Vector2.zero, 0.0f);
            Climbing.LedgeChild.SetParent(null);
        }

        private void UpdateLedgeChild(ClimbCastData castData)
        {
            if (Climbing.LedgeChild.parent == null ||
                Climbing.LedgeChild.parent != castData.Ledge.transform)
            {
                Climbing.LedgeChild.SetParent(castData.Ledge.transform);
            }

            Climbing.LedgeChild.position = Climbing.GetCharacterPositionOnLedge(castData);
            Climbing.LedgeChild.rotation = Climbing.GetCharacterRotationOnLedge(castData);
        }
    }
}