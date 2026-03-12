using UnityEngine;
using UnityEngine.UIElements;
using InvestigationGame.Data;
using InvestigationGame.Core;

namespace InvestigationGame.UI
{
    [UxmlElement]
    public partial class SuspectUIElement : VisualElement
    {
        private Label nameLabel;
        private VisualElement imageContainer;
        private Label statusLabel;
        private SuspectData suspectData;
        private System.Action<SuspectData> onClickCallback;

        public SuspectUIElement() { }

        public void Initialize(VisualTreeAsset template, SuspectData data, System.Action<SuspectData> onClick)
        {
            this.suspectData = data;
            this.onClickCallback = onClick;

            // Clear any existing content and instantiate from template
            this.Clear();
            template.CloneTree(this);
            this.AddToClassList("polaroid-container-item");

            // Get elements
            nameLabel = this.Q<Label>("SuspectNameLabel");
            imageContainer = this.Q<VisualElement>("PolaroidImage");
            statusLabel = this.Q<Label>("StatusLabel");

            // Assign data
            if (nameLabel != null) nameLabel.text = data.SuspectName;
            
            if (imageContainer != null && data.PolaroidSprite != null)
            {
                imageContainer.style.backgroundImage = new StyleBackground(data.PolaroidSprite);
            }

            // Register click event using standard RegisterCallback for Unity 6 consistency
            this.RegisterCallback<ClickEvent>(OnElementClicked);

            UpdateStatus(Verdict.Unsure);
        }

        private void OnElementClicked(ClickEvent evt)
        {
            onClickCallback?.Invoke(suspectData);
        }

        public void UpdateStatus(Verdict verdict)
        {
            if (statusLabel == null) return;

            string statusText = "RAGU-RAGU";
            switch (verdict)
            {
                case Verdict.Positive: statusText = "POSITIF"; break;
                case Verdict.Negative: statusText = "NEGATIF"; break;
                case Verdict.Unsure: statusText = "RAGU-RAGU"; break;
            }
            statusLabel.text = statusText;
            
            statusLabel.RemoveFromClassList("status-positive");
            statusLabel.RemoveFromClassList("status-negative");
            statusLabel.RemoveFromClassList("status-unsure");

            switch (verdict)
            {
                case Verdict.Positive:
                    statusLabel.AddToClassList("status-positive");
                    break;
                case Verdict.Negative:
                    statusLabel.AddToClassList("status-negative");
                    break;
                case Verdict.Unsure:
                default:
                    statusLabel.AddToClassList("status-unsure");
                    break;
            }
        }

        public void Cleanup()
        {
            this.UnregisterCallback<ClickEvent>(OnElementClicked);
        }
    }
}
