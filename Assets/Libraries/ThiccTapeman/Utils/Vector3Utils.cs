using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThiccTapeman.Utils
{
    public static class Vector3Utils
    {
        // ------------------------------------------------------------- //
        // Vector3 Projection on plane                                   //
        // ------------------------------------------------------------- //
        #region Projection

        /// <summary>
        /// This will project a 3d point onto a 3d plane
        /// </summary>
        /// <param name="planePoint">A point on the plane</param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <param name="vector">The vector that should be projected</param>
        /// <returns>The projected vector</returns>
        public static Vector3 ProjectVectorOntoPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 vector)
        {
            planePoint.z *= -1;

            // Calculate the component of the vector parallel to the plane normal
            float dotProduct = Vector3.Dot(vector + planePoint, planeNormal);
            Vector3 parallelComponent = dotProduct * planeNormal;

            // Subtracting the parallel component from the original vector gives the projected vector
            Vector3 projectedVector = vector - parallelComponent;

            return projectedVector;
        }

        /// <summary>
        /// This will project an 3d position onto a plane and then turn it into a 2d vector where the planePoint is (0, 0)
        /// </summary>
        /// <param name="planePoint">Where the center of the 2d vector should be</param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <param name="vector">The vector that should be projected onto the plane</param>
        /// <returns>Returns the newly projected 2d relative position</returns>
        public static Vector2 ProjectVectorOntoPlaneRelative(Vector3 planePoint, Vector3 planeNormal, Vector3 vector)
        {
            Vector3 relativePosition = ProjectVectorOntoPlane(planePoint, planeNormal, vector);

            Vector3 rightVector;
            Vector3 upVector;

            if (planeNormal == new Vector3(0, -1, 0)) rightVector = new Vector3(-1, 0, 0);
            else rightVector = Vector3.Cross(planeNormal, Vector3.up).normalized;

            if (planeNormal == new Vector3(0, -1, 0)) upVector = new Vector3(0, 0, 1);
            else upVector = Vector3.Cross(rightVector, planeNormal).normalized;

            float xCoordinate = Vector3.Dot(relativePosition, -rightVector);
            float yCoordinate = Vector3.Dot(relativePosition, upVector);

            Vector3 rotation = CalculateEulerAnglesFromNormalizedNormal(planeNormal);

            Vector2 position = new Vector2(xCoordinate, yCoordinate);

            return position;
        }

        #endregion
        // ------------------------------------------------------------- //
        // Rotatation                                                    //
        // ------------------------------------------------------------- //
        #region Rotation

        /// <summary>
        /// This will take an position and rotate it around 0 with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector3 RotatePositionAroundOrigin(Vector3 position, Vector3 rotation)
        {
            return RotatePositionAroundOrigin(position, Vector3.zero, rotation);
        }

        /// <summary>
        /// This will take an position and rotate it around an origin with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotationOrigin">The rotation origin</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector3 RotatePositionAroundOrigin(Vector3 position, Vector3 rotationOrigin, Vector3 rotation)
        {
            position = position - rotationOrigin;

            Quaternion quaternionRotation = Quaternion.Euler(rotation);

            Vector3 rotatedPosition = quaternionRotation * position;

            return rotatedPosition;
        }

        /// <summary>
        /// This will take an position and rotate it around 0 with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector3 RotatePositionAroundOrigin(Vector3 position, Quaternion rotation)
        {
            return RotatePositionAroundOrigin(position, Vector3.zero, rotation);
        }

        /// <summary>
        /// This will take an position and rotate it around an origin with an 3d rotation
        /// </summary>
        /// <param name="position">The position that should be rotated</param>
        /// <param name="rotationOrigin">The rotation origin</param>
        /// <param name="rotation">The rotation amount</param>
        /// <returns>The newly rotated position</returns>
        public static Vector3 RotatePositionAroundOrigin(Vector3 position, Vector3 rotationOrigin, Quaternion rotation)
        {
            position = position - rotationOrigin;

            Vector3 rotatedPosition = rotation * position;

            return rotatedPosition;
        }

        #endregion

        // ------------------------------------------------------------- //
        // Normal                                                        //
        // ------------------------------------------------------------- //
        /// <summary>
        /// Will calculate the normal for an set of 3d angles (in deg)
        /// </summary>
        /// <param name="x">Degrees in the x direction (0-360)</param>
        /// <param name="y">Degrees in the y direction (0-360)</param>
        /// <param name="z">Degrees in the z direction (0-360)</param>
        /// <returns>The normalized version of the angle</returns>
        public static Vector3 CalculateNormalizedNormalFromEulerAngles(float x, float y, float z)
        {
            // Convert Euler angles to Quaternion
            Quaternion rotation = Quaternion.Euler(new Vector3(x, y, z));

            // Transform a forward vector by the rotation to get the normal
            Vector3 normal = rotation * Vector3.forward;

            // Normalize the normal vector
            normal.Normalize();

            return normal;
        }

        /// <summary>
        /// Will calculate the normal for an set of 3d angles (in deg)
        /// </summary>
        /// <param name="eulerAngles">Degrees (0-360)</param>
        /// <returns>The normalized version of the angle</returns>
        public static Vector3 CalculateNormalizedNormalFromEulerAngles(Vector3 eulerAngles)
        {
            // Convert Euler angles to Quaternion
            Quaternion rotation = Quaternion.Euler(eulerAngles);

            // Transform a forward vector by the rotation to get the normal
            Vector3 normal = rotation * Vector3.forward;

            // Normalize the normal vector
            normal.Normalize();

            return normal;
        }

        public static Vector3 CalculateEulerAnglesFromNormalizedNormal(Vector3 normal)
        {
            // Align the normal with the world up vector (Y-axis)
            Vector3 upVector = Vector3.up;
            Quaternion alignNormal = Quaternion.FromToRotation(upVector, normal);

            // Create a rotation that aligns the world forward direction with the modified normal
            Quaternion rotation = alignNormal * Quaternion.LookRotation(Vector3.forward);

            // Convert the rotation to Euler angles
            Vector3 eulerAngles = rotation.eulerAngles;

            return eulerAngles;
        }
    }
}
