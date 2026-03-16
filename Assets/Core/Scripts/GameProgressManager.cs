using UnityEngine;
using InvestigationGame.Data;

namespace InvestigationGame.Core
{
    public class GameProgressManager : MonoBehaviour
    {
        private static GameProgressManager _instance;
        public static GameProgressManager Instance 
        { 
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameProgressManager");
                    _instance = go.AddComponent<GameProgressManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string UnlockedLevelKey = "HighestUnlockedLevel";

        public int HighestUnlockedLevel { get; private set; }
        public int CurrentLevelToPlay { get; set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        private void LoadProgress()
        {
            HighestUnlockedLevel = PlayerPrefs.GetInt(UnlockedLevelKey, 0); // 0-indexed
        }

        public void UnlockNextLevel(int completedLevelIndex)
        {
            if (completedLevelIndex >= HighestUnlockedLevel)
            {
                HighestUnlockedLevel = completedLevelIndex + 1;
                PlayerPrefs.SetInt(UnlockedLevelKey, HighestUnlockedLevel);
                PlayerPrefs.Save();
            }
        }
    }
}
