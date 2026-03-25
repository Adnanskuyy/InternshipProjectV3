using System.Collections;
using System.Collections.Generic;
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
        private ScrollView scrollView;

        // Fixed positions for levels based on the SVG map
        private readonly Vector2[] levelPositions = new Vector2[]
        {
            new Vector2(300, 800),
            new Vector2(600, 500),
            new Vector2(960, 540),
            new Vector2(1300, 300),
            new Vector2(1600, 300)
        };

        private List<VisualElement> spawnedNodes = new List<VisualElement>();

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            mapContainer = root.Q<VisualElement>("MapContainer");
            scrollView = root.Q<ScrollView>();
            
            var backBtn = root.Q<Button>("BackBtn");
            if (backBtn != null)
            {
                backBtn.RegisterCallback<ClickEvent>(evt => SceneManager.LoadScene("IntroScene"));
            }

            GenerateMapNodes();
            StartCoroutine(AnimateNodesIn());
        }

        private void GenerateMapNodes()
        {
            if (mapContainer == null || gameSettings == null || levelNodeTemplate == null) return;

            mapContainer.Clear();
            spawnedNodes.Clear();

            int highestUnlocked = GameProgressManager.Instance != null ? GameProgressManager.Instance.HighestUnlockedLevel : 0;

            // Draw connection lines first so they render underneath the level nodes
            for (int i = 0; i < gameSettings.Levels.Count - 1; i++)
            {
                Vector2 p1 = i < levelPositions.Length ? levelPositions[i] : new Vector2(100 + i * 200, 500);
                Vector2 p2 = (i + 1) < levelPositions.Length ? levelPositions[i + 1] : new Vector2(100 + (i + 1) * 200, 500);
                
                DrawConnectionLine(p1, p2, i < highestUnlocked);
            }

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

                var nodeContainer = node.Q<VisualElement>(className: "level-node");
                if (nodeContainer != null)
                {
                    // Center the 100x100 node on the position
                    Vector2 pos = i < levelPositions.Length ? levelPositions[i] : new Vector2(100 + i * 200, 500);
                    nodeContainer.style.left = pos.x - 50;
                    nodeContainer.style.top = pos.y - 50;
                    spawnedNodes.Add(nodeContainer);
                }

                mapContainer.Add(node);
            }
        }

        private void DrawConnectionLine(Vector2 p1, Vector2 p2, bool isUnlocked)
        {
            float distance = Vector2.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
            Vector2 center = (p1 + p2) / 2f;

            var line = new VisualElement();
            line.pickingMode = PickingMode.Ignore; // Ensure lines don't block clicks
            line.AddToClassList("level-node"); // Reuse scale animation
            
            // Inline styles override USS
            line.style.position = Position.Absolute;
            line.style.width = distance;
            line.style.height = 12f;
            line.style.left = center.x - distance / 2f;
            line.style.top = center.y - 6f;
            
            var color = isUnlocked ? new Color(0.18f, 0.8f, 0.44f, 0.8f) : new Color(0.5f, 0.55f, 0.55f, 0.8f);
            line.style.backgroundColor = new StyleColor(color);
            line.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(angle)));
            line.style.borderTopLeftRadius = 6f;
            line.style.borderBottomLeftRadius = 6f;
            line.style.borderTopRightRadius = 6f;
            line.style.borderBottomRightRadius = 6f;

            spawnedNodes.Add(line);
            mapContainer.Add(line);
        }

        private IEnumerator AnimateNodesIn()
        {
            yield return new WaitForSeconds(0.1f);

            // Center scroll view on the highest unlocked level
            if (scrollView != null && spawnedNodes.Count > 0)
            {
                int highestUnlocked = GameProgressManager.Instance != null ? GameProgressManager.Instance.HighestUnlockedLevel : 0;
                highestUnlocked = Mathf.Clamp(highestUnlocked, 0, spawnedNodes.Count - 1);
                
                Vector2 targetPos = highestUnlocked < levelPositions.Length ? levelPositions[highestUnlocked] : new Vector2(0, 0);
                
                // Use screen dimensions or hardcoded values for approximate centering
                float xScroll = Mathf.Max(0, targetPos.x - 1920f / 2f);
                float yScroll = Mathf.Max(0, targetPos.y - 1080f / 2f);
                scrollView.scrollOffset = new Vector2(xScroll, yScroll);
            }

            yield return new WaitForSeconds(0.2f);

            foreach (var node in spawnedNodes)
            {
                node.AddToClassList("show");
                yield return new WaitForSeconds(0.15f);
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