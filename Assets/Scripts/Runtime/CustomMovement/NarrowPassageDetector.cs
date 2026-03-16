using UnityEngine;
using DiasGames.AbilitySystem.Traversal;

namespace DiasGames.AbilitySystem.Components
{
    public class NarrowPassageDetector : MonoBehaviour
    {
        public NarrowPassage CurrentPassage { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            NarrowPassage passage = other.GetComponent<NarrowPassage>();
            if (passage != null)
            {
                CurrentPassage = passage;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            NarrowPassage passage = other.GetComponent<NarrowPassage>();
            if (passage != null && CurrentPassage == passage)
            {
                CurrentPassage = null;
            }
        }
    }
}