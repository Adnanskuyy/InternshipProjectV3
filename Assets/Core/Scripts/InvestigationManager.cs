using System;
using System.Collections.Generic;
using UnityEngine;
using InvestigationGame.Data;
using InvestigationGame.UI;

namespace InvestigationGame.Core
{
    public class SuspectResult
    {
        public SuspectData Suspect;
        public Verdict PlayerVerdict;
        public bool IsCorrect;
    }

    public class InvestigationResult
    {
        public bool IsSuccess;
        public List<SuspectResult> Details = new List<SuspectResult>();
    }

    public class InvestigationManager : MonoBehaviour
    {
        public static InvestigationManager Instance { get; private set; }

        public bool HasUsedUrineTest { get; private set; }

        public event Action<InvestigationResult> OnInvestigationComplete;

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
            // Reset state on start/restart
            HasUsedUrineTest = false;

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

        public void CompleteInvestigation(Dictionary<SuspectData, Verdict> verdicts)
        {
            var result = new InvestigationResult();
            bool allCorrect = true;

            foreach (var kvp in verdicts)
            {
                var suspect = kvp.Key;
                var verdict = kvp.Value;

                bool isCorrect = false;
                if (verdict == Verdict.Pengguna)
                {
                    if (suspect.Role == SuspectRole.Pengguna)
                    {
                        isCorrect = true;
                    }
                }
                else if (verdict == Verdict.OrangBiasa)
                {
                    if (suspect.Role == SuspectRole.OrangBiasa)
                    {
                        isCorrect = true;
                    }
                }
                else if (verdict == Verdict.Pengedar)
                {
                    if (suspect.Role == SuspectRole.Pengedar)
                    {
                        isCorrect = true;
                    }
                }

                if (!isCorrect)
                {
                    allCorrect = false;
                }

                result.Details.Add(new SuspectResult
                {
                    Suspect = suspect,
                    PlayerVerdict = verdict,
                    IsCorrect = isCorrect
                });
            }

            result.IsSuccess = allCorrect;

            OnInvestigationComplete?.Invoke(result);
        }
    }

    public enum Verdict
    {
        None,
        Pengedar,
        Pengguna,
        OrangBiasa
    }
}