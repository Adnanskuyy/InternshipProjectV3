using UnityEngine;

namespace InvestigationGame.Data
{
    public enum SuspectRole
    {
        OrangBiasa,
        Pengguna,
        Pengedar
    }

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
        public SuspectRole Role;
        [Tooltip("True for Positive, False for Negative")]
        public bool UrineTestResult;

        [Header("Runtime State")]
        [HideInInspector]
        public bool hasBeenTested;

        private void OnEnable()
        {
            hasBeenTested = false; // Reset state when the game starts or SO is loaded
        }
    }
}
