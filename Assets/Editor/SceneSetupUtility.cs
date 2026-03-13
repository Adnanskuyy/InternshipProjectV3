using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using InvestigationGame.Data;
using InvestigationGame.Core;
using InvestigationGame.UI;

public class SceneSetupUtility
{
    [MenuItem("Tools/Setup Investigation Scene")]
    public static void SetupScene()
    {
        // 1. Ensure we have the UXML and USS assets
        var mainScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Views/MainScreen.uxml");
        var suspectTemplateUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Views/SuspectTemplate.uxml");
        
        if (mainScreenUXML == null || suspectTemplateUXML == null)
        {
            Debug.LogError("UXML files not found. Ensure MainScreen.uxml and SuspectTemplate.uxml exist in Assets/UI/Views/");
            return;
        }

        // 2. Create the Managers GameObject
        GameObject managersGo = new GameObject("GameManagers");
        
        var investigationManager = managersGo.AddComponent<InvestigationManager>();
        var uiController = managersGo.AddComponent<GameUIController>();

        // 3. Setup UIDocument
        GameObject uiGo = new GameObject("UIDocument");
        var uiDocument = uiGo.AddComponent<UIDocument>();
        uiDocument.visualTreeAsset = mainScreenUXML;
        
        // Setup Panel Settings to scale with screen size for WebGL
        var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/Views/PanelSettings.asset");
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.match = 0.5f; // Match width and height equally
            AssetDatabase.CreateAsset(panelSettings, "Assets/UI/Views/PanelSettings.asset");
        }
        uiDocument.panelSettings = panelSettings;

        // 4. Generate some placeholder SuspectData if none exist
        var suspects = GeneratePlaceholderSuspects();
        
        // 5. Connect UI Controller
        uiController.InitializeUI(suspects);
        
        // 6. Save Scene
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/InvestigationScene.unity");
        Debug.Log("Scene Setup Complete! Saved as InvestigationScene.unity");
    }

    private static List<SuspectData> GeneratePlaceholderSuspects()
    {
        List<SuspectData> generated = new List<SuspectData>();
        string path = "Assets/Data/Instances";
        
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Instances");
        }

        string[] names = { "Alice", "Bob", "Charlie", "Diana" };
        
        for (int i = 0; i < 4; i++)
        {
            string assetPath = $"{path}/Suspect_{names[i]}.asset";
            SuspectData existing = AssetDatabase.LoadAssetAtPath<SuspectData>(assetPath);
            
            if (existing != null)
            {
                generated.Add(existing);
                continue;
            }

            SuspectData newData = ScriptableObject.CreateInstance<SuspectData>();
            newData.SuspectName = names[i];
            newData.Rumors = $"I heard {names[i]} has been acting very strange lately...";
            newData.Role = (i == 1) ? SuspectRole.Pengguna : SuspectRole.OrangBiasa; // Bob is the culprit
            newData.UrineTestResult = (i == 1); // Only the user has a positive urine test here

            newData.PhysicalCharacteristics = new EvidenceData { Description = "Dilated pupils, jittery movements." };            newData.Behavior = new EvidenceData { Description = "Arrived late to work, avoiding eye contact." };
            
            AssetDatabase.CreateAsset(newData, assetPath);
            generated.Add(newData);
        }
        
        AssetDatabase.SaveAssets();
        return generated;
    }
}