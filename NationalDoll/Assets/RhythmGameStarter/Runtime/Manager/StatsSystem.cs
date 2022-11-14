using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/stats-system")]
    public class StatsSystem : MonoBehaviour
    {
        [Comment("Responsible for advance stats' config and events.", order = 0)]
        [Title("Hit Level Config", false, 2, order = 1)]
        [Tooltip("Config the hit distance difference for each level, such as Perfect,Ok etc")]
        public HitLevelList levels;

        [Title("Events", 2)] [CollapsedEvent] public StringEvent onComboStatusUpdate;
        [CollapsedEvent] public StringEvent onScoreUpdate;
        [CollapsedEvent] public StringEvent onMaxComboUpdate;
        [CollapsedEvent] public StringEvent onMissedUpdate;

        #region RUNTIME_FIELD

        [NonSerialized] public int combo;
        [NonSerialized] public int maxCombo;
        [NonSerialized] public int missed;
        [NonSerialized] public int score;

        #endregion

        [Serializable]
        public class HitLevelList : ReorderableList<HitLevel>
        {
        }

        [Serializable]
        public class HitLevel
        {
            public string name;
            public float threshold;
            [HideInInspector] public int count;
            public float scorePrecentage = 1;
            public StringEvent onCountUpdate;
        }

        public void AddMissed(int addMissed)
        {
            missed += addMissed;
            onMissedUpdate.Invoke(missed.ToString());
        }

        void Start()
        {
            UpdateScoreDisplay();

            if (TryGetComponent<SongManager>(out var manager))
            {
                manager.onSongStartPlay.AddListener(ClearStats);
            }
        }

        public void ClearStats()
        {
            for (int i = 0; i < levels.values.Count; i++)
            {
                var x = levels.values[i];
                x.count = 0;
                x.onCountUpdate.Invoke(x.count.ToString());
            }

            score = 0;
            missed = 0;
            UpdateScoreDisplay();
        }

        public void AddCombo(int addCombo, float deltaDiff, int addScore)
        {
            // print(deltaDiff);
            combo += addCombo;
            if (combo > maxCombo)
            {
                maxCombo = combo;
                onMaxComboUpdate.Invoke(maxCombo.ToString());
            }

            for (int i = 0; i < levels.values.Count; i++)
            {
                var x = levels.values[i];
                if (deltaDiff <= x.threshold)
                {
                    x.count++;
                    score += (int) (addScore * x.scorePrecentage);
                    x.onCountUpdate.Invoke(x.count.ToString());
                    UpdateScoreDisplay();
                    onComboStatusUpdate.Invoke(x.name);
                    // print(x.name);
                    return;
                }
            }

            //When no level matched
            onComboStatusUpdate.Invoke("");
        }

        public void UpdateScoreDisplay()
        {
            onScoreUpdate.Invoke(score.ToString());
        }
    }
}