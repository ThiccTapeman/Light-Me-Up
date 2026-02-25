using UnityEngine;

namespace DiasGames
{
    public static class Extensions
    {
        public static Vector3 GetNormal2D(this Vector3 vector)
        {
            Vector3 direction = vector;
            direction.y = 0;
            return direction.normalized;
        }

        public static Vector3 GetSize2D(this Vector3 vector)
        {
            Vector3 size = vector;
            size.y = 0;
            return size;
        }
    }
}