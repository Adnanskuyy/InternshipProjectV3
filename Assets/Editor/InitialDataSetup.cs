using UnityEngine;
using UnityEditor;
using InvestigationGame.Data;
using System.IO;

public class InitialDataSetup
{
    [MenuItem("Tools/Generate Default Suspects")]
    public static void GenerateSuspects()
    {
        string path = "Assets/Data/Instances";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Instances");
        }

        string[] names = { "John Doe", "Jane Smith", "Bob Bobbington", "Alice Liddell" };

        for (int i = 0; i < names.Length; i++)
        {
            SuspectData newSuspect = ScriptableObject.CreateInstance<SuspectData>();
            newSuspect.SuspectName = names[i];
            
            newSuspect.PhysicalCharacteristics = new EvidenceData { Description = "Looks suspicious. Sweating profusely." };
            newSuspect.Behavior = new EvidenceData { Description = "Answers questions quickly, avoids eye contact." };
            newSuspect.Rumors = "Some people say they saw him near the crime scene.";
            newSuspect.IsUser = (i % 2 == 0); // Alternate true/false
            
            string assetPath = $"{path}/Suspect_{i + 1}.asset";
            AssetDatabase.CreateAsset(newSuspect, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated default suspects at " + path);
    }
}
