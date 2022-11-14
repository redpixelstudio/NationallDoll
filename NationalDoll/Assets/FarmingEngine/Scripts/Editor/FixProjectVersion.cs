using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace FarmingEngine.EditorTool
{
    /// <summary>
    /// Check if can apply any automatic fixes to issues that could be caused from changing asset version
    /// </summary>

    public class FixProjectVersion : ScriptableWizard
    {
        [MenuItem("Farming Engine/Fix Project Version", priority = 400)]
        static void ScriptableWizardMenu()
        {
            ScriptableWizard.DisplayWizard<FixProjectVersion>("Fix Project Version", "Fix");
        }

        void OnWizardCreate()
        {
            string[] allPrefabs = GetAllPrefabs();
            foreach (string prefab_path in allPrefabs)
            {
                GameObject prefab = (GameObject) AssetDatabase.LoadMainAssetAtPath(prefab_path);
                if (prefab != null)
                {
                    
                }
            }

            AssetDatabase.SaveAssets();
        }

        public static string[] GetAllPrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (s.Contains(".prefab")) result.Add(s);
            }
            return result.ToArray();
        }

        void OnWizardUpdate()
        {
            helpString = "Use this tool after updating Farming Engine version, to fix any prefabs that should be updated to match the new version.";
        }
    }
}