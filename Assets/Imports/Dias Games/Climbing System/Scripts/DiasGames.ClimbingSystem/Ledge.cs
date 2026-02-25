using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiasGames.ClimbingSystem
{
    public class Ledge : MonoBehaviour, ILedge
    {
        private List<Transform> grabPoints = new List<Transform>();

        [SerializeField] private List<GameObject> _excludeFromGrabPoints = new List<GameObject>();
        [SerializeField] private bool canClimbUp = false;
        [SerializeField] private bool canJumpSide = false;
        [SerializeField] private bool invisible = false;
        [SerializeField] private bool keepRotation = false;
        [Header("Debug")]
        [SerializeField] private Color grabPointColor = Color.magenta;
        [SerializeField] private Color arrowColor = Color.white;
        [SerializeField] private float grabPointRadius = 0.1f;
        [SerializeField] private float arrowSize = 0.35f;

        public bool CanClimbUp => canClimbUp;
        public bool CanJumpSide => canJumpSide;
        public List<Transform> GrabPoints => grabPoints;

        private Quaternion _initialRot = Quaternion.identity;

        private void Awake()
        {
            if (invisible)
            {
                var meshes = GetComponentsInChildren<MeshRenderer>();
                foreach (var mesh in meshes)
                    mesh.enabled = false;
            }
            
            grabPoints = GetGrabPoints();
            _initialRot = transform.rotation;
        }
        
        private void Update()
        {
            if (!keepRotation) return;

            transform.rotation = _initialRot;
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

        public Transform GetClosestPoint(Vector3 origin, Vector3 normal)
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

            if (closestGrab != null && !closestGrab.CompareTag("LedgeLimit"))
            {
                if (normal != Vector3.zero &&
                    closestDistance > 0.25f &&
                    Vector3.Dot(closestGrab.forward, normal) > 0.7f)
                    return null;
            }

            return closestGrab;
        }

        public Transform GetClosestPoint(Vector3 origin)
        {
            return GetClosestPoint(origin, Vector3.zero);
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