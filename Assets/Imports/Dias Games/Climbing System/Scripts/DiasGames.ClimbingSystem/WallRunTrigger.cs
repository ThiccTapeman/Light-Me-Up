using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiasGames.ClimbingSystem
{
    public class WallRunTrigger : MonoBehaviour
    {
        [SerializeField] private Transform wallContact;
        [SerializeField] private Transform wallMovementDirection;
        [SerializeField] private bool isRightMove;

        public Transform WallContact { get { return wallContact; } }
        public Vector3 WallMoveDirection { get { return wallMovementDirection.forward; } }
        public bool IsRightMove { get { return isRightMove; } }


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (wallMovementDirection)
            {
                Handles.color = Color.cyan;
                Handles.ArrowHandleCap(0, wallMovementDirection.position, Quaternion.LookRotation(wallMovementDirection.forward), 0.5f, EventType.Repaint);
            }

            if (wallContact)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(wallContact.position, 0.05f);
            }
#endif
        }
    }
}