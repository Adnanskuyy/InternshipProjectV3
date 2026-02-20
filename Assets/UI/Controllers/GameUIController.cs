using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using InvestigationGame.Data;
using InvestigationGame.Core;

namespace InvestigationGame.UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset suspectTemplate;

        // Exposed for SceneSetupUtility
        public UIDocument UIDocument { get => uiDocument; set => uiDocument = value; }
        public VisualTreeAsset SuspectTemplate { get => suspectTemplate; set => suspectTemplate = value; }

        private DetailPanelController detailPanelController;
        private Dictionary<SuspectData, Verdict> verdicts = new Dictionary<SuspectData, Verdict>();
        private VisualElement root;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned to GameUIController");
                return;
            }

            root = uiDocument.rootVisualElement;

            // Initialize Detail Panel
            var detailPanelElement = root.Q<VisualElement>("DetailPanel");
            detailPanelController = new DetailPanelController(detailPanelElement, OnVerdictSubmitted);

            // Setup Final Submit Button
            var finalSubmitBtn = root.Q<Button>("FinalSubmitBtn");
            if (finalSubmitBtn != null)
            {
                finalSubmitBtn.clicked += OnFinalSubmit;
            }
        }

        public void InitializeUI(List<SuspectData> selectedSuspects)
        {
            if (root == null)
            {
                if (uiDocument != null) root = uiDocument.rootVisualElement;
                if (root == null) return;
            }

            // Populate Suspects Grid
            var polaroidGrid = root.Q<VisualElement>("PolaroidGrid");
            if (polaroidGrid != null && suspectTemplate != null)
            {
                polaroidGrid.Clear(); // Clear any existing
                verdicts.Clear();

                foreach (var suspect in selectedSuspects)
                {
                    var suspectUI = new SuspectUIElement(suspectTemplate, suspect, OnSuspectClicked);
                    polaroidGrid.Add(suspectUI.Root);
                    verdicts[suspect] = Verdict.Unsure; // Default verdict
                }
            }
        }

        private void OnSuspectClicked(SuspectData suspect)
        {
            detailPanelController.Show(suspect);
        }

        private void OnVerdictSubmitted(SuspectData suspect, Verdict verdict)
        {
            verdicts[suspect] = verdict;
            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.SubmitVerdict(suspect, verdict);
            }
        }

        private void OnFinalSubmit()
        {
            Debug.Log("Finalizing Investigation...");
            foreach (var kvp in verdicts)
            {
                Debug.Log($"- {kvp.Key.SuspectName}: {kvp.Value}");
            }
            // Transition to end screen logic here
        }
    }
}