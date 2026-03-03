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
        private VisualElement physicalThumbnail;
        private Label behaviorText;
        private VisualElement behaviorThumbnail;
        private Label rumorsText;

        private Button closeBtn;
        private Button urineTestBtn;
        private Button positiveBtn;
        private Button negativeBtn;
        private Button unsureBtn;
        private VisualElement urineTestStamp;

        // Expanded Image Overlay Elements
        private VisualElement expandedImageOverlay;
        private VisualElement expandedImageContent;
        private Button expandedImageCloseBtn;

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
            physicalThumbnail = panelRoot.Q<VisualElement>("PhysicalImageThumbnail");
            behaviorText = panelRoot.Q<Label>("BehaviorText");
            behaviorThumbnail = panelRoot.Q<VisualElement>("BehaviorImageThumbnail");
            rumorsText = panelRoot.Q<Label>("RumorsText");

            closeBtn = panelRoot.Q<Button>("CloseDetailBtn");
            urineTestBtn = panelRoot.Q<Button>("UrineTestBtn");
            positiveBtn = panelRoot.Q<Button>("VerdictPositiveBtn");
            negativeBtn = panelRoot.Q<Button>("VerdictNegativeBtn");
            unsureBtn = panelRoot.Q<Button>("VerdictUnsureBtn");
            urineTestStamp = panelRoot.Q<VisualElement>("UrineTestStamp");

            // Query expanded image overlay elements (assuming they are in the root scope or a parent scope)
            var uiDocument = panelRoot.parent?.parent; // Navigate up to find the document root if necessary, or pass it in. 
            // In our case, the DetailPanel and ExpandedImageOverlay are siblings under the main-container.
            // Let's try querying from the panelRoot's parent.
            var mainContainer = panelRoot.parent;
            if (mainContainer != null)
            {
                expandedImageOverlay = mainContainer.Q<VisualElement>("ExpandedImageOverlay");
                expandedImageContent = mainContainer.Q<VisualElement>("ExpandedImageContent");
                expandedImageCloseBtn = mainContainer.Q<Button>("ExpandedImageCloseBtn");

                if (expandedImageCloseBtn != null)
                {
                    expandedImageCloseBtn.clicked += HideExpandedImage;
                }
            }

            // Register events
            closeBtn.clicked += Hide;
            urineTestBtn.clicked += OnUrineTestClicked; 

            positiveBtn.clicked += () => SubmitVerdict(Verdict.Positive);
            negativeBtn.clicked += () => SubmitVerdict(Verdict.Negative);
            unsureBtn.clicked += () => SubmitVerdict(Verdict.Unsure);

            // Thumbnail Click Events
            if (physicalThumbnail != null)
            {
                physicalThumbnail.RegisterCallback<ClickEvent>(evt =>
                {
                    if (currentSuspect?.PhysicalCharacteristics.Image != null)
                    {
                        ShowExpandedImage(currentSuspect.PhysicalCharacteristics.Image);
                    }
                });
            }

            if (behaviorThumbnail != null)
            {
                behaviorThumbnail.RegisterCallback<ClickEvent>(evt =>
                {
                    if (currentSuspect?.Behavior.Image != null)
                    {
                        ShowExpandedImage(currentSuspect.Behavior.Image);
                    }
                });
            }

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

            // Handle Thumbnails
            if (physicalThumbnail != null)
            {
                if (suspect.PhysicalCharacteristics.Image != null)
                {
                    physicalThumbnail.style.display = DisplayStyle.Flex;
                    physicalThumbnail.style.backgroundImage = new StyleBackground(suspect.PhysicalCharacteristics.Image);
                }
                else
                {
                    physicalThumbnail.style.display = DisplayStyle.None;
                }
            }

            if (behaviorThumbnail != null)
            {
                if (suspect.Behavior.Image != null)
                {
                    behaviorThumbnail.style.display = DisplayStyle.Flex;
                    behaviorThumbnail.style.backgroundImage = new StyleBackground(suspect.Behavior.Image);
                }
                else
                {
                    behaviorThumbnail.style.display = DisplayStyle.None;
                }
            }

            if (suspect.PolaroidSprite != null)
            {
                detailImage.style.backgroundImage = new StyleBackground(suspect.PolaroidSprite);
            }

            // Update UI State for urine test
            UpdateUrineTestButtonState();

            // Check if this specific suspect has already been tested
            if (currentSuspect.hasBeenTested && urineTestStamp != null)
            {
                urineTestStamp.style.display = DisplayStyle.Flex;
                urineTestStamp.style.backgroundColor = currentSuspect.IsUser ? new StyleColor(Color.green) : new StyleColor(Color.red);
                var stampLabel = urineTestStamp.Q<Label>("UrineTestStampLabel");
                if (stampLabel != null)
                {
                    stampLabel.text = currentSuspect.IsUser ? "TEST RESULT: POSITIVE" : "TEST RESULT: NEGATIVE";
                }
            }
            else if (urineTestStamp != null)
            {
                urineTestStamp.style.display = DisplayStyle.None;
            }

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

                currentSuspect.hasBeenTested = true;

                if (urineTestStamp != null)
                {
                    urineTestStamp.style.display = DisplayStyle.Flex;
                    urineTestStamp.style.backgroundColor = currentSuspect.IsUser ? new StyleColor(Color.green) : new StyleColor(Color.red);

                    var stampLabel = urineTestStamp.Q<Label>("UrineTestStampLabel");
                    if (stampLabel != null)
                    {
                        stampLabel.text = currentSuspect.IsUser ? "TEST RESULT: POSITIVE" : "TEST RESULT: NEGATIVE";
                    }
                }

                UpdateUrineTestButtonState();
            }
        }

        private void UpdateUrineTestButtonState()
        {
            urineTestBtn.style.display = DisplayStyle.Flex; // Always keep it in layout

            if (InvestigationManager.Instance != null && InvestigationManager.Instance.HasUsedUrineTest)
            {
                urineTestBtn.SetEnabled(false);
                urineTestBtn.text = "Urine Test (Used)";
            }
            else
            {
                urineTestBtn.SetEnabled(true);
                urineTestBtn.text = "Use Urine Test (1x)";
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

        private void ShowExpandedImage(Sprite sprite)
        {
            if (expandedImageOverlay != null && expandedImageContent != null)
            {
                expandedImageContent.style.backgroundImage = new StyleBackground(sprite);
                expandedImageOverlay.style.display = DisplayStyle.Flex;
            }
        }

        private void HideExpandedImage()
        {
            if (expandedImageOverlay != null)
            {
                expandedImageOverlay.style.display = DisplayStyle.None;
            }
        }
    }
}