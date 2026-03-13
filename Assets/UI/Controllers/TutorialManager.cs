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

        private VisualElement blockerTop;
        private VisualElement blockerBottom;
        private VisualElement blockerLeft;
        private VisualElement blockerRight;

        private GameUIController mainController;
        private VisualElement root;

        private int currentStep = -1;
        private bool isTutorialActive = false;

        private const string TUTORIAL_PREF_KEY = "TutorialCompleted_v1";

        public TutorialManager(VisualElement root, GameUIController controller)
        {
            this.mainController = controller;
            SetRoot(root);
        }

        public void SetRoot(VisualElement newRoot)
        {
            // Unregister from old elements if any
            Cleanup();

            this.root = newRoot;

            tutorialOverlay = root.Q<VisualElement>("TutorialOverlay");
            highlightBox = root.Q<VisualElement>("TutorialHighlightBox");
            dialogPanel = root.Q<VisualElement>("TutorialDialogPanel");

            titleLabel = dialogPanel?.Q<Label>("TutorialTitle");
            textLabel = dialogPanel?.Q<Label>("TutorialText");
            nextBtn = dialogPanel?.Q<Button>("TutorialNextBtn");

            if (nextBtn != null)
            {
                nextBtn.RegisterCallback<ClickEvent>(OnNextBtnClick);
            }

            if (tutorialOverlay != null)
            {
                // Create blockers if they don't exist
                if (blockerTop == null)
                {
                    blockerTop = CreateBlocker();
                    blockerBottom = CreateBlocker();
                    blockerLeft = CreateBlocker();
                    blockerRight = CreateBlocker();
                    
                    // Insert before highlightBox so dialog and highlight sit on top
                    tutorialOverlay.Insert(0, blockerRight);
                    tutorialOverlay.Insert(0, blockerLeft);
                    tutorialOverlay.Insert(0, blockerBottom);
                    tutorialOverlay.Insert(0, blockerTop);
                }

                // Make overlay transparent and allow clicks through to holes
                tutorialOverlay.style.backgroundColor = new StyleColor(Color.clear);
                tutorialOverlay.pickingMode = PickingMode.Ignore;
            }

            if (highlightBox != null) highlightBox.pickingMode = PickingMode.Ignore;
            if (dialogPanel != null) dialogPanel.pickingMode = PickingMode.Position;
        }

        private VisualElement CreateBlocker()
        {
            var blocker = new VisualElement();
            blocker.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.6f));
            blocker.style.position = Position.Absolute;
            blocker.pickingMode = PickingMode.Position; // Block clicks outside the hole
            return blocker;
        }

        public void Cleanup()
        {
            if (nextBtn != null)
            {
                nextBtn.UnregisterCallback<ClickEvent>(OnNextBtnClick);
            }
            ClearHighlight();
        }

        private void OnNextBtnClick(ClickEvent evt) => OnNextClicked();

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
        private IVisualElementScheduledItem breathingAnimation;

        private void StartTutorial()
        {
            isTutorialActive = true;
            currentStep = 0;
            if (tutorialOverlay != null)
            {
                tutorialOverlay.style.display = DisplayStyle.Flex;
                tutorialOverlay.pickingMode = PickingMode.Ignore; 
            }

            ShowStep(currentStep);
        }

        private void EndTutorial()
        {
            isTutorialActive = false;
            if (tutorialOverlay != null)
            {
                tutorialOverlay.style.display = DisplayStyle.None;
            }
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
            if (nextBtn != null)
            {
                nextBtn.style.display = DisplayStyle.Flex;
                nextBtn.text = "LANJUT";
            }
            if (dialogPanel != null)
            {
                dialogPanel.style.top = new Length(50, LengthUnit.Percent);
                dialogPanel.style.left = new Length(50, LengthUnit.Percent);
                dialogPanel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent));
            }

            switch (step)
            {
                case 0:
                    titleLabel.text = "SELAMAT DATANG DETEKTIF";
                    textLabel.text = "Tujuan Anda adalah menemukan SATU pengguna narkoba di antara 4 tersangka ini. Hanya satu dari mereka yang bersalah.";
                    break;
                case 1:
                    titleLabel.text = "PARA TERSANGKA";
                    textLabel.text = "Klik pada foto polaroid tersangka untuk meninjau berkas dan bukti mereka.";
                    
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
                    titleLabel.text = "BUKTI-BUKTI";
                    textLabel.text = "Tinjau ciri fisik, perilaku, dan rumor tentang mereka untuk mencari petunjuk.";
                    
                    // Highlight the detail text area
                    var detailPanel = root.Q<VisualElement>("DetailPanel");
                    var detailRight = detailPanel?.Q<VisualElement>(className: "detail-right");
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
                    titleLabel.text = "TES URIN";
                    textLabel.text = "Anda hanya memiliki SATU Tes Urin per permainan untuk mendapatkan bukti yang tak terbantahkan. Gunakan dengan bijak!";
                    
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
                    titleLabel.text = "KEPUTUSAN";
                    textLabel.text = "Anda harus menentukan peran setiap tersangka sebagai 'Pengedar', 'Pengguna', atau 'Orang Biasa'. Tandai keputusan sekarang untuk melanjutkan.";
                    
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
                    var dp = root.Q<VisualElement>("DetailPanel");
                    if (dp != null) dp.style.display = DisplayStyle.None;

                    titleLabel.text = "MENGIRIM LAPORAN";
                    textLabel.text = "Setelah semua orang memiliki keputusan, kirim laporan Anda untuk menyelesaikan penyelidikan.";
                    
                    var finalSubmitBtn = root.Q<Button>("FinalSubmitBtn");
                    if (finalSubmitBtn != null)
                    {
                        HighlightElement(finalSubmitBtn);
                        dialogPanel.style.top = 200;
                    }
                    nextBtn.text = "MENGERTI";
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

                // Start breathing animation
                StartBreathingAnimation();

                // Attach GeometryChangedEvent
                currentHighlightTarget.RegisterCallback<GeometryChangedEvent>(OnTargetGeometryChanged);
            });
        }

        private void UpdateHighlightBounds()
        {
            if (currentHighlightTarget == null || highlightBox == null) 
            {
                if (blockerTop != null)
                {
                    blockerTop.style.width = new Length(100, LengthUnit.Percent);
                    blockerTop.style.height = new Length(100, LengthUnit.Percent);
                    blockerTop.style.left = 0;
                    blockerTop.style.top = 0;

                    blockerBottom.style.display = DisplayStyle.None;
                    blockerLeft.style.display = DisplayStyle.None;
                    blockerRight.style.display = DisplayStyle.None;
                }
                return;
            }
            
            var worldBound = currentHighlightTarget.worldBound;
            
            // Use WorldToLocal to ensure correct positioning within the overlay
            var localPos = tutorialOverlay.WorldToLocal(worldBound.position);
            
            float left = localPos.x - 10;
            float top = localPos.y - 10;
            float width = worldBound.width + 20;
            float height = worldBound.height + 20;

            highlightBox.style.left = left;
            highlightBox.style.top = top;
            highlightBox.style.width = width;
            highlightBox.style.height = height;

            if (blockerTop != null)
            {
                blockerBottom.style.display = DisplayStyle.Flex;
                blockerLeft.style.display = DisplayStyle.Flex;
                blockerRight.style.display = DisplayStyle.Flex;

                // Top blocker
                blockerTop.style.left = 0;
                blockerTop.style.top = 0;
                blockerTop.style.width = new Length(100, LengthUnit.Percent);
                blockerTop.style.height = top;

                // Bottom blocker
                blockerBottom.style.left = 0;
                blockerBottom.style.top = top + height;
                blockerBottom.style.width = new Length(100, LengthUnit.Percent);
                blockerBottom.style.height = new Length(100, LengthUnit.Percent);

                // Left blocker
                blockerLeft.style.left = 0;
                blockerLeft.style.top = top;
                blockerLeft.style.width = left;
                blockerLeft.style.height = height;

                // Right blocker
                blockerRight.style.left = left + width;
                blockerRight.style.top = top;
                blockerRight.style.width = new Length(100, LengthUnit.Percent);
                blockerRight.style.height = height;
            }
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
            }).Every(500);
        }

        private void ClearHighlight()
        {
            if (breathingAnimation != null)
            {
                breathingAnimation.Pause();
                breathingAnimation = null;
                highlightBox.style.scale = new StyleScale(new Scale(Vector3.one));
            }

            if (highlightBox != null)
            {
                highlightBox.style.display = DisplayStyle.None;
            }

            if (currentHighlightTarget != null)
            {
                currentHighlightTarget.UnregisterCallback<GeometryChangedEvent>(OnTargetGeometryChanged);
            }

            currentHighlightTarget = null;
            UpdateHighlightBounds(); // Resets blockers to cover full screen
        }

        public void NotifySuspectClicked()
        {
            if (isTutorialActive && currentStep == 1)
            {
                currentStep++;
                ShowStep(currentStep);
            }
        }

        public void NotifyVerdictSubmitted()
        {
            if (isTutorialActive && currentStep == 4)
            {
                currentStep++;
                ShowStep(currentStep);
            }
        }
    }
}
