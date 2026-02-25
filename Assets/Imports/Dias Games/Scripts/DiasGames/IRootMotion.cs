using UnityEngine;

namespace DiasGames
{
    public interface IRootMotion
    {
        void ApplyRootMotion(Vector3 multiplier, bool useRotation = false);
        void StopRootMotion();
    }
}