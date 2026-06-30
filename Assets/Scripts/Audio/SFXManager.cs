using UnityEngine;
using PocketGarden.Quests;

namespace PocketGarden.Audio
{
    /// <summary>
    /// Minimal sound-effects player. Loads clips from <c>Resources/Audio</c> and exposes simple
    /// Play methods. The merge sound is the same "card reveal" flip used in Magic Pairs
    /// (<c>Resources/Audio/Card_Flip</c>); the level-up jingle is <c>Level_Complete</c>.
    /// Created as a singleton by the scene setup.
    /// </summary>
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

        private AudioSource _source;
        private AudioClip _merge;
        private AudioClip _levelUp;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _merge = Resources.Load<AudioClip>("Audio/Card_Flip");
            _levelUp = Resources.Load<AudioClip>("Audio/Level_Complete");
        }

        private void OnEnable()
        {
            QuestManager.OnQuestComplete += OnQuestComplete;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestComplete -= OnQuestComplete;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnQuestComplete(Quest q) => PlayLevelUp();

        /// <summary>Plays the merge / card-reveal sound.</summary>
        public void PlayMerge() => Play(_merge);

        /// <summary>Plays the level-up / quest-complete jingle (from Magic Pairs).</summary>
        public void PlayLevelUp() => Play(_levelUp);

        private void Play(AudioClip clip)
        {
            if (clip != null && _source != null)
                _source.PlayOneShot(clip, volume);
        }
    }
}
