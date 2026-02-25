using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbLookBack", menuName = "Dias Games/Abilities/ClimbLookBack", order = 0)]
    public class ClimbLookBack : ClimbAbility
    {
        [SerializeField] private string _lookBackAnimState = "Climb.Looking back";

        private const float BackInputThreshold = -0.2f;
        private ClimbCastData _castData;

        public override bool CanStart()
        {
            if (!base.CanStart() || Climbing.IsHanging)
            {
                return false;
            }

            return Climbing.HasCurrentLedge(out _castData);
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            base.OnStartAbility(instigator);

            AnimationController.SetAnimationState(_lookBackAnimState);
            Movement.StopMovement();
            Movement.SetGravityEnabled(false);
            Climbing.LedgeChild.position = Climbing.GetCharacterPositionOnLedge(_castData);
            Climbing.LedgeChild.rotation = Climbing.GetCharacterRotationOnLedge(_castData);
            Climbing.LedgeChild.SetParent(_castData.Ledge.transform);
        }

        public override void UpdateAbility(float deltaTime)
        {
            float backInput = Vector3.Dot(Movement.GetWorldDirectionInput(), transform.forward);
            if (backInput > BackInputThreshold)
            {
                StopAbility(OwnerSystem.GameObject);
                OwnerSystem.StartAbilityByName("Climb Idle", OwnerSystem.GameObject);
                return;
            }

            Movement.SetPosition(Climbing.LedgeChild.position);
            Movement.SetRotation(Climbing.LedgeChild.rotation);
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            base.OnStopAbility(instigator);
            Movement.SetGravityEnabled(true);
            Climbing.LedgeChild.SetParent(null);
        }
    }
}