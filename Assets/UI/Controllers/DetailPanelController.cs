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
            this.onVerdictSubmitted = onVerdict;
            SetRoot(root);
        }

        public void SetRoot(VisualElement root)
        {
            Cleanup(); // Unregister from old elements

            panelRoot = root;

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

            var mainContainer = panelRoot.parent;
            if (mainContainer != null)
            {
                expandedImageOverlay = mainContainer.Q<VisualElement>("ExpandedImageOverlay");
                expandedImageContent = mainContainer.Q<VisualElement>("ExpandedImageContent");
                expandedImageCloseBtn = mainContainer.Q<Button>("ExpandedImageCloseBtn");

                if (expandedImageCloseBtn != null)
                {
                    expandedImageCloseBtn.RegisterCallback<ClickEvent>(OnExpandedImageCloseClick);
                }
            }

            // Register events
            if (closeBtn != null) closeBtn.RegisterCallback<ClickEvent>(OnCloseClick);
            if (urineTestBtn != null) urineTestBtn.RegisterCallback<ClickEvent>(OnUrineTestClick); 

            if (positiveBtn != null) positiveBtn.RegisterCallback<ClickEvent>(OnPositiveClick);
            if (negativeBtn != null) negativeBtn.RegisterCallback<ClickEvent>(OnNegativeClick);
            if (unsureBtn != null) unsureBtn.RegisterCallback<ClickEvent>(OnUnsureClick);

            // Thumbnail Click Events
            if (physicalThumbnail != null) physicalThumbnail.RegisterCallback<ClickEvent>(OnPhysicalThumbnailClick);
            if (behaviorThumbnail != null) behaviorThumbnail.RegisterCallback<ClickEvent>(OnBehaviorThumbnailClick);

            Hide(); // Hidden by default
        }

        public void Cleanup()
        {
            if (closeBtn != null) closeBtn.UnregisterCallback<ClickEvent>(OnCloseClick);
            if (urineTestBtn != null) urineTestBtn.UnregisterCallback<ClickEvent>(OnUrineTestClick);
            if (positiveBtn != null) positiveBtn.UnregisterCallback<ClickEvent>(OnPositiveClick);
            if (negativeBtn != null) negativeBtn.UnregisterCallback<ClickEvent>(OnNegativeClick);
            if (unsureBtn != null) unsureBtn.UnregisterCallback<ClickEvent>(OnUnsureClick);
            if (physicalThumbnail != null) physicalThumbnail.UnregisterCallback<ClickEvent>(OnPhysicalThumbnailClick);
            if (behaviorThumbnail != null) behaviorThumbnail.UnregisterCallback<ClickEvent>(OnBehaviorThumbnailClick);
            if (expandedImageCloseBtn != null) expandedImageCloseBtn.UnregisterCallback<ClickEvent>(OnExpandedImageCloseClick);
        }

        private void OnCloseClick(ClickEvent evt) => Hide();
        private void OnUrineTestClick(ClickEvent evt) => OnUrineTestClicked();
        private void OnPositiveClick(ClickEvent evt) => SubmitVerdict(Verdict.Positive);
        private void OnNegativeClick(ClickEvent evt) => SubmitVerdict(Verdict.Negative);
        private void OnUnsureClick(ClickEvent evt) => SubmitVerdict(Verdict.Unsure);
        private void OnExpandedImageCloseClick(ClickEvent evt) => HideExpandedImage();
        
        private void OnPhysicalThumbnailClick(ClickEvent evt)
        {
            if (currentSuspect?.PhysicalCharacteristics.Image != null)
                ShowExpandedImage(currentSuspect.PhysicalCharacteristics.Image);
        }
        
        private void OnBehaviorThumbnailClick(ClickEvent evt)
        {
            if (currentSuspect?.Behavior.Image != null)
                ShowExpandedImage(currentSuspect.Behavior.Image);
        }

        public void Show(SuspectData suspect)
        {
            currentSuspect = suspect;

            // Populate data
            if (nameLabel != null) nameLabel.text = suspect.SuspectName;
            if (physicalText != null) physicalText.text = suspect.PhysicalCharacteristics.Description;
            if (behaviorText != null) behaviorText.text = suspect.Behavior.Description;
            if (rumorsText != null) rumorsText.text = suspect.Rumors;

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

            if (detailImage != null && suspect.PolaroidSprite != null)
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
                    stampLabel.text = currentSuspect.IsUser ? "HASIL TES: POSITIF" : "HASIL TES: NEGATIF";
                }
            }
            else if (urineTestStamp != null)
            {
                urineTestStamp.style.display = DisplayStyle.None;
            }

            if (panelRoot != null) panelRoot.style.display = DisplayStyle.Flex; // Show
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.style.display = DisplayStyle.None; // Hide
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
                currentSuspect.hasBeenTested = true;

                if (urineTestStamp != null)
                {
                    urineTestStamp.style.display = DisplayStyle.Flex;
                    urineTestStamp.style.backgroundColor = currentSuspect.IsUser ? new StyleColor(Color.green) : new StyleColor(Color.red);

                    var stampLabel = urineTestStamp.Q<Label>("UrineTestStampLabel");
                    if (stampLabel != null)
                    {
                        stampLabel.text = currentSuspect.IsUser ? "HASIL TES: POSITIF" : "HASIL TES: NEGATIF";
                    }
                }

                UpdateUrineTestButtonState();
            }
        }

        private void UpdateUrineTestButtonState()
        {
            if (urineTestBtn == null) return;
            // Removed urineTestBtn.style.display = DisplayStyle.Flex; which might override flex styling from uxml

            if (InvestigationManager.Instance != null && InvestigationManager.Instance.HasUsedUrineTest)
            {
                urineTestBtn.SetEnabled(false);
                urineTestBtn.text = "Tes Urin (Sudah Digunakan)";
            }
            else
            {
                urineTestBtn.SetEnabled(true);
                urineTestBtn.text = "Gunakan tes urin (1x)"; // Localized back
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
