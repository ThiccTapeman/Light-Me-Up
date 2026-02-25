using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Components
{
    public class MovementComponent : MonoBehaviour, IRootMotion, IMovement
    {
        public event System.Action OnLanded;
        public event System.Action OnStartFalling;

        private const float MinSqrtThreshold = 0.05f;
        
        [Header("Player")] [SerializeField] private float _defaultMaxMoveSpeed = 4.5f;
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [SerializeField] private float _rotationSmoothTime = 0.12f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float _speedChangeRate = 10.0f;
        [Tooltip("Whether to use or not camera forward vector to calculate movement direction relative to camera")]
        [SerializeField] private bool _useCameraOrientation = true;

        [Header("Player Grounded")]
        [Tooltip("Useful for rough ground")]
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private float _groundedCastDistance = 0.07f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float _groundedRadius = 0.28f;
        [Range(0, 90)][SerializeField] private float _slopeAngleLimit = 40.0f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float _gravity = -15.0f;
        [SerializeField] private bool _orientRotation;
        [SerializeField] [UnityTag] private string _movableGroundTag = "Untagged";
        
        private float DeltaTime => _controller.IsRigidbody ? Time.fixedDeltaTime : Time.deltaTime;

        private readonly float _terminalVelocity = 53.0f;
        private GameObject _movableGroundAux;

        private float _currentMaxMoveSpeed;
        private bool _grounded = true;
        private Collider _currentGround;
        private float _speed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _initialCapsuleHeight = 2f;
        private float _initialCapsuleRadius = 0.28f;
        private Vector2 _currentInput;
        private Vector2 _lastNonZeroInput;
        private bool _isUsingGravity = true;

        // variables for root motion
        private bool _useRootMotion = false;
        private Vector3 _rootMotionMultiplier = Vector3.one;
        private bool _useRotationRootMotion = true;

        private Animator _animator;
        private MovementController _controller;
        private GameObject _mainCamera;

        // controls character velocity
        private Vector3 _velocity;

        public event System.Action OnCollisionEnabledChanged;
        public bool IsFalling => !_grounded;
        public float MaxSpeed => _currentMaxMoveSpeed;

        public float Gravity => _gravity;
        public bool GravityEnabled => _isUsingGravity;

        public float CurrentMoveSpeed => _speed;

        public Vector3 Velocity => _velocity;

        public Collider CurrentGround => _currentGround;
        public LayerMask GroundLayerMask => _groundLayers;

        public float CapsuleHeight => _controller.Height;
        public float CapsuleRadius => _controller.Radius;

        private void Awake()
        {
            _controller = new MovementController(gameObject);
            _mainCamera = Camera.main.gameObject;
            _animator = GetComponent<Animator>();

            _initialCapsuleHeight = _controller.Height;
            _initialCapsuleRadius = _controller.Radius;
            _currentMaxMoveSpeed = _defaultMaxMoveSpeed;
            _movableGroundAux = new GameObject("Movable Ground Aux");
            ResetMovableAux();

            OrientRotationToMovement(true);
        }

        private void ResetMovableAux()
        {
            _movableGroundAux.transform.SetParent(transform);
            _movableGroundAux.transform.localPosition = Vector3.zero;
            _movableGroundAux.transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (_controller.IsRigidbody)
            {
                return;
            }
            
            InternalUpdate();
        } 
        
        private void FixedUpdate()
        {
            if (_controller.IsRigidbody)
            {
                InternalUpdate();
            }
        }

        private void InternalUpdate()
        {
            UpdateTransformOnMovables();

            if (_isUsingGravity)
            {
                GravityControl();
            }
            GroundedCheck();

            if (_useRootMotion)
            {
                return;
            }

            _controller.Move(_velocity * DeltaTime);
        }

        private void UpdateTransformOnMovables()
        {
            if (_controller.CollisionEnabled &&
                _movableGroundAux.transform.parent != transform)
            {
                Vector3 planarVelocity = _velocity.GetSize2D();
                if (planarVelocity.sqrMagnitude < 0.05f)
                {
                    var charTransform = transform;

                    Vector3 position = _movableGroundAux.transform.position;
                    position.y = charTransform.position.y;

                    SetPosition(position);
                    SetRotation(_movableGroundAux.transform.rotation);
                }
                else
                {
                    _movableGroundAux.transform.position = transform.position;
                    _movableGroundAux.transform.rotation = transform.rotation;
                }
            }
        }

        private void OnAnimatorMove()
        {
            RunMoveRootMotion();
            RunRotationRootMotion();
        }

        private void RunMoveRootMotion()
        {
            if (!_useRootMotion) return;
            if(_rootMotionMultiplier == Vector3.zero) return;

            Vector3 delta = Vector3.Scale(_animator.deltaPosition, _rootMotionMultiplier);
            delta.y = _isUsingGravity ? _velocity.y * DeltaTime : delta.y;

            _controller.Move(delta);
        }

        private void RunRotationRootMotion()
        {
            if (_useRotationRootMotion)
            {
                transform.rotation *= _animator.deltaRotation;
            }
        }

        public void SetGravityEnabled(bool gravityEnabled)
        {
            _isUsingGravity = gravityEnabled;
        }

        public void RotateToMovementDirection(float rotationControl = 1f)
        {
            Vector3 inputDirection = GetWorldDirectionInput();

            if (inputDirection.sqrMagnitude > MinSqrtThreshold)
            {
                _targetRotation = GetRotationFromDirection(inputDirection).eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    _rotationSmoothTime * (1.0f / rotationControl));
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        public void RotateToViewDirection()
        {
            Vector3 direction = _mainCamera != null ? _mainCamera.transform.forward.GetNormal2D() :  Vector3.forward;
            _targetRotation = GetRotationFromDirection(direction).eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity,  _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x,
                transform.position.y + _groundedRadius + _groundedCastDistance, transform.position.z);
            RaycastHit[] hits = new RaycastHit[10];
            int size = Physics.SphereCastNonAlloc(spherePosition, _groundedRadius, Vector3.down, hits,
                _groundedCastDistance * 2.0f, _groundLayers, QueryTriggerInteraction.Ignore);

            // check angle
            bool hasValidAngle = HasValidAngle(size, hits);
            if (hasValidAngle && _velocity.y <= 0.5f)
            {
                _currentGround = hits[0].collider;
                if (!_grounded)
                {
                    _grounded = true;
                    Land();
                }

                if (_currentGround.CompareTag(_movableGroundTag))
                {
                    _movableGroundAux.transform.SetParent(_currentGround.transform, true);
                }
                else if(_movableGroundAux.transform.parent != transform)
                {
                    ResetMovableAux();
                }
            }
            else
            {
                if(_movableGroundAux.transform.parent != transform)
                {
                    ResetMovableAux();
                }

                if (_grounded)
                {
                    if (Physics.Raycast(spherePosition, Vector3.down, out RaycastHit lineHit,
                            _groundedRadius + _groundedCastDistance * 2.0f + _controller.StepUp,
                            _groundLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (HasValidAngle(1, new[] { lineHit }))
                        {
                            return;
                        }
                    }

                    _grounded = false;
                    StartFall();
                }

                _currentGround = null;
                Vector3 normal = size > 0 ? hits[0].normal : Vector3.zero;
                Depenetrate(normal);
            }
        }

        private bool HasValidAngle(int size, IReadOnlyList<RaycastHit> hits)
        {
            for (int i = 0; i < size; i++)
            {
                RaycastHit currentHit = hits[i];
                float angle = Vector3.Angle(Vector3.up, currentHit.normal);
                if (angle <= _slopeAngleLimit)
                {
                    return true;
                }
            }

            return false;
        }

        private void Depenetrate(Vector3 normal)
        {
            if (!_controller.CollisionEnabled || !_isUsingGravity || _controller.IsRigidbody) return;

            const float GroundedOffset = -0.14f;
            const float SkinWidth = 0.02f;

            if (_controller.IsGrounded && normal.sqrMagnitude > 0)
            {
                Vector3 direction = normal;
                direction.y = -1;
                _controller.Move(direction * (SkinWidth * 100 * DeltaTime));
                return;
            }
            
            // if not depenetrate char
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + Vector3.up, _controller.Radius, Vector3.down,
                    out hit, 1 - GroundedOffset, _groundLayers, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.5f)
                {
                    _grounded = false;
                    Vector3 direction = hit.normal;
                    direction.y = -1;
                    _controller.Move(direction.normalized * (SkinWidth * 100 * DeltaTime));
                }
            }
        }

        private void StartFall()
        {
            if (_velocity.y < 0.0f)
            {
                _velocity.y = 0.0f;
            }

            OnStartFalling?.Invoke();
        }

        private void Land()
        {
            OnLanded?.Invoke();
        }

        private void GravityControl()
        {
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_velocity.y < _terminalVelocity)
            {
                _velocity.y += _gravity * DeltaTime;
            }

            if (_grounded)
            {
                if (_controller.IsRigidbody)
                {
                    RigidbodyGroundedGravity();
                }
                else
                {
                    ControllerGroundedGravity();
                }
            }
        }

        private void ControllerGroundedGravity()
        {
            // stop our velocity dropping infinitely when grounded
            if (_velocity.y < 0.5f)
            {
                if (!_useRootMotion || _rootMotionMultiplier.y == 0)
                {
                    _velocity.y = -5f;
                }
            }
        }

        private void RigidbodyGroundedGravity()
        {
            // stop our velocity dropping infinitely when grounded
            if (_velocity.y < 0.0f)
            {
                if (!_useRootMotion || _rootMotionMultiplier.y == 0)
                {
                    Vector3 projected = Vector3.ProjectOnPlane(_velocity, GetGroundNormal());
                    _velocity.y = projected.y;
                }
            }
        }

        private Vector3 GetGroundNormal()
        {
            if (_currentGround == null)
            {
                return Vector3.up;
            }

            Vector3 startCast = new Vector3(transform.position.x,
                transform.position.y + _groundedCastDistance, transform.position.z);
            RaycastHit[] hits = new RaycastHit[10];
            int size = Physics.RaycastNonAlloc(startCast, Vector3.down, hits,
                _groundedCastDistance * 2.0f, _groundLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < size; i++)
            {
                if (hits[0].collider == _currentGround)
                {
                    return hits[0].normal;
                }
            }

            return Vector3.up;
        }

        // ****************************** //
        // Interface implementation
        // ****************************** //

        public void SetMaxMoveSpeed(float newMaxMoveSpeed)
        {
            _currentMaxMoveSpeed = newMaxMoveSpeed;
        }

        public void OrientRotationToMovement(bool orient)
        {
            _orientRotation = orient;
        }

        public void AddMoveInput(Vector2 moveInput)
        {
            _currentInput = moveInput;
        }

        public void MoveByInput()
        {
            float targetSpeed = _currentMaxMoveSpeed;

            if (_currentInput.sqrMagnitude < MinSqrtThreshold)
            {
                targetSpeed = 0.0f;
            }
            else
            {
                _lastNonZeroInput = _currentInput;
            }

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_velocity.x, 0.0f, _velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _currentInput.magnitude; // _input.analogMovement ? _input.move.magnitude : 1f;

            if (inputMagnitude > 1)
            {
                inputMagnitude = 1f;
            }

            float targetInputSpeed = targetSpeed * inputMagnitude;
            // accelerate or decelerate to target speed
            if ((currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset) 
                && !Mathf.Approximately(Mathf.Abs(_speed-targetInputSpeed), 0.0f))
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetInputSpeed,
                    DeltaTime * _speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetInputSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = transform.forward;
            if (_lastNonZeroInput.sqrMagnitude > MinSqrtThreshold)
            {
                inputDirection = new Vector3(_lastNonZeroInput.x, 0.0f, _lastNonZeroInput.y).normalized;
            }

            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                   (_useCameraOrientation && _mainCamera != null
                                       ? _mainCamera.transform.eulerAngles.y
                                       : 0);

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            _velocity = targetDirection.normalized * _speed + new Vector3(0.0f, _velocity.y, 0.0f);
        }

        public void Move(Vector3 velocity)
        {
            Vector3 newVel = velocity;
            if (_isUsingGravity)
            {
                newVel.y = _velocity.y;
            }

            _velocity = newVel;
            _speed = velocity.magnitude;
        }

        public void StopMovement()
        {
            _controller.Move(Vector3.zero);     // Forces controller to set internal velocity to Zero
            _velocity = Vector3.zero;
            _speed = 0;
        }
        
        public void Jump(float jumpPower)
        {
            _velocity.y = jumpPower;
            _speed = _velocity.GetNormal2D().magnitude;
            _grounded = false;
        }

        public void SetPosition(Vector3 position)
        {
            _controller.SetPosition(position);
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        /// <summary>
        /// Get rotation to face desired direction
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRotationFromDirection(Vector3 direction)
        {
            float yaw = Mathf.Atan2(direction.x, direction.z);
            return Quaternion.Euler(0, yaw * Mathf.Rad2Deg, 0);
        }

        public Vector3 GetWorldDirectionInput()
        {
            if (_currentInput.sqrMagnitude < MinSqrtThreshold) return Vector3.zero;

            Vector3 normalizedInput = _currentInput.normalized;
            float cameraYaw = !_useCameraOrientation || _mainCamera == null ? 0 : _mainCamera.transform.eulerAngles.y;
            float targetYaw = Mathf.Atan2(normalizedInput.x, normalizedInput.y) * Mathf.Rad2Deg + cameraYaw;

            return Quaternion.Euler(0, targetYaw, 0) * Vector3.forward;
        }

        public void EnableCollision()
        {
            _controller.CollisionEnabled = true;
            OnCollisionEnabledChanged?.Invoke();
        }

        public void DisableCollision()
        {
            _controller.CollisionEnabled = false;
            OnCollisionEnabledChanged?.Invoke();
        }

        public void ApplyRootMotion(Vector3 multiplier, bool useRotation = false)
        {
            _useRootMotion = multiplier.sqrMagnitude > 0.05f;
            _rootMotionMultiplier = multiplier;
            _useRotationRootMotion = useRotation;
        }

        public void StopRootMotion()
        {
            _useRootMotion = false;
            _useRotationRootMotion = false;
        }

        public void SetCapsuleSize(float newHeight, float newRadius)
        {
            if (newRadius > newHeight * 0.5f)
                newRadius = newHeight * 0.5f;

            _controller.Radius = newRadius;
            _controller.Height = newHeight;
            _controller.Center = new Vector3(0, newHeight * 0.5f, 0);
        }

        public void ResetCapsuleSize()
        {
            SetCapsuleSize(_initialCapsuleHeight, _initialCapsuleRadius);
        }
    }
}