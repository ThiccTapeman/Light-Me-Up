using UnityEngine;

namespace DiasGames
{
    public interface IAnimationController
    {
        Animator Animator { get; }
        void SetAnimationState(string stateName, int layerIndex = 0, float transitionDuration = 0.1f);
        bool HasFinishedAnimation(string state, int layerIndex = 0);
    }
}