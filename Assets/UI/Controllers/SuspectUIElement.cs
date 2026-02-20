using UnityEngine;
using UnityEngine.UIElements;
using InvestigationGame.Data;

namespace InvestigationGame.UI
{
    public class SuspectUIElement
    {
        public VisualElement Root { get; private set; }
        private SuspectData suspectData;
        private System.Action<SuspectData> onClickCallback;

        public SuspectUIElement(VisualTreeAsset template, SuspectData data, System.Action<SuspectData> onClick)
        {
            this.suspectData = data;
            this.onClickCallback = onClick;

            // Instantiate from template
            Root = template.Instantiate();
            Root.AddToClassList("polaroid-container-item"); // Optional spacing class

            // Get elements
            var nameLabel = Root.Q<Label>("SuspectNameLabel");
            var imageContainer = Root.Q<VisualElement>("PolaroidImage");

            // Assign data
            if (nameLabel != null) nameLabel.text = data.SuspectName;
            
            if (imageContainer != null && data.PolaroidSprite != null)
            {
                imageContainer.style.backgroundImage = new StyleBackground(data.PolaroidSprite);
            }

            // Register click event
            // Using a clickable to handle pointer events easily
            var clickable = new Clickable(() => onClickCallback?.Invoke(suspectData));
            Root.AddManipulator(clickable);
        }
    }
}
