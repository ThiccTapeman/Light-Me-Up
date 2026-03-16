using UnityEngine;

namespace DiasGames.AbilitySystem.Traversal
{
    [RequireComponent(typeof(Collider))]
    public class NarrowPassage : MonoBehaviour
    {
        private static readonly Color CenterPathColor = new Color(0.2f, 0.9f, 1f, 1f);
        private static readonly Color WallPathColor = new Color(1f, 0.65f, 0.2f, 1f);
        private static readonly Color EntryColor = new Color(0.25f, 1f, 0.25f, 1f);
        private static readonly Color ExitColor = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color WallNormalColor = new Color(1f, 1f, 0.2f, 1f);

        public enum WallSide
        {
            Left = -1,
            Right = 1
        }

        [Header("Traversal Points")]
        [SerializeField] private Transform _entryPoint;
        [SerializeField] private Transform _exitPoint;

        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 1.25f;
        [SerializeField] private float _capsuleHeight = 1.4f;
        [SerializeField] private bool _lockToPassageForward = true;
        [SerializeField] private WallSide _wallSide = WallSide.Right;
        [SerializeField] private float _wallOffset = 0.35f;

        public Transform EntryPoint => _entryPoint;
        public Transform ExitPoint => _exitPoint;
        public float MoveSpeed => _moveSpeed;
        public float CapsuleHeight => _capsuleHeight;
        public bool LockToPassageForward => _lockToPassageForward;
        public float WallOffset => _wallOffset;
        public WallSide Side => _wallSide;
        public float PathLength
        {
            get
            {
                if (_entryPoint == null || _exitPoint == null)
                {
                    return 0f;
                }

                Vector3 a = _entryPoint.position;
                Vector3 b = _exitPoint.position;
                a.y = 0f;
                b.y = 0f;
                return Vector3.Distance(a, b);
            }
        }

        public Vector3 Forward
        {
            get
            {
                if (_entryPoint != null && _exitPoint != null)
                {
                    Vector3 dir = (_exitPoint.position - _entryPoint.position);
                    dir = Vector3.ProjectOnPlane(dir, Vector3.up);
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        return dir.normalized;
                    }
                }

                Vector3 forward = transform.forward;
                forward.y = 0f;
                return forward.normalized;
            }
        }

        public Vector3 WallNormal
        {
            get
            {
                Vector3 normal = Vector3.Cross(Vector3.up, Forward).normalized;
                return normal * (int)_wallSide;
            }
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        public Vector3 GetClosestPointOnPath(Vector3 worldPosition)
        {
            Vector3 a = EntryPoint.position;
            Vector3 b = ExitPoint.position;

            // ignore Y
            a.y = worldPosition.y;
            b.y = worldPosition.y;

            Vector3 ab = b - a;
            float abLengthSqr = ab.sqrMagnitude;

            if (abLengthSqr <= 0.0001f)
            {
                return a;
            }

            float t = Vector3.Dot(worldPosition - a, ab) / abLengthSqr;
            t = Mathf.Clamp01(t);

            Vector3 point = Vector3.Lerp(a, b, t);
            point.y = worldPosition.y; // preserve player height

            return point;
        }

        public Vector3 GetClosestPointOnWallPath(Vector3 worldPosition)
        {
            Vector3 point = GetClosestPointOnPath(worldPosition);
            Vector3 wallPoint = point + WallNormal * _wallOffset;
            wallPoint.y = worldPosition.y;
            return wallPoint;
        }

        public Vector3 GetPointOnPath(float normalizedProgress, float y)
        {
            Vector3 a = EntryPoint.position;
            Vector3 b = ExitPoint.position;
            a.y = y;
            b.y = y;
            return Vector3.Lerp(a, b, Mathf.Clamp01(normalizedProgress));
        }

        public Vector3 GetPointOnWallPath(float normalizedProgress, float y)
        {
            Vector3 center = GetPointOnPath(normalizedProgress, y);
            Vector3 wallPoint = center + WallNormal * _wallOffset;
            wallPoint.y = y;
            return wallPoint;
        }

        public float GetNormalizedProgress(Vector3 worldPosition)
        {
            Vector3 a = EntryPoint.position;
            Vector3 b = ExitPoint.position;

            // ignore Y
            a.y = worldPosition.y;
            b.y = worldPosition.y;

            Vector3 ab = b - a;
            float abLengthSqr = ab.sqrMagnitude;

            if (abLengthSqr <= 0.0001f)
            {
                return 0f;
            }

            float t = Vector3.Dot(worldPosition - a, ab) / abLengthSqr;
            return Mathf.Clamp01(t);
        }

        private void OnDrawGizmosSelected()
        {
            if (_entryPoint == null || _exitPoint == null)
            {
                return;
            }

            Vector3 a = _entryPoint.position;
            Vector3 b = _exitPoint.position;

            Vector3 wallOffset = WallNormal * _wallOffset;
            Vector3 wallA = a + wallOffset;
            Vector3 wallB = b + wallOffset;

            Gizmos.color = CenterPathColor;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere(a, 0.06f);
            Gizmos.DrawSphere(b, 0.06f);

            Gizmos.color = WallPathColor;
            Gizmos.DrawLine(wallA, wallB);
            Gizmos.DrawSphere(wallA, 0.05f);
            Gizmos.DrawSphere(wallB, 0.05f);

            Gizmos.color = EntryColor;
            Gizmos.DrawWireCube(a + Vector3.up * 0.06f, new Vector3(0.12f, 0.12f, 0.12f));

            Gizmos.color = ExitColor;
            Gizmos.DrawWireCube(b + Vector3.up * 0.06f, new Vector3(0.12f, 0.12f, 0.12f));

            Vector3 mid = Vector3.Lerp(a, b, 0.5f);
            Gizmos.color = WallNormalColor;
            Gizmos.DrawLine(mid, mid + wallOffset);
            Gizmos.DrawWireSphere(mid + wallOffset, 0.08f);
        }
    }
}
