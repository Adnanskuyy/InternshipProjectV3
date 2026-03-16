using UnityEngine;

namespace InvestigationGame.Data
{
    [System.Serializable]
    public struct LevelConfig
    {
        public string LevelName;
        public int TotalSuspects;
        public int CulpritsCount;
    }
}
