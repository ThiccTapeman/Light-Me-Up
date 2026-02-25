using UnityEngine;

namespace DiasGames.Components
{
    public class CharacterAudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource effectsSource;

        public void PlayVoice(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, float delay = 0.0f)
        {
            PlaySource(voiceSource, clip, volume, pitch,delay);
        }

        public void PlayEffect(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, float delay = 0.0f)
        {
            PlaySource(effectsSource, clip, volume, pitch, delay);
        }

        public void PlayVoice(AudioClipContainer clipContainer)
        {
            PlayVoice(clipContainer.GetRandomClip(), clipContainer.GetVolume(), clipContainer.GetPitch(), clipContainer.GetDelay());
        }

        public void PlayEffect(AudioClipContainer clipContainer)
        {
            PlayEffect(clipContainer.GetRandomClip(), clipContainer.GetVolume(), clipContainer.GetPitch(), clipContainer.GetDelay());
        }

        private void PlaySource(AudioSource source, AudioClip clip, float volume, float pitch, float delay)
        {
            if (source == null) return;

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;

            if (Mathf.Approximately(delay, 0.0f))
            {
                source.Play();
            }
            else
            {
                source.PlayDelayed(delay);
            }
        }
    }
}