using System.Collections.Generic;
using System.Linq;
using DiasGames.AbilitySystem.Core;
using UnityEditor;
using UnityEngine;

namespace DiasGames.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(AbilityTag))]
    public class AbilityTagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property); 

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var settings = AbilityTagSettings.GetOrCreateSettings();
            int currentSelected = GetCurrentSelected(property);

            List<string> options = settings.Tags.ToList().ConvertAll(x => x.Name);

            int newSelection = EditorGUI.Popup(position, currentSelected, options.ToArray());
            if (newSelection != currentSelected)
            {
                property.FindPropertyRelative("_name").stringValue = settings.Tags[newSelection].Name;
            }

            EditorGUI.EndProperty();
        }

        private int GetCurrentSelected(SerializedProperty property)
        {
            var settings = AbilityTagSettings.GetOrCreateSettings();
            string tagName = property.FindPropertyRelative("_name").stringValue;
            for (int i = 0; i < settings.Tags.Count; i++)
            {
                if (settings.Tags[i].Name.Equals(tagName))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}