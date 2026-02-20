using UnityEngine;
using UnityEngine.UIElements;
using InvestigationGame.Data;
using InvestigationGame.Core;

namespace InvestigationGame.UI
{
    public class DetailPanelController
    {
        private VisualElement panelRoot;
        private Label nameLabel;
        private VisualElement detailImage;
        private Label physicalText;
        private Label behaviorText;
        private Label rumorsText;

        private Button closeBtn;
        private Button urineTestBtn;
        private Button positiveBtn;
        private Button negativeBtn;
        private Button unsureBtn;
        private VisualElement urineTestStamp;

        private SuspectData currentSuspect;
        private System.Action<SuspectData, Verdict> onVerdictSubmitted;

        public DetailPanelController(VisualElement root, System.Action<SuspectData, Verdict> onVerdict)
        {
            panelRoot = root;
            this.onVerdictSubmitted = onVerdict;

            // Query elements
            nameLabel = panelRoot.Q<Label>("DetailNameLabel");
            detailImage = panelRoot.Q<VisualElement>("DetailImage");
            physicalText = panelRoot.Q<Label>("PhysicalText");
            behaviorText = panelRoot.Q<Label>("BehaviorText");
            rumorsText = panelRoot.Q<Label>("RumorsText");

            closeBtn = panelRoot.Q<Button>("CloseDetailBtn");
            urineTestBtn = panelRoot.Q<Button>("UrineTestBtn");
            positiveBtn = panelRoot.Q<Button>("VerdictPositiveBtn");
            negativeBtn = panelRoot.Q<Button>("VerdictNegativeBtn");
            unsureBtn = panelRoot.Q<Button>("VerdictUnsureBtn");
            urineTestStamp = panelRoot.Q<VisualElement>("UrineTestStamp");

            // Register events
            closeBtn.clicked += Hide;
            urineTestBtn.clicked += OnUrineTestClicked;
            
            positiveBtn.clicked += () => SubmitVerdict(Verdict.Positive);
            negativeBtn.clicked += () => SubmitVerdict(Verdict.Negative);
            unsureBtn.clicked += () => SubmitVerdict(Verdict.Unsure);

            Hide(); // Hidden by default
        }

        public void Show(SuspectData suspect)
        {
            currentSuspect = suspect;

            // Populate data
            nameLabel.text = suspect.SuspectName;
            physicalText.text = suspect.PhysicalCharacteristics.Description;
            behaviorText.text = suspect.Behavior.Description;
            rumorsText.text = suspect.Rumors;

            if (suspect.PolaroidSprite != null)
            {
                detailImage.style.backgroundImage = new StyleBackground(suspect.PolaroidSprite);
            }

            // Update UI State for urine test
            UpdateUrineTestButtonState();

            panelRoot.style.display = DisplayStyle.Flex; // Show
        }

        public void Hide()
        {
            panelRoot.style.display = DisplayStyle.None; // Hide
            currentSuspect = null;
            if (urineTestStamp != null)
            {
                urineTestStamp.style.display = DisplayStyle.None;
            }
        }

        private void OnUrineTestClicked()
        {
            if (InvestigationManager.Instance != null && InvestigationManager.Instance.UseUrineTest())
            {
                // Uncover the truth
                Debug.Log($"Urine test used on {currentSuspect.SuspectName}! Truth: {(currentSuspect.IsUser ? "Positive" : "Negative")}");
                
                if (urineTestStamp != null)
                {
                    urineTestStamp.style.display = DisplayStyle.Flex;
                    urineTestStamp.style.backgroundColor = currentSuspect.IsUser ? new StyleColor(Color.red) : new StyleColor(Color.green);
                }

                UpdateUrineTestButtonState();
            }
        }

        private void UpdateUrineTestButtonState()
        {
            if (InvestigationManager.Instance != null && InvestigationManager.Instance.HasUsedUrineTest)
            {
                urineTestBtn.SetEnabled(false);
                urineTestBtn.text = "Urine Test Used";
            }
        }

        private void SubmitVerdict(Verdict verdict)
        {
            if (currentSuspect != null)
            {
                onVerdictSubmitted?.Invoke(currentSuspect, verdict);
                Hide();
            }
        }
    }
}