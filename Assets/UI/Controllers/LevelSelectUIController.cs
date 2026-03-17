using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using InvestigationGame.Data;
using InvestigationGame.Core;

namespace InvestigationGame.UI
{
    public class LevelSelectUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private VisualTreeAsset levelNodeTemplate;

        private VisualElement root;
        private VisualElement mapContainer;

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            mapContainer = root.Q<VisualElement>("MapContainer");
            
            var backBtn = root.Q<Button>("BackBtn");
            if (backBtn != null)
            {
                backBtn.RegisterCallback<ClickEvent>(evt => SceneManager.LoadScene("IntroScene"));
            }

            GenerateMapNodes();
        }

        private void GenerateMapNodes()
        {
            if (mapContainer == null || gameSettings == null || levelNodeTemplate == null) return;

            mapContainer.Clear();

            int highestUnlocked = GameProgressManager.Instance != null ? GameProgressManager.Instance.HighestUnlockedLevel : 0;

            for (int i = 0; i < gameSettings.Levels.Count; i++)
            {
                int levelIndex = i;
                var config = gameSettings.Levels[i];

                var node = levelNodeTemplate.Instantiate();
                var button = node.Q<Button>("LevelBtn");
                var label = node.Q<Label>("LevelName");

                if (label != null)
                {
                    label.text = string.IsNullOrEmpty(config.LevelName) ? $"Level {levelIndex + 1}" : config.LevelName;
                }

                if (button != null)
                {
                    if (levelIndex <= highestUnlocked)
                    {
                        button.RemoveFromClassList("locked-level");
                        button.AddToClassList("unlocked-level");
                        button.RegisterCallback<ClickEvent>(evt => OnLevelClicked(levelIndex));
                    }
                    else
                    {
                        button.RemoveFromClassList("unlocked-level");
                        button.AddToClassList("locked-level");
                        button.SetEnabled(false);
                    }
                }

                mapContainer.Add(node);
            }
        }

        private void OnLevelClicked(int index)
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.CurrentLevelToPlay = index;
            }
            SceneManager.LoadScene("InvestigationScene");
        }
    }
}
