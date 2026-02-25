using System.Collections.Generic;
using System.Linq;
using DiasGames.AbilitySystem.Core;
using UnityEditor;
using UnityEngine;

namespace DiasGames.AbilitySystem.Editor
{
    public class SelectionPopup : PopupWindowContent
    {
        private Vector2 scrollPosition;
        private string[] optionsArray;
        private SerializedProperty _property;

        public SelectionPopup(string[] optionsArray, SerializedProperty property)
        {
            this.optionsArray = optionsArray;
            _property = property;
        }

        public override void OnGUI(Rect rect)
        {
            var settings = AbilityTagSettings.GetOrCreateSettings();
            var tagsProperty = _property.FindPropertyRelative("_tags");
            
            List<string> selectedOptions = new List<string>(10);
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                selectedOptions.Add(tagsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            if (GUILayout.Button("Clear"))
            {
                tagsProperty.ClearArray();
            }

            for (int i = 0; i < optionsArray.Length; i++)
            {
                bool selected = selectedOptions.Contains(optionsArray[i]);
                bool selectionNew = EditorGUILayout.ToggleLeft(optionsArray[i], selected);

                if (selectionNew && !selected)
                {
                    AbilityTag tag = settings.Tags[i];
                        
                    tagsProperty.arraySize++;
                    SerializedProperty newTag = tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1);
                    newTag.FindPropertyRelative("_name").stringValue = tag.Name;
                }
                else if (!selectionNew && selected)
                {
                    for (int j = 0; j < tagsProperty.arraySize; j++)
                    {
                        string tagName = tagsProperty.GetArrayElementAtIndex(j).FindPropertyRelative("_name")
                            .stringValue;
                        if (tagName == optionsArray[i])
                        {
                            int arraySize = tagsProperty.arraySize;
                            tagsProperty.DeleteArrayElementAtIndex(j);
                            if (arraySize == tagsProperty.arraySize)
                            {
                                tagsProperty.DeleteArrayElementAtIndex(j);
                            }
                            break;
                        }
                    }
                }
            }
            _property.serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }
    }
    
    [CustomPropertyDrawer(typeof(AbilityTagContainer))]
    public class AbilityTagContainerDrawer : PropertyDrawer
    {
        private Vector2 scrollPosition;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var settings = AbilityTagSettings.GetOrCreateSettings();

            List<string> options = settings.Tags.ToList().ConvertAll(x => x.Name);
            var tagsProperty = property.FindPropertyRelative("_tags");

            string dropdownLabel = tagsProperty.arraySize == 0 ? "None" : string.Empty;
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                dropdownLabel += tagsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
                if (i < tagsProperty.arraySize - 1)
                {
                    dropdownLabel += ", ";
                }
            }

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(label, GUILayout.Width(150));
            
            // Reserve space in layout and get the rect for the button
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Show Popup"), GUI.skin.button);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(dropdownLabel), FocusType.Passive))
            {
                // Add local rect position to window position to get screen-space rect
                Rect screenRect = new Rect(
                    buttonRect.x,
                    buttonRect.y,
                    buttonRect.width,
                    buttonRect.height
                );

                PopupWindow.Show(screenRect, new SelectionPopup(options.ToArray(), property));
            }
            EditorGUILayout.EndHorizontal();
            
            DrawTagsSelected(property);

            EditorGUI.EndProperty();
        }

        private void DrawTagsSelected(SerializedProperty property)
        {
            var tagsList = property.FindPropertyRelative("_tags");
            string names = string.Empty;
            for (int i = 0; i < tagsList.arraySize; i++)
            {
                names += tagsList.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
                if (i + 1 < tagsList.arraySize)
                {
                    names += ", ";
                }
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            labelStyle.wordWrap = true;
            EditorGUILayout.LabelField(names, labelStyle);

            EditorGUILayout.EndHorizontal();
        }
    }
}