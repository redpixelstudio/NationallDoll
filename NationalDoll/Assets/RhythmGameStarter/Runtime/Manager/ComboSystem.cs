using UnityEngine;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/stats-system")]
    public class ComboSystem : MonoBehaviour
    {
        private bool isShowing;

        [Comment("Handling simple combo events.")]
        // [Header("[Events]")]
        [CollapsedEvent]
        public StringEvent onComboUpdate;
        [CollapsedEvent]
        public BoolEvent onVisibilityChange;

        private StatsSystem statsSystem;

        private void Awake() {
            statsSystem = GetComponent<StatsSystem>();
        }

        private void Start()
        {
            UpdateComboDisplay();

            if (TryGetComponent<SongManager>(out var manager))
            {
                manager.onSongStartPlay.AddListener(ClearStats);
            }
        }

        public void ClearStats()
        {
            statsSystem.combo = 0;
            statsSystem.maxCombo = 0;
            UpdateComboDisplay();
        }

        public void AddCombo(int addCombo, float deltaDiff, int score)
        {
            statsSystem.AddCombo(addCombo, deltaDiff, score);

            if (!isShowing)
            {
                isShowing = true;
                onVisibilityChange.Invoke(isShowing);
            }

            UpdateComboDisplay();
        }

        public void BreakCombo()
        {
            statsSystem.AddMissed(1);
            statsSystem.combo = 0;

            isShowing = false;
            onVisibilityChange.Invoke(isShowing);
        }

        public void UpdateComboDisplay()
        {
            onComboUpdate.Invoke(statsSystem.combo.ToString());
        }
    }
}