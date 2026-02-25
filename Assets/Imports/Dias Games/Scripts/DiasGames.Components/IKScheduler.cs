using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Components
{
    public class IKScheduler : MonoBehaviour
    {
        private Animator _animator = null;
        private readonly Dictionary<AvatarIKGoal, IKParameters> _activeIKs = new(4);

        private readonly Queue<AvatarIKGoal> _activeIkToRemove = new(4);

        [SerializeField] private float IKSmoothTime = 0.12f;
        [SerializeField] private bool _applyIK = true;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            foreach (KeyValuePair<AvatarIKGoal, IKParameters> valuePair in _activeIKs)
            {
                valuePair.Value.UpdateWeight(IKSmoothTime, _animator);
                if (Mathf.Approximately(valuePair.Value.Weight, 0.0f)
                    && valuePair.Value.TargetWeight == 0.0f)
                {
                    _activeIkToRemove.Enqueue(valuePair.Key);
                }
            }

            while (_activeIkToRemove.Count > 0)
            {
                _activeIKs.Remove(_activeIkToRemove.Dequeue());
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_activeIKs.Count == 0 || !_applyIK) return;

            foreach (KeyValuePair<AvatarIKGoal,IKParameters> valuePair in _activeIKs)
            {
                AvatarIKGoal goal = valuePair.Key;
                IKParameters currentIK = valuePair.Value;
                if (currentIK.Weight < 0.01f) continue;

                _animator.SetIKPositionWeight(goal, currentIK.Weight * currentIK.PositionWeight);
                _animator.SetIKRotationWeight(goal, currentIK.Weight * currentIK.RotationWeight);

                _animator.SetIKPosition(goal, currentIK.Effector.position);
                _animator.SetIKRotation(goal, currentIK.Effector.rotation);
            }
        }

        /// <summary>
        /// Ask system to apply IK
        /// </summary>
        public void ApplyIK(AvatarIKGoal goal, Transform effector, 
            float positionWeight = 1.0f, 
            float rotationWeight = 1.0f,
            string animatorParameter = "")
        {
            IKParameters ikParameters = new IKParameters(effector, positionWeight, rotationWeight, animatorParameter);
            _activeIKs[goal] = ikParameters;
        }

        /// <summary>
        /// Tells system to stop IK
        /// </summary>
        public void StopIK(AvatarIKGoal goal)
        {
            if (_activeIKs.TryGetValue(goal, out IKParameters ikParameters))
            {
                ikParameters.TargetWeight = 0.0f;
            }
        }
    }

    public class IKParameters
    {
        public Transform Effector { get; private set; }
        public float Weight { get; private set; }
        public float TargetWeight{ get; set; }
        public float PositionWeight { get; private set; }
        public float RotationWeight { get; private set; }

        private readonly string _animatorParam;

        private float _vel;
        private float _step;

        public IKParameters(Transform effector, float positionWeight, float rotationWeight, string animatorParam)
        {
            Effector = effector;
            PositionWeight = positionWeight;
            RotationWeight = rotationWeight;
            _animatorParam = animatorParam;

            Weight = 0;
            TargetWeight = 1.0f;
        }

        public void UpdateWeight(float smoothTime, Animator animator)
        {
            _step = 1.0f / smoothTime;
            float animatorWeight = string.IsNullOrEmpty(_animatorParam) ? 1.0f : animator.GetFloat(_animatorParam);
            float target = TargetWeight * animatorWeight;
            if (!Mathf.Approximately(Weight, target))
            {
                Weight = Mathf.MoveTowards(Weight, TargetWeight * animatorWeight, _step * Time.deltaTime);
            }
        }
    }
}