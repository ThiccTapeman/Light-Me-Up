using UnityEditor;
using UnityEngine;

namespace DiasGames.ClimbingSystem.Editors
{
    [CustomEditor(typeof(Ledge))]
    public class LedgeInspector : Editor
    {
        private GameObject _startGrabPoint;
        private Vector3 _step;
        private int _amount;
        private bool _foldout;
        private string _error = string.Empty;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout, "Create Grab Points");

            if (_foldout)
            {
                EditorGUI.indentLevel++;
                
                _startGrabPoint = (GameObject)EditorGUILayout.ObjectField("Start Grab Point", _startGrabPoint, 
                    typeof(GameObject), true);
                _step = EditorGUILayout.Vector3Field("Distance between points", _step);
                _amount = EditorGUILayout.IntField("Number of Points", _amount);

                if (GUILayout.Button("Create Grab Points"))
                {
                    _error = string.Empty;
                    CreateGrabPoints();
                }

                if (!string.IsNullOrEmpty(_error))
                {
                    EditorGUILayout.HelpBox(_error, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void CreateGrabPoints()
        {
            if (_startGrabPoint == null)
            {
                ShowError("No Grab Point defined!");
                return;
            }

            if (_step == Vector3.zero)
            {
                ShowError("No valid distance. Must be non-zero values");
                return;
            }

            Vector3 startPos = _startGrabPoint.transform.localPosition;
            Quaternion rotation = _startGrabPoint.transform.localRotation;
            GameObject targetObject = (serializedObject.targetObject as Ledge).gameObject;
            if (targetObject == null)
            {
                return;
            }

            int children = targetObject.transform.childCount;
            for (int i = 1; i < _amount; i++)
            {
                if (i < children)
                {
                    Transform transform = targetObject.transform.GetChild(i);
                    transform.localPosition = startPos + _step * i;
                    transform.localRotation = rotation;
                    continue;
                }

                GameObject go = new GameObject($"Grab Point {i}");
                go.transform.SetParent(targetObject.transform);
                go.transform.localPosition = startPos + _step * i;
                go.transform.localRotation = rotation;
            }
        }

        private void ShowError(string error)
        {
            _error = error;
        }
    }
}