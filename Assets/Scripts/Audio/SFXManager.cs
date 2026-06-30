using UnityEngine;

namespace PocketGarden.Audio
{
    /// <summary>
    /// Minimal sound-effects player. Loads clips from <c>Resources/Audio</c> and exposes simple
    /// Play methods. The merge sound is the same "card reveal" flip used in Magic Pairs
    /// (<c>Resources/Audio/Card_Flip</c>). Created as a singleton by the scene setup.
    /// </summary>
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

        private AudioSource _source;
        private AudioClip _merge;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _merge = Resources.Load<AudioClip>("Audio/Card_Flip");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Plays the merge / card-reveal sound.</summary>
        public void PlayMerge() => Play(_merge);

        private void Play(AudioClip clip)
        {
            if (clip != null && _source != null)
                _source.PlayOneShot(clip, volume);
        }
    }
}
