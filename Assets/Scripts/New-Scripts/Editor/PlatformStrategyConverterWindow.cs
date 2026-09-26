using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using New_Scripts.Platform;

namespace New_Scripts.Editor
{
    /// <summary>
    /// LinearPingPongStrategy içeren platformları TransformPingPongStrategy'ye dönüştüren sürükle-bırak destekli editör aracı.
    /// </summary>
    public class PlatformStrategyConverterWindow : EditorWindow
    {
        private List<GameObject> _platforms = new List<GameObject>();
        private bool _transferStartEndPoints = true;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Platform Strategy Converter")]
        public static void ShowWindow()
        {
            PlatformStrategyConverterWindow window = GetWindow<PlatformStrategyConverterWindow>("Strategy Converter");
            window.minSize = new Vector2(400, 350);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Platform Strategy Converter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Converts platforms containing LinearPingPongStrategy to TransformPingPongStrategy.", MessageType.Info);
            EditorGUILayout.Space();

            // Options
            _transferStartEndPoints = EditorGUILayout.Toggle("Transfer Start & End Points", _transferStartEndPoints);
            EditorGUILayout.Space();

            // Drag and Drop Area
            DrawDragAndDropArea();
            EditorGUILayout.Space();

            // Selection Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Selected from Hierarchy"))
            {
                LoadFromSelection();
            }
            if (GUILayout.Button("Clear List"))
            {
                _platforms.Clear();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Platforms List
            GUILayout.Label($"Platforms to Convert ({_platforms.Count}):", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(200));
            for (int i = 0; i < _platforms.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _platforms[i] = (GameObject)EditorGUILayout.ObjectField($"Platform {i + 1}", _platforms[i], typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _platforms.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            // Convert Button
            GUI.enabled = _platforms.Count > 0;
            if (GUILayout.Button("Convert Strategies", GUILayout.Height(40)))
            {
                ConvertSelectedPlatforms();
            }
            GUI.enabled = true;
        }

        private void DrawDragAndDropArea()
        {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Platforms Here", EditorStyles.helpBox);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject go)
                            {
                                if (!_platforms.Contains(go))
                                {
                                    _platforms.Add(go);
                                }
                            }
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }

        private void LoadFromSelection()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go.GetComponent<LinearPingPongStrategy>() != null)
                {
                    if (!_platforms.Contains(go))
                    {
                        _platforms.Add(go);
                    }
                }
            }
        }

        private void ConvertSelectedPlatforms()
        {
            int convertedCount = 0;
            
            // Collect all non-null platforms
            List<GameObject> targetPlatforms = new List<GameObject>();
            foreach (var p in _platforms)
            {
                if (p != null && !targetPlatforms.Contains(p))
                {
                    targetPlatforms.Add(p);
                }
            }

            foreach (GameObject platform in targetPlatforms)
            {
                LinearPingPongStrategy linearStrategy = platform.GetComponent<LinearPingPongStrategy>();
                if (linearStrategy == null)
                    continue;

                // 1. Read values from LinearPingPongStrategy
                SerializedObject serializedLinear = new SerializedObject(linearStrategy);
                Vector2 startPt = serializedLinear.FindProperty("startPoint").vector2Value;
                Vector2 endPt = serializedLinear.FindProperty("endPoint").vector2Value;
                float period = serializedLinear.FindProperty("period").floatValue;
                float phaseOffset = serializedLinear.FindProperty("phaseOffset").floatValue;

                // Register Undo for the platform GameObject and parent/siblings context
                Undo.RegisterCompleteObjectUndo(platform, "Convert Platform Strategy");

                GameObject startObj = null;
                GameObject endObj = null;

                // 2. Create target transforms if requested
                if (_transferStartEndPoints)
                {
                    // Create siblings under platform's parent to avoid scale/rotation propagation issues
                    startObj = new GameObject(platform.name + "_Start");
                    Undo.RegisterCreatedObjectUndo(startObj, "Create Start Transform");
                    if (platform.transform.parent != null)
                    {
                        startObj.transform.SetParent(platform.transform.parent);
                    }
                    startObj.transform.position = new Vector3(startPt.x, startPt.y, platform.transform.position.z);

                    endObj = new GameObject(platform.name + "_End");
                    Undo.RegisterCreatedObjectUndo(endObj, "Create End Transform");
                    if (platform.transform.parent != null)
                    {
                        endObj.transform.SetParent(platform.transform.parent);
                    }
                    endObj.transform.position = new Vector3(endPt.x, endPt.y, platform.transform.position.z);
                }

                // 3. Add TransformPingPongStrategy
                TransformPingPongStrategy newStrategy = Undo.AddComponent<TransformPingPongStrategy>(platform);
                
                // Copy properties
                SerializedObject serializedTarget = new SerializedObject(newStrategy);
                serializedTarget.FindProperty("period").floatValue = period;
                serializedTarget.FindProperty("phaseOffset").floatValue = phaseOffset;
                if (_transferStartEndPoints)
                {
                    serializedTarget.FindProperty("startTransform").objectReferenceValue = startObj.transform;
                    serializedTarget.FindProperty("endTransform").objectReferenceValue = endObj.transform;
                }
                serializedTarget.ApplyModifiedProperties();

                // 4. Update references in PlatformController or TriggeredPlatformController
                var platformController = platform.GetComponent<PlatformController>();
                if (platformController != null)
                {
                    SerializedObject serializedCtrl = new SerializedObject(platformController);
                    serializedCtrl.FindProperty("movementStrategy").objectReferenceValue = newStrategy;
                    serializedCtrl.ApplyModifiedProperties();
                    EditorUtility.SetDirty(platformController);
                }

                var triggeredController = platform.GetComponent<TriggeredPlatformController>();
                if (triggeredController != null)
                {
                    SerializedObject serializedTrigger = new SerializedObject(triggeredController);
                    serializedTrigger.FindProperty("movementStrategy").objectReferenceValue = newStrategy;
                    serializedTrigger.ApplyModifiedProperties();
                    EditorUtility.SetDirty(triggeredController);
                }

                // 5. Remove the old strategy component
                Undo.DestroyObjectImmediate(linearStrategy);

                // Mark dirty
                EditorUtility.SetDirty(platform);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(platform.scene);

                convertedCount++;
            }

            // Clear the converted list
            _platforms.Clear();

            EditorUtility.DisplayDialog("Conversion Complete", $"Successfully converted {convertedCount} platform(s) to TransformPingPongStrategy.", "OK");
        }
    }
}
