using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DiasGames.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(UnityTagAttribute))]
    public class UnityTagSelectionDrawer : PropertyDrawer
    {
        private List<string> _options;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProperty = tagManager.FindProperty("tags");

            _options = new List<string>(tagsProperty.arraySize)
            {
                "Empty",
                "Untagged"
            };

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                _options.Add(tagsProperty.GetArrayElementAtIndex(i).stringValue);
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                HandleSingle(position, property, label);
            }
            else
            {
                HandleCollection(position, property, label);
            }

            EditorGUI.EndProperty();
        }

        private void HandleCollection(Rect position, SerializedProperty property, GUIContent label)
        {
            int currentSelected = ConvertSelectedTagsToInt(property);
            
            int newSelection = EditorGUI.MaskField(position, label, currentSelected, _options.ToArray());
            if (newSelection != currentSelected)
            {
                property.ClearArray();
                for (int i = 0; i < _options.Count; i++)
                {
                    int pow = (int)Mathf.Pow(2, i);
                    if ((newSelection & pow) == pow)
                    {
                        property.arraySize++;
                        property.GetArrayElementAtIndex(property.arraySize - 1).stringValue = _options[i];
                    }
                }
            }
        }

        private int ConvertSelectedTagsToInt(SerializedProperty property)
        {
            int sum = 0;
            for (int i = 0; i < property.arraySize; i++)
            {
                var selectedTag = property.GetArrayElementAtIndex(i).stringValue;
                bool found = false;
                for (int j = 0; j < _options.Count; j++)
                {
                    var currentSystemTag = _options[j];
                    if (selectedTag.Equals(currentSystemTag))
                    {
                        sum += (int)Mathf.Pow(2, j);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    property.DeleteArrayElementAtIndex(i);
                    return ConvertSelectedTagsToInt(property);
                }
            }

            return sum;
        }

        private void HandleSingle(Rect position, SerializedProperty property, GUIContent label)
        {
            int currentSelected = GetCurrentSelected(property);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int newSelection = EditorGUI.Popup(position, currentSelected, _options.ToArray());
            if (newSelection != currentSelected)
            {
                property.stringValue = _options[newSelection];
            }
        }

        private int GetCurrentSelected(SerializedProperty property)
        {
            string tagName = property.stringValue;
            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i].Equals(tagName))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}