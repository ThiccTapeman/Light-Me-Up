using System;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Components
{
    /// <summary>
    /// Struct that stores launch properties
    /// </summary>
    public struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly Vector3 target;
        public readonly float timeToTarget;
        public readonly bool foundSolution;

        public LaunchData(Vector3 launchVelocity, Vector3 targetPoint, float timeToReach, bool found)
        {
            initialVelocity = launchVelocity;
            target = targetPoint;
            timeToTarget = timeToReach;
            foundSolution = found;
        }

        public static LaunchData Empty()
        {
            return new LaunchData(Vector3.zero, Vector3.zero, 0f, false);
        }
    }

    [System.Serializable]
    public struct JumpParameters
    {
        public float minJumpHeight;
        public float maxJumpHeight;
        public float horizontalSpeed;

        public JumpParameters(float minJumpHeight, float maxJumpHeight, float horizontalSpeed)
        {
            this.minJumpHeight = minJumpHeight;
            this.maxJumpHeight = maxJumpHeight;
            this.horizontalSpeed = horizontalSpeed;
        }

        public float GetVerticalSpeed(float gravity)
        {
            return Mathf.Sqrt(-2 * gravity * maxJumpHeight);
        }
    }

    public enum JumpResultType
    {
        ChooseBest,
        Faster,
        Highest
    }

    public class LaunchComponent : MonoBehaviour
    {
        IMovement _movement;

        private float Gravity => _movement?.Gravity ?? Physics.gravity.y;

        private void Awake()
        {
            _movement = GetComponent<IMovement>();
        }

        /// <summary>
        /// Calculate velocity to reach desired point
        /// </summary>
        /// <returns>Data for the launch</returns>
        public LaunchData CalculateLaunchData(Vector3 startPoint, Vector3 targetPoint, 
            JumpParameters parameter, JumpResultType resultType = JumpResultType.ChooseBest)
        {
            LaunchData nullData = new LaunchData(Vector3.zero, targetPoint, -1, false);

            // Full displacement
            Vector3 Displacement = targetPoint - startPoint;

            // Organize by vertical and horizontal displacements
            float displacementY = Displacement.y;
            Vector3 displacementXZ = new Vector3(Displacement.x, 0, Displacement.z);

            // Check if target point is too high
            // When target point is higher than character maximum jump height, it means that point is not reachable
            if (displacementY - parameter.maxJumpHeight > 0)
                return nullData;

            // Get a jump height if target point is between min height and maximum height
            float m_JumpHeight = Mathf.Clamp(displacementY, parameter.minJumpHeight, parameter.maxJumpHeight);

            // Time to reach point
            // time: Time using the maximum height jump
            float time = Mathf.Sqrt(-2 * parameter.maxJumpHeight / Gravity) +
                Mathf.Sqrt(2 * (displacementY - parameter.maxJumpHeight) / Gravity);

            // timeLower: Time using height of the ledge
            float timeLower = Mathf.Sqrt(-2 * m_JumpHeight / Gravity) +
                Mathf.Sqrt(2 * (displacementY - m_JumpHeight) / Gravity);

            // Velocities for each time calculated
            Vector3 velocity = displacementXZ / time;
            Vector3 velocityLower = displacementXZ / timeLower;

            // If velocity is greater than maximum horizontal speed, means that this launch is not possible
            if (velocity.magnitude > parameter.horizontalSpeed)
                return nullData;

            // Check which launch to use
            bool useLower = false;
            switch (resultType)
            {
                case JumpResultType.ChooseBest:
                    useLower = timeLower < time && velocityLower.magnitude <= parameter.horizontalSpeed;
                    break;
                case JumpResultType.Faster:
                    useLower = true;
                    break;
                case JumpResultType.Highest:
                    useLower = false;
                    break;
            }

            // Set vertical speed
            float vy = Mathf.Sqrt(-2 * Gravity * (useLower ? m_JumpHeight : parameter.maxJumpHeight));

            // Get final velocity
            Vector3 finalVelocity = (useLower) ? velocityLower : velocity;
            finalVelocity.y = vy * -Mathf.Sign(Gravity);

            return new LaunchData(finalVelocity, targetPoint, (useLower) ? timeLower : time, true);
        }
    }
}