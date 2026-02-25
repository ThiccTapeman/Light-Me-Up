using UnityEditor;
using UnityEngine;

namespace DiasGames.Editors
{
    public static class Utilities
    {
        /// <summary>
        /// Creates a new script based on template
        /// </summary>
        /// <param name="templateName">The name of the template without extension</param>
        /// <param name="defaultFileName">The name of the file to be created with file extension</param>
        public static void CreateNewScript(string templateName, string defaultFileName)
        {
            // Load object
            Object obj = AssetDatabase.LoadAssetAtPath("Assets", typeof(Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);

            string[] templateGUID = AssetDatabase.FindAssets(templateName);

            if (templateGUID.Length <= 0) return;

            var templatePath = AssetDatabase.GUIDToAssetPath(templateGUID[0]);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultFileName);
        }
    }
}