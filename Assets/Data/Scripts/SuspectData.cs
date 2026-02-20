using UnityEngine;

namespace InvestigationGame.Data
{
    [System.Serializable]
    public struct EvidenceData
    {
        [TextArea]
        public string Description;
        public Sprite Image;
    }

    [CreateAssetMenu(fileName = "NewSuspect", menuName = "GameData/Suspect")]
    public class SuspectData : ScriptableObject
    {
        [Header("Basic Info")]
        public string SuspectName;
        public Sprite PolaroidSprite;

        [Header("Evidence")]
        public EvidenceData PhysicalCharacteristics;
        public EvidenceData Behavior;

        [Header("Narrative")]
        [TextArea]
        public string Rumors;

        [Header("Truth")]
        public bool IsUser;
    }
}
