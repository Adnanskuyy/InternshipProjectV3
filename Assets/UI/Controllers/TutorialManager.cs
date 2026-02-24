using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using InvestigationGame.Core;
using InvestigationGame.Data;

namespace InvestigationGame.UI
{
    public class TutorialManager
    {
        private VisualElement tutorialOverlay;
        private VisualElement highlightBox;
        private VisualElement dialogPanel;
        private Label titleLabel;
        private Label textLabel;
        private Button nextBtn;

        private GameUIController mainController;
        private VisualElement root;

        private int currentStep = -1;
        private bool isTutorialActive = false;

        private const string TUTORIAL_PREF_KEY = "TutorialCompleted_v1";

        public TutorialManager(VisualElement root, GameUIController controller)
        {
            this.root = root;
            this.mainController = controller;

            tutorialOverlay = root.Q<VisualElement>("TutorialOverlay");
            highlightBox = root.Q<VisualElement>("TutorialHighlightBox");
            dialogPanel = root.Q<VisualElement>("TutorialDialogPanel");

            titleLabel = dialogPanel?.Q<Label>("TutorialTitle");
            textLabel = dialogPanel?.Q<Label>("TutorialText");
            nextBtn = dialogPanel?.Q<Button>("TutorialNextBtn");

            if (nextBtn != null)
            {
                nextBtn.clicked += OnNextClicked;
            }
        }

        public void TryStartTutorial(bool forceStart = false)
        {
            if (tutorialOverlay == null) return;

            bool hasCompleted = PlayerPrefs.GetInt(TUTORIAL_PREF_KEY, 0) == 1;

            if (!hasCompleted || forceStart)
            {
                StartTutorial();
            }
        }

        private VisualElement currentHighlightTarget;
        private VisualElement originalParent;
        private int originalSiblingIndex;
        private IVisualElementScheduledItem breathingAnimation;

        private void StartTutorial()
        {
            isTutorialActive = true;
            currentStep = 0;
            tutorialOverlay.style.display = DisplayStyle.Flex;
            
            // Set overlay to block clicks
            if (tutorialOverlay != null)
            {
                tutorialOverlay.pickingMode = PickingMode.Position; 
            }

            ShowStep(currentStep);
        }

        private void EndTutorial()
        {
            isTutorialActive = false;
            tutorialOverlay.style.display = DisplayStyle.None;
            ClearHighlight();
            PlayerPrefs.SetInt(TUTORIAL_PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        private void OnNextClicked()
        {
            currentStep++;
            ShowStep(currentStep);
        }

        private void ShowStep(int step)
        {
            // Reset Highlight and Next Button
            ClearHighlight();
            nextBtn.style.display = DisplayStyle.Flex;
            nextBtn.text = "NEXT";
            dialogPanel.style.top = new Length(50, LengthUnit.Percent);
            dialogPanel.style.left = new Length(50, LengthUnit.Percent);
            dialogPanel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent));

            switch (step)
            {
                case 0:
                    titleLabel.text = "WELCOME DETECTIVE";
                    textLabel.text = "Your goal is to find exactly ONE drug user among these 4 suspects. Only one of them is guilty.";
                    break;
                case 1:
                    titleLabel.text = "THE SUSPECTS";
                    textLabel.text = "Click on a suspect's polaroid to review their file and evidence.";
                    
                    // Highlight the first suspect in the grid
                    var polaroidGrid = root.Q<VisualElement>("PolaroidGrid");
                    if (polaroidGrid != null && polaroidGrid.childCount > 0)
                    {
                        var firstSuspect = polaroidGrid[0];
                        HighlightElement(firstSuspect);
                        // Move dialog to top to avoid covering suspects
                        dialogPanel.style.top = 100;
                        dialogPanel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);
                    }
                    
                    nextBtn.style.display = DisplayStyle.None; // Wait for player click
                    break;
                case 2:
                    titleLabel.text = "THE EVIDENCE";
                    textLabel.text = "Review their physical characteristics, behaviors, and rumors for clues.";
                    
                    // Highlight the detail text area
                    var detailRight = root.Q<VisualElement>("DetailPanel").Q<VisualElement>(className: "detail-right");
                    var scrollView = detailRight?.Q<ScrollView>();
                    if (scrollView != null)
                    {
                        HighlightElement(scrollView);
                        // Move dialog to the left
                        dialogPanel.style.top = new Length(50, LengthUnit.Percent);
                        dialogPanel.style.left = 250;
                        dialogPanel.style.translate = new Translate(0, new Length(-50, LengthUnit.Percent));
                    }
                    break;
                case 3:
                    titleLabel.text = "THE URINE TEST";
                    textLabel.text = "You have ONE Urine Test per game to get undeniable proof. Use it wisely!";
                    
                    var urineTestBtn = root.Q<Button>("UrineTestBtn");
                    if (urineTestBtn != null)
                    {
                        HighlightElement(urineTestBtn);
                        dialogPanel.style.top = new Length(50, LengthUnit.Percent);
                        dialogPanel.style.left = 250;
                        dialogPanel.style.translate = new Translate(0, new Length(-50, LengthUnit.Percent));
                    }
                    break;
                case 4:
                    titleLabel.text = "THE VERDICT";
                    textLabel.text = "You must mark the real user as 'Positive' and everyone else as 'Negative'. Mark a verdict now to continue.";
                    
                    // Highlight the verdict buttons
                    var actionBar = root.Q<VisualElement>(className: "action-bar");
                    if (actionBar != null)
                    {
                        HighlightElement(actionBar);
                        dialogPanel.style.top = new Length(20, LengthUnit.Percent);
                        dialogPanel.style.left = new Length(50, LengthUnit.Percent);
                        dialogPanel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);
                    }
                    nextBtn.style.display = DisplayStyle.None; // Wait for verdict click
                    break;
                case 5:
                    // Hide the detail panel automatically to show the final submit button
                    root.Q<VisualElement>("DetailPanel").style.display = DisplayStyle.None;

