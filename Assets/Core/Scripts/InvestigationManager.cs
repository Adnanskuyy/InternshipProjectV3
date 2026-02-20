using System.Collections.Generic;
using UnityEngine;
using InvestigationGame.Data;
using InvestigationGame.UI;

namespace InvestigationGame.Core
{
    public class InvestigationManager : MonoBehaviour
    {
        public static InvestigationManager Instance { get; private set; }

        public bool HasUsedUrineTest { get; private set; }

        [SerializeField] private List<SuspectData> masterSuspectPool;
        [SerializeField] private GameUIController uiController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (masterSuspectPool == null || masterSuspectPool.Count == 0)
            {
                Debug.LogError("Master suspect pool is empty!");
                return;
            }

            var selectedSuspects = SuspectSelector.PickSuspects(masterSuspectPool);

            if (uiController != null)
            {
                uiController.InitializeUI(selectedSuspects);
            }
            else
            {
                Debug.LogError("GameUIController is not assigned in InvestigationManager!");
            }
        }

        public bool UseUrineTest()
        {
            if (HasUsedUrineTest)
            {
                Debug.LogWarning("Urine test already used!");
                return false;
            }
            
            HasUsedUrineTest = true;
            return true;
        }

        public void SubmitVerdict(SuspectData suspect, Verdict verdict)
        {
            Debug.Log($"Verdict for {suspect.SuspectName} submitted: {verdict}");
        }
    }

    public enum Verdict
    {
        Unsure,
        Positive,
        Negative
    }
}