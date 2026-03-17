using UnityEngine;
using UnityEditor;

public class WaterBackgroundSetup
{
    [MenuItem("Tools/Setup Water Background")]
    public static void SetupBackground()
    {
        // Check if it already exists to avoid duplicates
        if (GameObject.Find("WaterBackground") != null)
        {
            Debug.Log("WaterBackground already exists in the scene.");
            return;
        }

        var bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgObj.name = "WaterBackground";
        
        // Remove the mesh collider so it doesn't interfere with anything
        GameObject.DestroyImmediate(bgObj.GetComponent<Collider>());

        // Position it far back and scale it to cover the screen
        bgObj.transform.position = new Vector3(0, 0, 90f);
        
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float height = cam.orthographicSize * 2.0f;
            float width = height * cam.aspect;
            bgObj.transform.localScale = new Vector3(width, height, 1f);
        }
        else
        {
            // Fallback scale if main camera is not found or not orthographic
            bgObj.transform.localScale = new Vector3(40f, 20f, 1f);
        }

        // Assign the custom shader material
        var renderer = bgObj.GetComponent<MeshRenderer>();
        Shader waterShader = Shader.Find("Custom/BlueWater");
        if (waterShader != null)
        {
            // Create the material and save it as an asset if it doesn't exist,
            // or just assign a new instance.
            renderer.sharedMaterial = new Material(waterShader);
            renderer.sharedMaterial.name = "BlueWaterMaterial";
        }
        else
        {
            Debug.LogWarning("Shader Custom/BlueWater not found!");
        }

        // Register the creation so it can be undone
        Undo.RegisterCreatedObjectUndo(bgObj, "Create Water Background");
    }
}
