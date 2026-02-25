using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "CheckLedges", menuName = "Dias Games/Abilities/CheckLedges", order = 0)]
    public class CheckLedges : ClimbAbility
    {
        public override bool CanStart()
        {
            return base.CanStart() && Movement.IsFalling;
        }

        protected override void OnStartAbility(GameObject instigator)
        { }

        public override void UpdateAbility(float deltaTime)
        {
            if (Climbing.FindLedge(out ClimbCastData data))
            {
                OwnerSystem.StartAbilityByName("StartClimb", OwnerSystem.GameObject);
            }

            if (!Movement.IsFalling)
            {
                StopAbility(OwnerSystem.GameObject);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        { }
    }
}