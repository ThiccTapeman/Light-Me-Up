using UnityEngine;

namespace DiasGames.ClimbingSystem
{
    public class JumpData
    {
        public float dot;
        public Transform target;
        private float yPos;

        public JumpData(float dot, Transform target)
        {
            this.dot = dot;
            this.target = target;

            yPos = target.position.y;
        }

        public float GetSortFactor()
        {
            if(yPos < 0)
            {
                return yPos - Mathf.Abs(yPos * (1-dot));
            }

            return yPos * dot;
        }
    }
}