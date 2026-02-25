using UnityEngine;

namespace DiasGames.Components
{
    public class AnimationController : MonoBehaviour, IAnimationController
    {
        [SerializeField] private Animator _animator;

        public Animator Animator => _animator;
        
        [SerializeField] private string _speedParameterName = "Move Speed";
        [SerializeField] private string _verticalSpeedParameterName = "Vertical Speed";
        [SerializeField] private string _fallingParameterName = "Is Falling";
        [SerializeField] private string _horizontalParamName = "Horizontal";
        [SerializeField] private string _verticalParamName = "Vertical";

        private IMovement _movementComponent;
        private Transform _mainCamera;

        private int _speedParameterId;
        private int _verticalSpeedParameterId;
        private int _fallingParameterId;
        private int _horizontalParameterId;
        private int _verticalParameterId;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _movementComponent = GetComponent<IMovement>();
            _mainCamera = Camera.main.transform;

            AssignAnimationID();
        }

        private void AssignAnimationID()
        {
            _speedParameterId = Animator.StringToHash(_speedParameterName);
            _verticalSpeedParameterId = Animator.StringToHash(_verticalSpeedParameterName);
            _fallingParameterId = Animator.StringToHash(_fallingParameterName);
            _horizontalParameterId = Animator.StringToHash(_horizontalParamName);
            _verticalParameterId = Animator.StringToHash(_verticalParamName);
        }

        private void Update()
        {
            if (_movementComponent != null)
            {
                _animator.SetFloat(_speedParameterId, _movementComponent.CurrentMoveSpeed);
                _animator.SetFloat(_verticalSpeedParameterId, _movementComponent.Velocity.y);
                _animator.SetBool(_fallingParameterId, _movementComponent.IsFalling);
 
                _animator.SetFloat(_horizontalParameterId, 
                    Vector3.Dot(_movementComponent.GetWorldDirectionInput(), _mainCamera.right),
                    0.1f, Time.deltaTime);

                _animator.SetFloat(_verticalParameterId, 
                    Vector3.Dot(_movementComponent.GetWorldDirectionInput(), _mainCamera.forward.GetNormal2D()),
                    0.1f, Time.deltaTime);
            }
        }

        public void SetAnimationState(string stateName, int layerIndex = 0, float transitionDuration = 0.1f)
        {
            if (_animator.HasState(layerIndex, Animator.StringToHash(stateName)))
            {
                _animator.CrossFadeInFixedTime(stateName, transitionDuration, layerIndex);
            }
        }

        /// <summary>
        /// Check if a specific state has finished
        /// </summary>
        /// <param name="state"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public bool HasFinishedAnimation(string state, int layerIndex = 0)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            var nextStateInfo = _animator.GetNextAnimatorStateInfo(layerIndex);

            if (_animator.IsInTransition(layerIndex) && nextStateInfo.IsName(state)) return false;

            if (stateInfo.IsName(state))
            {
                float normalizeTime = Mathf.Repeat(stateInfo.normalizedTime, 1);
                if (normalizeTime >= 0.95f)
                {
                    return true;
                }

                if (nextStateInfo.fullPathHash != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public TBehaviour GetBehaviourOnState<TBehaviour>(string stateName, int layer = 0) where TBehaviour : StateMachineBehaviour
        {
            string layerName = _animator.GetLayerName(layer);
            int fullPathHash = Animator.StringToHash($"{layerName}.{stateName}");
            StateMachineBehaviour[] behaviours = _animator.GetBehaviours(fullPathHash, layer);
            foreach (var behaviour in behaviours)
            {
                if (behaviour.GetType() == typeof(TBehaviour))
                {
                    return (TBehaviour)behaviour;
                }
            }

            return null;
        }

        private void Reset()
        {
            if (_animator)
            {
                _animator = GetComponent<Animator>();
            }
        }
    }
}