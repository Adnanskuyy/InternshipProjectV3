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

            UpdateStatus(Verdict.None);
        }

        private void OnElementClicked(ClickEvent evt)
        {
            onClickCallback?.Invoke(suspectData);
        }

        public void UpdateStatus(Verdict verdict)
        {
            if (statusLabel == null) return;

            if (verdict == Verdict.None)
            {
                statusLabel.text = "BELUM DITENTUKAN";
            }
            else if (verdict == Verdict.OrangBiasa)
            {
                statusLabel.text = "ORANG BIASA";
            }
            else
            {
                statusLabel.text = verdict.ToString().ToUpper();
            }
            
            statusLabel.RemoveFromClassList("status-positive");
            statusLabel.RemoveFromClassList("status-negative");
            statusLabel.RemoveFromClassList("status-unsure");

            switch (verdict)
            {
                case Verdict.Pengedar:
                    statusLabel.AddToClassList("status-positive");
                    break;
                case Verdict.Pengguna:
                    statusLabel.AddToClassList("status-negative");
                    break;
                case Verdict.OrangBiasa:
                    statusLabel.AddToClassList("status-unsure"); // Keeping CSS names for now, mapping them
                    break;
                case Verdict.None:
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
