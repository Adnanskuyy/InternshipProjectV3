using System.Collections.Generic;
using UnityEngine;

namespace InvestigationGame.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "GameData/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public List<LevelConfig> Levels = new List<LevelConfig>();
    }
}
