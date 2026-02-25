using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DiasGames.AbilitySystem.Core;

namespace DiasGames.AbilitySystem.Editor
{
    public class AbilityTagSettingsProvider : SettingsProvider
    {
        private SerializedObject m_CustomSettings;
        private bool _createTagFoldout;
        private string _newAbilityTagName;
        private string _error = string.Empty;

        const string k_AbilityTagSettingsPath = "Assets/Dias Games/Scripts/DiasGames.Editor/AbilityTagSettings.asset";

        class TagDrawer
        {
            public string CompositeName;
            public string Name;
            public List<TagDrawer> Children = new List<TagDrawer>(10);
        }

        private List<TagDrawer> _tagDrawers = new List<TagDrawer>(100);
        private Dictionary<TagDrawer, bool> _drawersFoldout = new Dictionary<TagDrawer, bool>(20);

        public AbilityTagSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {
        }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(k_AbilityTagSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the AbilityTag element in the Settings window.
            m_CustomSettings = AbilityTagSettings.GetSerializedSettings();
            _tagDrawers.Clear();
            _drawersFoldout.Clear();
        }

        public override void OnGUI(string searchContext)
        {
            SerializedProperty tagsProperty = m_CustomSettings.FindProperty("_tags");

            DrawCreateTagFoldout(tagsProperty);
            
            EditorGUILayout.Space();

            FillTagsHierarchyAndDraw(tagsProperty);

            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void DrawCreateTagFoldout(SerializedProperty tagsProperty)
        {
            _createTagFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_createTagFoldout, "Create Ability Tags");

            if (_createTagFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox("To create a parent-child hierarchy in your tags, use a dot (.) to separate levels. For example, 'X.Y.Z' indicates that 'Z' is a child of 'Y', and 'Y' is a child of 'X'.", MessageType.Info);
                
                _newAbilityTagName = EditorGUILayout.TextField("Ability Tag Name", _newAbilityTagName);

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Tag"))
                {
                    if (CreateNewTag(_newAbilityTagName))
                    {
                        _error = string.Empty;
                        _newAbilityTagName = string.Empty;
                    }
                    else
                    {
                        _error = $"Tag not created! {_newAbilityTagName} already exists";
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(_error))
                {
                    EditorGUILayout.HelpBox(_error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public bool CreateNewTag(string name)
        {
            SerializedProperty tagsProperty = m_CustomSettings.FindProperty("_tags");

            if (!IsNameValid(tagsProperty, name)) return false;

            tagsProperty.arraySize++;
            SerializedProperty newTag = tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1);
            newTag.FindPropertyRelative("_name").stringValue = name;

            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            SortAlphabetically(tagsProperty);
            return true;
        }

        private void FillTagsHierarchyAndDraw(SerializedProperty tagsProperty)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("ABILITY TAGS", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                SerializedProperty abilityTag = tagsProperty.GetArrayElementAtIndex(i);
                SerializedProperty abilityTagName = abilityTag.FindPropertyRelative("_name");

                FillDrawers(abilityTagName);
            }
            
            DrawAllTags(_tagDrawers);

            EditorGUILayout.EndVertical();
        }

        private void DrawAllTags(List<TagDrawer> drawers)
        {
            for (int i = 0; i < drawers.Count; i++)
            {
                TagDrawer tagDrawer = drawers[i];
                if (tagDrawer.Children.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(tagDrawer.Name);
                    if (GUILayout.Button("x", EditorStyles.miniButton))
                    {
                        if (DeleteTag(tagDrawer.CompositeName))
                        {
                            _tagDrawers.Clear();
                            _drawersFoldout.Clear();
                            break;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    _drawersFoldout.TryAdd(tagDrawer, false);
                    _drawersFoldout[tagDrawer] = EditorGUILayout.Foldout(_drawersFoldout[tagDrawer], tagDrawer.Name, EditorStyles.foldoutHeader);
                    if (_drawersFoldout[tagDrawer])
                    {
                        EditorGUI.indentLevel++;
                        DrawAllTags(tagDrawer.Children);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void FillDrawers(SerializedProperty abilityTagName)
        {
            string[] splittedNames = abilityTagName.stringValue.Split('.');
            int k = 0;
            List<TagDrawer> drawersList = _tagDrawers;
            while (k < splittedNames.Length)
            {
                TagDrawer drawer = drawersList.Find(x => x.Name == splittedNames[k]);
                if (drawer == null)
                {
                    drawer = new TagDrawer
                    {
                        Name = splittedNames[k],
                        CompositeName = abilityTagName.stringValue
                    };
                    drawersList.Add(drawer);
                }

                k++;
                drawersList = drawer.Children;
            }
        }

        public bool DeleteTag(string name)
        {
            SerializedProperty tagsProperty = m_CustomSettings.FindProperty("_tags");
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue == name)
                {
                    tagsProperty.DeleteArrayElementAtIndex(i);
                    m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                    SortAlphabetically(tagsProperty);
                    return true;
                }
            }

            return false;
        }

        private bool IsNameValid(SerializedProperty tagsProperty, string name)
        {
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (name == tagsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void SortAlphabetically(SerializedProperty tagsList)
        {
            int arraySize = tagsList.arraySize;
            List<AbilityTag> tempAbilityTagsList = new List<AbilityTag>(arraySize);

            for (int i = 0; i < arraySize; i++)
            {
                string name = tagsList.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue;
                tempAbilityTagsList.Add(new AbilityTag(name));
            }

            tempAbilityTagsList.Sort((x,y) => x.Name.CompareTo(y.Name));
            for (int i = 0; i < arraySize; i++)
            {
                tagsList.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue = tempAbilityTagsList[i].Name;
            }

            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            _tagDrawers.Clear();
            _drawersFoldout.Clear();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateAbilityTagSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider =
                    new AbilityTagSettingsProvider("Project/Ability Tags", SettingsScope.Project);

                // Automatically extract all keywords from the Styles.
                //provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}