using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Components;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Abilities
{
    [CreateAssetMenu(fileName = "ClimbFreeSidePredictedJump", menuName = "Dias Games/Abilities/ClimbFreeSidePredictedJump", order = 0)]
    public class ClimbFreeSidePredictedJump : ClimbFreeSideJump
    {
        [SerializeField] private JumpParameters _launchJumpParams;

        private PredictedJumpComponent _predictedJumpComponent;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _predictedJumpComponent = ownerSystem.GameObject.GetComponent<PredictedJumpComponent>();
        }

        protected override void LaunchCharacter()
        {
            JumpType jumpType =
                _predictedJumpComponent.GetLaunchParameters(_direction, _launchJumpParams, out LaunchData launchData);

            if (jumpType == JumpType.JumpWithPrediction)
            {
                Vector3 velocity = launchData.initialVelocity;
                Movement.SetGravityEnabled(false);
                Movement.Move(velocity);
                Movement.SetGravityEnabled(true);

                Vector3 newDirection = (launchData.target - transform.position).GetNormal2D();
                Quaternion targetRotation = Quaternion.LookRotation(newDirection);
                const float lerpDuration = 0.3f;
                Tweener.DoRotationLerp(targetRotation, lerpDuration);
                return;
            }

            base.LaunchCharacter();
        }
    }
}