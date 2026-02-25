using UnityEngine;

namespace DiasGames.ClimbingSystem
{
    [System.Serializable]
    public class ClimbJumpAnimation
    {
        [SerializeField] private string _animationStateName;
        [SerializeField] private float _jumpDuration = 0.4f;
        [SerializeField] private float _animDuration = 0.7f;
        [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0,0,1,1);

        public string AnimationStateName => _animationStateName;
        public float JumpDuration => _jumpDuration;
        public float AnimDuration => _animDuration;
        public AnimationCurve Curve => _jumpCurve;
    }
}