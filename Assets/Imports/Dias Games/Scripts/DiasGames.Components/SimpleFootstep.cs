using UnityEngine;

namespace DiasGames.Components
{
    public class SimpleFootstep : MonoBehaviour
    {
        [SerializeField] private AudioSource _footstepAudioSource;
        [SerializeField] private AudioClipContainer _footstepClips;

        public void Footstep(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5f)
            {
                _footstepAudioSource.clip = _footstepClips.GetRandomClip();
                _footstepAudioSource.volume = _footstepClips.GetVolume();
                _footstepAudioSource.pitch = _footstepClips.GetPitch();
                _footstepAudioSource.Play();
            }
        }
    }
}