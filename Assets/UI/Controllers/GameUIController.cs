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
        private Dictionary<SuspectData, SuspectUIElement> suspectUIElements = new Dictionary<SuspectData, SuspectUIElement>();
        private VisualElement root;
        private Button finalSubmitBtn;
        private VisualElement verdictOverlay;
        private Button playAgainBtn;
        private Button helpBtn;
        
        public TutorialManager TutorialManager { get; private set; }

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            // Initialize Detail Panel
            var detailPanelElement = root.Q<VisualElement>("DetailPanel");
            if (detailPanelController == null)
            {
                detailPanelController = new DetailPanelController(detailPanelElement, OnVerdictSubmitted);
            }
            else
            {
                detailPanelController.SetRoot(detailPanelElement);
            }

            // Setup Final Submit Button
            finalSubmitBtn = root.Q<Button>("FinalSubmitBtn");
            if (finalSubmitBtn != null)
            {
                finalSubmitBtn.RegisterCallback<ClickEvent>(OnFinalSubmitClick);
                UpdateSubmitButtonState();
            }

            // Setup Verdict Overlay
            verdictOverlay = root.Q<VisualElement>("VerdictOverlay");
            playAgainBtn = root.Q<Button>("PlayAgainBtn");
            if (playAgainBtn != null)
            {
                playAgainBtn.RegisterCallback<ClickEvent>(OnPlayAgainClick);
            }

            // Setup Tutorial
            if (TutorialManager == null)
            {
                TutorialManager = new TutorialManager(root, this);
            }
            else
            {
                TutorialManager.SetRoot(root);
            }

            helpBtn = root.Q<Button>("HelpBtn");
            if (helpBtn != null)
            {
                helpBtn.RegisterCallback<ClickEvent>(OnHelpClick);
            }
        }

        private void OnDisable()
        {
            if (finalSubmitBtn != null) finalSubmitBtn.UnregisterCallback<ClickEvent>(OnFinalSubmitClick);
            if (playAgainBtn != null) playAgainBtn.UnregisterCallback<ClickEvent>(OnPlayAgainClick);
            if (helpBtn != null) helpBtn.UnregisterCallback<ClickEvent>(OnHelpClick);

            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.OnInvestigationComplete -= HandleInvestigationComplete;
            }

            // Cleanup suspect UI elements
            foreach (var suspectUI in suspectUIElements.Values)
            {
                suspectUI.Cleanup();
            }
        }

        private void Start()
        {
            // Subscribe to event here to avoid race conditions when reloading the scene
            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.OnInvestigationComplete -= HandleInvestigationComplete;
                InvestigationManager.Instance.OnInvestigationComplete += HandleInvestigationComplete;
            }

            // Try to start the tutorial on first run
            if (TutorialManager != null)
            {
                TutorialManager.TryStartTutorial();
            }
        }

        private void OnFinalSubmitClick(ClickEvent evt) => OnFinalSubmit();
        private void OnPlayAgainClick(ClickEvent evt) => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        private void OnHelpClick(ClickEvent evt)
        {
            // Replay the intro video instead of the tutorial overlay
            global::Core.Scripts.IntroVideoManager.ForceReplay = true;
            SceneManager.LoadScene("IntroScene");
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
                suspectUIElements.Clear();

                foreach (var suspect in selectedSuspects)
                {
                    // Reset the tested state whenever we initialize them for a new game
                    suspect.hasBeenTested = false;

                    var suspectUI = new SuspectUIElement();
                    suspectUI.Initialize(suspectTemplate, suspect, OnSuspectClicked);
                    polaroidGrid.Add(suspectUI);
                    verdicts[suspect] = Verdict.Unsure; // Default verdict
                    suspectUIElements[suspect] = suspectUI;
                }
            }
            
            UpdateSubmitButtonState();
        }

        private void OnSuspectClicked(SuspectData suspect)
        {
            detailPanelController.Show(suspect);
            TutorialManager?.NotifySuspectClicked();
        }

        private void OnVerdictSubmitted(SuspectData suspect, Verdict verdict)
        {
            verdicts[suspect] = verdict;

            if (suspectUIElements.TryGetValue(suspect, out var suspectUI))
            {
                suspectUI.UpdateStatus(verdict);
            }

            if (InvestigationManager.Instance != null)
            {
                InvestigationManager.Instance.SubmitVerdict(suspect, verdict);
            }
            UpdateSubmitButtonState();
            TutorialManager?.NotifyVerdictSubmitted();
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

