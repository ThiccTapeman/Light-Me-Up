using UnityEngine;

namespace DiasGames
{
    public interface IAudioPlayer
    {
        public void PlayVoice(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, float delay = 0.0f);
        public void PlayEffect(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, float delay = 0.0f);
        public void PlayVoice(AudioClipContainer clipContainer);
        public void PlayEffect(AudioClipContainer clipContainer);
    }
}