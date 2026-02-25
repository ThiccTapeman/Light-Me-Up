using UnityEngine;

namespace DiasGames
{
    [CreateAssetMenu(fileName = "AudioClipContainer", menuName = "Dias Games/AudioClipContainer")]
    public class AudioClipContainer : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [Header("Volume")]
        [SerializeField] private float _minVolume = 0.75f;
        [SerializeField] private float _maxVolume = 1.0f;
        [Header("Pitch")]
        [SerializeField] private float _minPitch = 0.85f;
        [SerializeField] private float _maxPitch = 1.15f;
        [Header("Delay")]
        [SerializeField] private float _delay = 0f;

        public AudioClip GetRandomClip()
        {
            if (_clips.Length <= 0)
            {
                return null;
            }

            int random = Random.Range(0, _clips.Length);
            return _clips[random];
        }

        public AudioClip[] GetAllClips()
        {
            return _clips;
        }

        public AudioClip GetClipAtIndex(int index)
        {
            if (_clips.Length > index)
            {
                return _clips[index];
            }

            return null;
        }

        public float GetVolume()
        {
            return Random.Range(_minVolume, _maxVolume);
        }

        public float GetPitch()
        {
            return Random.Range(_minPitch, _maxPitch);
        }

        public float GetDelay()
        {
            return _delay;
        }
    }
}