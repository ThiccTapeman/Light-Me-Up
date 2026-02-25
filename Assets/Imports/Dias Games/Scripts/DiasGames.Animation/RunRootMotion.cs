using UnityEngine;

namespace DiasGames.Animation
{
    public class RunRootMotion : StateMachineBehaviour
    {
        [SerializeField] private Vector3 _rootMotionMultiplier = Vector3.one;
        [SerializeField] private bool _useRotation = false;

        private IRootMotion _rootMotion;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = true;
            if (_rootMotion == null)
            {
                _rootMotion = animator.GetComponent<IRootMotion>();
            }

            _rootMotion.ApplyRootMotion(_rootMotionMultiplier, _useRotation);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);

            if(HasRootMotion(animator, currentState, layerIndex))
            {
                return;
            }

            animator.applyRootMotion = false;
            _rootMotion.StopRootMotion();
        }

        private bool HasRootMotion(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            StateMachineBehaviour[] behaviours = animator.GetBehaviours(stateInfo.fullPathHash, layerIndex);
            foreach (var behaviour in behaviours)
            {
                if (behaviour.GetType() == typeof(RunRootMotion))
                {
                    return true;
                }
            }

            return false;
        }
    }
}