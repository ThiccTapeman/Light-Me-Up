using UnityEngine;

namespace ThiccTapeman.Utils
{
    public static class Vector2Utils
    {
        /// <summary>
        /// This will take an position and rotate it around 0 with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector2 RotatePositionAroundOrigin(Vector2 position, Vector2 rotation)
        {
            return RotatePositionAroundOrigin(position, Vector2.zero, rotation);
        }

        /// <summary>
        /// This will take an position and rotate it around an origin with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotationOrigin">The rotation origin</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector2 RotatePositionAroundOrigin(Vector2 position, Vector2 rotationOrigin, Vector2 rotation)
        {
            position = position - rotationOrigin;

            Quaternion quaternionRotation = Quaternion.Euler(rotation);

            Vector2 rotatedPosition = quaternionRotation * position;

            return rotatedPosition;
        }
    }
}