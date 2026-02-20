using UnityEngine;

namespace InvestigationGame.Core
{
    public enum Verdict
    {
        Unsure,
        Positive,
        Negative
    }

    public class InvestigationManager : MonoBehaviour
    {
        public static InvestigationManager Instance { get; private set; }

        public bool HasUsedUrineTest { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        public bool UseUrineTest()
        {
            if (HasUsedUrineTest)
            {
                Debug.LogWarning("Urine test has already been used this session.");
                return false;
            }

            HasUsedUrineTest = true;
            Debug.Log("Urine test used.");
            return true;
        }

        public void SubmitVerdict(Data.SuspectData suspect, Verdict verdict)
        {
            if (suspect == null)
            {
                Debug.LogError("Tried to submit verdict for null suspect!");
                return;
            }

            // Here we would typically store this verdict for the end-game summary
            Debug.Log($"Verdict for {suspect.SuspectName} submitted: {verdict}");
        }
    }
}
