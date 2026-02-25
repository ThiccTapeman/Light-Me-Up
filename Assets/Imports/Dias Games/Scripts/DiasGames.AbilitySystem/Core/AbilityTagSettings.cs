using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DiasGames.AbilitySystem.Core
{
    public class AbilityTagSettings : ScriptableObject
    {
        public const string k_AbilityTagSettingsPath = "Assets/Dias Games/Scripts/DiasGames.Editor/AbilityTagSettings.asset";

        [SerializeField] private List<AbilityTag> _tags = new List<AbilityTag>();

        public List<AbilityTag> Tags => _tags;

#if UNITY_EDITOR
        public static AbilityTagSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AbilityTagSettings>(k_AbilityTagSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<AbilityTagSettings>();
                settings._tags = new List<AbilityTag>();
                AssetDatabase.CreateAsset(settings, k_AbilityTagSettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}