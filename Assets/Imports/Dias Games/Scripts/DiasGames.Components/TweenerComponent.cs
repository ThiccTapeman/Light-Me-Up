using UnityEngine;

namespace DiasGames.Components
{
    public class TweenerComponent : MonoBehaviour
    {
        [SerializeField] private UpdateMode _updateMode = UpdateMode.Update;
        [SerializeField] private AnimationCurve _defaultCurve = AnimationCurve.Linear(0,0,1,1);
        
        private Transform _target;
        private Transform _start;
        private Vector3 _bezierPoint;
        private bool _bezierTween;
        private bool _inTween;
        private float _weight;
        private float _duration;
        private float _tweenStep;
        private AnimationCurve _curve;
        private bool _useRotationOnly = false;
        private IMovement _movement;

        public bool IsRunningTween => _inTween;

        public float DeltaTime => _updateMode == UpdateMode.Update ? Time.deltaTime : Time.fixedDeltaTime;

        private void Awake()
        {
            _target = new GameObject("Tweener Target (Don't destroy it)").transform;
            _start = new GameObject("Tweener Start (Don't destroy it)").transform;
            _movement = GetComponent<IMovement>();
        }
        
        private void Update()
        {
            if (_updateMode == UpdateMode.Update)
            {
                TweenUpdate();
            }
        }
        
        private void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                TweenUpdate();
            }
        }

        private void TweenUpdate()
        {
            if (!_inTween)
            {
                return;
            }

            _weight = Mathf.MoveTowards(_weight, 1.0f, _tweenStep * DeltaTime);

            if (!_useRotationOnly)
            {
                if (_bezierTween)
                {
                    _movement.SetPosition(BezierLerp(_start.position, _target.position, _bezierPoint,
                        _curve.Evaluate(_weight)));
                }
                else
                {
                    _movement.SetPosition(Vector3.Lerp(_start.position, _target.position, _curve.Evaluate(_weight)));
                }
            }

            _movement.SetRotation(Quaternion.Lerp(_start.rotation, _target.rotation, _curve.Evaluate(_weight)));

            if (Mathf.Approximately(_weight, 1.0f))
            {
                _inTween = false;
                if (!_useRotationOnly)
                {
                    _movement.SetPosition(_target.position);
                }

                _movement.SetRotation(_target.rotation);
            }
        }

        public void StopTween()
        {
            _inTween = false;
        }
        
        public void DoBestLerp(Vector3 targetPosition, Quaternion targetRotation, float duration, Transform parent = null, AnimationCurve curve = null)
        {
            float rotDot = Quaternion.Dot(transform.rotation, targetRotation);
            if (rotDot > 0.75f)
            {
                DoLerp(targetPosition, targetRotation, duration, parent, curve);
            }
            else
            {
                DoBezier(targetPosition, targetRotation, duration, parent, curve);
            }
        }

        public void DoLerp(Vector3 targetPosition, Quaternion targetRotation, float duration, Transform parent = null, AnimationCurve curve = null)
        {
            Transform charTransform = transform;

            _start.position = charTransform.position;
            _start.rotation = charTransform.rotation;

            _target.position = targetPosition;
            _target.rotation = targetRotation;
            _target.parent = parent;

            _weight = 0;
            _duration = duration;
            _inTween = true;
            _tweenStep = 1f / duration;
            _curve = curve ?? _defaultCurve;
            _bezierTween = false;
            _useRotationOnly = false;
        }

        public void DoBezier(Vector3 targetPosition, Quaternion targetRotation, float duration, Transform parent = null, AnimationCurve curve = null)
        {
            DoLerp(targetPosition, targetRotation, duration, parent, curve);
            
            // calculate bezier point
            Quaternion midRot = Quaternion.Lerp(_start.rotation, _target.rotation, 0.5f);
            Vector3 forward = midRot * Vector3.forward;
            _bezierPoint = Vector3.Lerp(_start.position, _target.position, 0.5f) - forward;
            _bezierTween = true;
        }
        
        public void DoBezier(Vector3 targetPosition, Quaternion targetRotation, Vector3 midPoint,
            float duration, Transform parent = null, AnimationCurve curve = null)
        {
            DoLerp(targetPosition, targetRotation, duration, parent, curve);

            _bezierPoint = midPoint;
            _bezierTween = true;
        }
        
        private Vector3 BezierLerp(Vector3 start, Vector3 end, Vector3 bezier, float t)
        {
            Vector3 point = Mathf.Pow(1 - t, 2) * start;
            point += 2 * (1 - t) * t * bezier;
            point += t * t * end;

            return point;
        }

        public void DoRotationLerp(Quaternion targetRotation, float duration)
        {
            DoLerp(transform.position, targetRotation, duration);
            _useRotationOnly = true;
        }

        private void OnDestroy()
        {
            if (_target != null)
            {
                Destroy(_target);
            }

            if (_start != null)
            {
                Destroy(_start);
            }
        }
    }
}