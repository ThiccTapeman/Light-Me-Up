using UnityEngine;

namespace DiasGames.Components
{
    internal class MovementController
    {
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _capsule;
        private readonly CharacterController _controller;

        public bool IsRigidbody => _rigidbody != null;

        public bool CollisionEnabled
        {
            get => _rigidbody ? _capsule.enabled : _controller.enabled;
            set
            {
                if (_rigidbody)
                {
                    _capsule.enabled = value;
                }
                else
                {
                    _controller.enabled = value;
                }
            }
        }

        public float Height
        {
            get => _rigidbody ? _capsule.height : _controller.height;
            set
            {
                if (_rigidbody)
                {
                    _capsule.height = value;
                }
                else
                {
                    _controller.height = value;
                }
            }
        }

        public float Radius
        {
            get => _rigidbody ? _capsule.radius : _controller.radius;
            set
            {
                if (_rigidbody)
                {
                    _capsule.radius = value;
                }
                else
                {
                    _controller.radius = value;
                }
            }
        }

        public Vector3 Center
        {
            get => _rigidbody ? _capsule.center : _controller.center;
            set
            {
                if (_rigidbody)
                {
                    _capsule.center = value;
                }
                else
                {
                    _controller.center = value;
                }
            }
        }
        
        public float StepUp
        {
            get => _rigidbody ? 0.0f : _controller.stepOffset;
            set
            {
                if (!_rigidbody)
                {
                    _controller.stepOffset = value;
                }
            }
        }

        public bool IsGrounded => !_rigidbody && _controller.isGrounded;
        public Vector3 Velocity => _rigidbody ? _rigidbody.linearVelocity : _controller.velocity;

        public MovementController(GameObject owner)
        {
            _rigidbody = owner.GetComponent<Rigidbody>();
            _capsule = owner.GetComponent<CapsuleCollider>();
            _controller = owner.GetComponent<CharacterController>();
        }

        public void Move(Vector3 delta)
        {
            if (_rigidbody)
            {
                _rigidbody.linearVelocity = delta / Time.deltaTime;
            }
            else
            {
                if (CollisionEnabled)
                {
                    _controller.Move(delta);
                }
                else
                {
                    _controller.gameObject.transform.position += delta;
                }
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (_rigidbody)
            {
                _rigidbody.position = position;
            }
            else
            {
                bool currentEnable = _controller.enabled;

                _controller.enabled = false;
                _controller.transform.position = position;
                _controller.enabled = currentEnable;
            }
        }
    }
}