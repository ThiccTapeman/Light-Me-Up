using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiasGames.ClimbingSystem
{
    public class Ladder : MonoBehaviour, ILedge
    {
        [SerializeField] private List<GameObject> _excludeFromGrabPoints = new List<GameObject>();
        [SerializeField] private GameObject _topLimit;
        [SerializeField] private GameObject _bottomLimit;
        [SerializeField] private bool canClimbUp = false;
        [Header("Debug")]
        [SerializeField] private Color grabPointColor = Color.magenta;
        [SerializeField] private Color arrowColor = Color.white;
        [SerializeField] private float grabPointRadius = 0.1f;
        [SerializeField] private float arrowSize = 0.35f;

        public GameObject TopLimit => _topLimit;
        public GameObject BottomLimit => _bottomLimit;
        public bool CanClimbUp => canClimbUp;
        public bool CanJumpSide => true;
        public List<Transform> GrabPoints => grabPoints;

        private List<Transform> grabPoints = new List<Transform>();

        private void Awake()
        {
            grabPoints = GetGrabPoints();
        }

        private List<Transform> GetGrabPoints()
        {
            List<Transform> grabList = new List<Transform>();

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject target = transform.GetChild(i).gameObject;
                if (target.activeSelf && !_excludeFromGrabPoints.Contains(target))
                {
                    grabList.Add(transform.GetChild(i));
                }
            }

            return grabList;
        }

        public Transform GetClosestPoint(Vector3 origin)
        {
            if (grabPoints.Count == 0)
                return null;

            Transform closestGrab = grabPoints[0];
            float closestDistance = Vector3.Distance(closestGrab.position, origin);
            foreach (var grab in grabPoints)
            {
                float distance = Vector3.Distance(grab.position, origin);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGrab = grab;
                }
            }

            return closestGrab;
        }
        
        public Transform GetClosestPoint(Vector3 origin, Vector3 normal)
        {
            return GetClosestPoint(origin);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            grabPoints = GetGrabPoints();

            if (grabPoints.Count == 0) return;

            if (Vector3.Distance(Camera.current.transform.position, transform.position) > 15f)
                return;

            foreach (var grab in grabPoints)
            {
                Gizmos.color = grabPointColor;
                Gizmos.DrawSphere(grab.position, grabPointRadius);

                Handles.color = arrowColor;
                Handles.ArrowHandleCap(0, grab.position, Quaternion.LookRotation(grab.forward), arrowSize, EventType.Repaint);
            }

        }
#endif
    }
}