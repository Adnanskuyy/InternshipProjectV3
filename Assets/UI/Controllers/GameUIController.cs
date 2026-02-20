using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using InvestigationGame.Data;
using InvestigationGame.Core;
using System.Linq;

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
        private Button finalSubmitBtn;
        private VisualElement verdictOverlay;

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
            finalSubmitBtn = root.Q<Button>("FinalSubmitBtn");
            if (finalSubmitBtn != null)
            {
                finalSubmitBtn.clicked += OnFinalSubmit;
                finalSubmitBtn.SetEnabled(false); // Disabled initially until all suspects are judged
            }

            // Setup Verdict Overlay
            verdictOverlay = root.Q<VisualElement>("VerdictOverlay");
            var playAgainBtn = root.Q<Button>("PlayAgainBtn");
            if (playAgainBtn != null)
            {
                playAgainBtn.clicked += () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void Start()
        {
            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.OnInvestigationComplete += HandleInvestigationComplete;
            }
        }

        private void OnDestroy()
        {
            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.OnInvestigationComplete -= HandleInvestigationComplete;
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
            
            UpdateSubmitButtonState();
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
            UpdateSubmitButtonState();
        }

        private void UpdateSubmitButtonState()
        {
            if (finalSubmitBtn == null) return;

            // Ensure all suspects have a verdict other than Unsure
            bool allJudged = verdicts.Count > 0 && verdicts.Values.All(v => v != Verdict.Unsure);
            finalSubmitBtn.SetEnabled(allJudged);
        }

        private void OnFinalSubmit()
        {
            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.CompleteInvestigation(verdicts);
            }
        }

        private void HandleInvestigationComplete(InvestigationResult result)
        {
            if (verdictOverlay == null) return;

            var titleLabel = verdictOverlay.Q<Label>("VerdictTitle");
            var resultLabel = verdictOverlay.Q<Label>("VerdictResult");
            var detailsList = verdictOverlay.Q<ScrollView>("VerdictDetailsList");

            if (resultLabel != null)
            {
                resultLabel.text = result.IsSuccess ? "SUCCESS: You found the user!" : "FAILED: The user slipped away...";
                resultLabel.RemoveFromClassList("result-success");
                resultLabel.RemoveFromClassList("result-failure");
                resultLabel.AddToClassList(result.IsSuccess ? "result-success" : "result-failure");
            }

            if (detailsList != null)
            {
                detailsList.Clear();
                foreach (var detail in result.Details)
                {
                    var item = new VisualElement();
                    item.AddToClassList("suspect-result-item");

                    var nameLabel = new Label(detail.Suspect.SuspectName);
                    nameLabel.AddToClassList("suspect-result-name");

                    var statusLabel = new Label(detail.IsCorrect ? "CORRECT" : "WRONG");
                    statusLabel.AddToClassList("suspect-result-status");
                    statusLabel.style.color = detail.IsCorrect ? new StyleColor(new Color(0.2f, 0.8f, 0.2f)) : new StyleColor(new Color(0.8f, 0.2f, 0.2f));

                    item.Add(nameLabel);
                    item.Add(statusLabel);
                    detailsList.Add(item);
                }
            }

            verdictOverlay.style.display = DisplayStyle.Flex;
        }
    }
}