                    titleLabel.text = "SUBMITTING THE REPORT";
                    textLabel.text = "Once everyone has a verdict (Positive or Negative), submit your report to finish the investigation.";
                    
                    var finalSubmitBtn = root.Q<Button>("FinalSubmitBtn");
                    if (finalSubmitBtn != null)
                    {
                        HighlightElement(finalSubmitBtn);
                        dialogPanel.style.top = 200;
                    }
                    nextBtn.text = "GOT IT";
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }

        private void HighlightElement(VisualElement target)
        {
            if (target == null || highlightBox == null) return;

            ClearHighlight();
            currentHighlightTarget = target;

            // Use scheduling to ensure layout is calculated before positioning
            target.schedule.Execute(() =>
            {
                UpdateHighlightBounds();
                highlightBox.style.display = DisplayStyle.Flex;
                
                // Add juice glow class
                highlightBox.ClearClassList();
                highlightBox.AddToClassList("tutorial-highlight-box"); // Base class
                highlightBox.AddToClassList("tutorial-glow");

                // Start breathing animation (Pulse scale from 1.0 to 1.05)
                StartBreathingAnimation();

                // Attach GeometryChangedEvent to track target movement in real-time
                currentHighlightTarget.RegisterCallback<GeometryChangedEvent>(OnTargetGeometryChanged);

                // Move the target into the TutorialOverlay so it renders ON TOP of the blocking layer
                // and correctly receives pointer events.
                originalParent = currentHighlightTarget.parent;
                if (originalParent != null)
                {
                    originalSiblingIndex = originalParent.IndexOf(currentHighlightTarget);
                    
                    // Detach from current parent
                    currentHighlightTarget.RemoveFromHierarchy();
                    
                    // Add to overlay
                    tutorialOverlay.Add(currentHighlightTarget);
                    
                    // IMPORTANT: We must manually position it absolutely since it's no longer in its original flex layout
                    currentHighlightTarget.style.position = Position.Absolute;
                    currentHighlightTarget.style.left = currentHighlightTarget.worldBound.x;
                    currentHighlightTarget.style.top = currentHighlightTarget.worldBound.y;
                    currentHighlightTarget.style.width = currentHighlightTarget.worldBound.width;
                    currentHighlightTarget.style.height = currentHighlightTarget.worldBound.height;
                }
            });
        }

        private void UpdateHighlightBounds()
        {
            if (currentHighlightTarget == null || highlightBox == null) return;
            var worldBound = currentHighlightTarget.worldBound;
            highlightBox.style.left = worldBound.x - 10;
            highlightBox.style.top = worldBound.y - 10;
            highlightBox.style.width = worldBound.width + 20;
            highlightBox.style.height = worldBound.height + 20;
        }

        private void OnTargetGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateHighlightBounds();
        }

        private void StartBreathingAnimation()
        {
            bool scaleUp = true;
            if (breathingAnimation != null)
            {
                breathingAnimation.Pause();
            }

            // Using the experimental animation API for UI Toolkit
            breathingAnimation = highlightBox.schedule.Execute(() =>
            {
                if (scaleUp)
                {
                    highlightBox.experimental.animation.Scale(1.05f, 500).Ease(Easing.InOutSine);
                }
                else
                {
                    highlightBox.experimental.animation.Scale(1.0f, 500).Ease(Easing.InOutSine);
                }
                scaleUp = !scaleUp;
            }).Every(500); // Loops every 500ms
        }

        private void ClearHighlight()
        {
            if (breathingAnimation != null)
            {
                breathingAnimation.Pause();
                breathingAnimation = null;
                highlightBox.style.scale = new StyleScale(new Scale(Vector3.one)); // Reset scale safely
            }

            if (highlightBox != null)
            {
                highlightBox.style.display = DisplayStyle.None;
            }

            if (currentHighlightTarget != null)
            {
                currentHighlightTarget.UnregisterCallback<GeometryChangedEvent>(OnTargetGeometryChanged);

                if (originalParent != null)
                {
                    // Restore original parent and position
                    currentHighlightTarget.RemoveFromHierarchy();
                    originalParent.Insert(originalSiblingIndex, currentHighlightTarget);
                    
                    // Reset positional overrides
                    currentHighlightTarget.style.position = StyleKeyword.Null;
                    currentHighlightTarget.style.left = StyleKeyword.Null;
                    currentHighlightTarget.style.top = StyleKeyword.Null;
                    currentHighlightTarget.style.width = StyleKeyword.Null;
                    currentHighlightTarget.style.height = StyleKeyword.Null;
                }
            }

            currentHighlightTarget = null;
            originalParent = null;
        }

        // --- Hooks for Game Flow ---

        public void NotifySuspectClicked()
        {
            if (isTutorialActive && currentStep == 1)
            {
                // Advance to evidence
                currentStep++;
                ShowStep(currentStep);
            }
        }

        public void NotifyVerdictSubmitted()
        {
            if (isTutorialActive && currentStep == 4)
            {
                // Advance to final submit
                currentStep++;
                ShowStep(currentStep);
            }
        }
    }
}