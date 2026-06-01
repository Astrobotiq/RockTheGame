using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using New_Scripts.KinematicActionSystem.Core;

namespace New_Scripts.KinematicActionSystem.Editor
{
    /// <summary>
    /// KinematicActionSystemRunner için [SerializeReference] tabanlı yeni Inspector ve Timeline arayüzü.
    /// </summary>
    [CustomEditor(typeof(KinematicActionSystemRunner))]
    public class KinematicActionSystemRunnerEditor : UnityEditor.Editor
    {
        private struct TransformState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public bool active;
        }

        // Önizleme Ayarları
        private bool _isPreviewing;
        private double _previewStartTime;
        private double _lastTickTime;
        private float _currentPreviewTime;
        private TransformState _savedState;
        private float _maxTimelineTime = 8.0f; // Timeline görünüm genişliği (saniye)

        // Sürükle-Bırak (Timeline) Durumu
        private int _draggingIndex = -1;
        private int _dragMode = 0; // 1: Süre Ayarlama (Sağ kenar), 2: Konum Kaydırma (Orta)
        private float _dragStartMouseX;
        private float _dragStartVal;
        private float _dragStartDur;

        // Katlanabilir Özellik Listesi Görünümü
        private Dictionary<ActionNode, bool> _foldoutStates = new Dictionary<ActionNode, bool>();

        private KinematicActionSystemRunner _runner;

        private void OnEnable()
        {
            _runner = (KinematicActionSystemRunner)target;
        }

        private void OnDisable()
        {
            if (_isPreviewing)
            {
                StopEditorPreview();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. Üst Kontroller (Önizleme Butonları)
            DrawPreviewControls();
            
            GUILayout.Space(10);
            
            // 2. Timeline Zaman Çizelgesi Editörü
            DrawTimelineArea();

            GUILayout.Space(10);

            // 3. Eylem Ekleme Butonları
            DrawAddNodeButtons();

            GUILayout.Space(10);

            // 4. Detaylı Eylem Özellikleri ve Eğri Şablon Butonları
            DrawActionProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPreviewControls()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Label("Edit-Mode Runner (Güvenli Önizleme)", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (!_isPreviewing)
            {
                GUI.backgroundColor = new Color(0.2f, 0.7f, 0.3f);
                if (GUILayout.Button("▶ Play Preview", GUILayout.Height(30)))
                {
                    StartEditorPreview();
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
                if (GUILayout.Button("■ Stop Preview", GUILayout.Height(30)))
                {
                    StopEditorPreview();
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            if (_isPreviewing)
            {
                float totalLength = GetTotalTimelineLength();
                float newPreviewTime = EditorGUILayout.Slider("Zaman (saniye)", _currentPreviewTime, 0f, Mathf.Max(totalLength, 1f));
                if (Mathf.Abs(newPreviewTime - _currentPreviewTime) > 0.001f)
                {
                    _currentPreviewTime = newPreviewTime;
                    _lastTickTime = EditorApplication.timeSinceStartup - _currentPreviewTime; // scrubbing desteği
                    ApplyPreviewStateAt(_currentPreviewTime);
                }
            }
            
            GUILayout.EndVertical();
        }

        private void DrawTimelineArea()
        {
            GUILayout.Label("Timeline Çizelgesi (Sürükle-Bırak Destekli)", EditorStyles.boldLabel);
            
            // Timeline kutusunun boyutunu belirle
            Rect rect = GUILayoutUtility.GetRect(10, 200, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "", GUI.skin.window);

            float labelWidth = 140f;
            float timelineWidth = rect.width - labelWidth - 30f;
            float rowHeight = 28f;

            List<ActionNode> actions = _runner.Actions;

            if (actions == null || actions.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 20, rect.y + 40, rect.width - 40, 30), "Henüz eylem eklenmemiş. Aşağıdan eylem ekleyin.");
                return;
            }

            // Grid Çizgilerini Çiz
            DrawTimelineGrid(rect, labelWidth, timelineWidth);

            Event evt = Event.current;
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null) continue;

                Rect rowRect = new Rect(rect.x, rect.y + 25f + (i * rowHeight), rect.width, rowHeight);
                
                // Pasif eylemleri hafif şeffaf göster
                Color originalContentColor = GUI.contentColor;
                if (!action.isEnabled) GUI.contentColor = new Color(1, 1, 1, 0.4f);

                // Sol Sütun: Eylem Tipi ve İsmi
                string cleanName = action.GetType().Name.Replace("Action", "");
                GUI.Label(new Rect(rowRect.x + 5, rowRect.y + 4, labelWidth - 10, rowHeight - 4), $"{cleanName} ({action.duration:F1}s)", EditorStyles.boldLabel);

                // Sağ Sütun: Zaman Çubuğu
                float startPct = action.startTime / _maxTimelineTime;
                float durPct = action.duration / _maxTimelineTime;

                float barX = rowRect.x + labelWidth + (startPct * timelineWidth);
                float barWidth = Mathf.Max(durPct * timelineWidth, 8f);
                Rect barRect = new Rect(barX, rowRect.y + 5, barWidth, rowHeight - 10);

                // Eylem türüne göre renk seçimi
                Color actionColor = GetActionColor(action);
                if (!action.isEnabled) actionColor.a = 0.3f;

                Color originalGUIColor = GUI.color;
                GUI.color = actionColor;
                GUI.Box(barRect, "", GUI.skin.button);
                GUI.color = originalGUIColor;

                GUI.contentColor = originalContentColor;

                // Mouse etkileşimleri (Resize / Move)
                HandleTimelineMouseInteraction(evt, i, barRect, ref action, timelineWidth);
            }
        }

        private void DrawTimelineGrid(Rect rect, float labelWidth, float timelineWidth)
        {
            Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
            for (int s = 0; s <= _maxTimelineTime; s++)
            {
                float pct = (float)s / _maxTimelineTime;
                float x = rect.x + labelWidth + (pct * timelineWidth);
                
                // Dikey Kılavuz Çizgisi
                Handles.DrawLine(new Vector3(x, rect.y + 20), new Vector3(x, rect.y + rect.height));
                
                // Üst Zaman Yazısı
                GUI.Label(new Rect(x - 8, rect.y + 2, 25, 18), $"{s}s", EditorStyles.miniLabel);
            }
        }

        private void HandleTimelineMouseInteraction(Event evt, int index, Rect barRect, ref ActionNode action, float timelineWidth)
        {
            // Sağ kenar (Resize kolu)
            Rect rightHandleRect = new Rect(barRect.x + barRect.width - 8f, barRect.y, 8f, barRect.height);

            // Cursor ikonunu değiştirme ipucu
            if (rightHandleRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(rightHandleRect, MouseCursor.ResizeHorizontal);
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (barRect.Contains(evt.mousePosition))
                    {
                        _draggingIndex = index;
                        _dragStartMouseX = evt.mousePosition.x;
                        _dragStartVal = action.startTime;
                        _dragStartDur = action.duration;

                        if (rightHandleRect.Contains(evt.mousePosition))
                        {
                            _dragMode = 1; // Süre uzat/kısalt
                        }
                        else
                        {
                            _dragMode = 2; // Sürükle taşı
                        }
                        evt.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (_draggingIndex == index)
                    {
                        float deltaX = evt.mousePosition.x - _dragStartMouseX;
                        float deltaTime = (deltaX / timelineWidth) * _maxTimelineTime;

                        Undo.RecordObject(_runner, "Timeline Değişikliği");

                        if (_dragMode == 1) // Süre Değiştirme
                        {
                            action.duration = Mathf.Max(0.05f, _dragStartDur + deltaTime);
                        }
                        else if (_dragMode == 2) // Başlangıç Zamanı Taşıma
                        {
                            action.startTime = Mathf.Max(0f, _dragStartVal + deltaTime);
                        }

                        // Eğer süre maksimum görünüm sınırını geçerse sınırı büyüt
                        float totalEnd = action.startTime + action.duration;
                        if (totalEnd > _maxTimelineTime)
                        {
                            _maxTimelineTime = Mathf.Ceil(totalEnd + 1f);
                        }

                        EditorUtility.SetDirty(_runner);
                        Repaint();
                        evt.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (_draggingIndex == index)
                    {
                        _draggingIndex = -1;
                        _dragMode = 0;
                        evt.Use();
                    }
                    break;
            }
        }

        private Color GetActionColor(ActionNode action)
        {
            string typeName = action.GetType().Name;
            switch (typeName)
            {
                case "MoveAction": return new Color(0.2f, 0.55f, 0.9f);
                case "SplineMoveAction": return new Color(0.1f, 0.75f, 0.85f);
                case "RotateAction": return new Color(0.95f, 0.6f, 0.15f);
                case "SquashStretchAction": return new Color(0.85f, 0.25f, 0.6f);
                case "VelocityAction": return new Color(0.25f, 0.8f, 0.45f);
                case "ConditionAction": return new Color(0.5f, 0.35f, 0.8f);
                case "HitstopShakeAction": return new Color(0.9f, 0.3f, 0.3f);
                case "ToggleActiveAction": return new Color(0.6f, 0.65f, 0.7f);
                default: return Color.gray;
            }
        }

        private void DrawAddNodeButtons()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Yeni Eylem Ekle", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Move", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.MoveAction>("MoveAction");
            if (GUILayout.Button("+ Spline", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.SplineMoveAction>("SplineMoveAction");
            if (GUILayout.Button("+ Rotate", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.RotateAction>("RotateAction");
            if (GUILayout.Button("+ Squash", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.SquashStretchAction>("SquashAction");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Velocity", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.VelocityAction>("VelocityAction");
            if (GUILayout.Button("+ Condition", GUILayout.Width(75))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.ConditionAction>("ConditionAction");
            if (GUILayout.Button("+ Hitstop/Shake", GUILayout.Width(110))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.HitstopShakeAction>("HitstopShakeAction");
            if (GUILayout.Button("+ Active Toggle", GUILayout.Width(110))) AddNewActionNode<New_Scripts.KinematicActionSystem.Actions.ToggleActiveAction>("ToggleActiveAction");
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }

        private void AddNewActionNode<T>(string defaultName) where T : ActionNode, new()
        {
            T newNode = new T();
            newNode.name = defaultName;
            newNode.startTime = 0f;
            newNode.duration = 1.0f;

            Undo.RecordObject(_runner, "Eylem Eklendi");
            _runner.Actions.Add(newNode);
            
            EditorUtility.SetDirty(_runner);
        }

        private void DeleteActionNode(int index)
        {
            Undo.RecordObject(_runner, "Eylem Silindi");
            _runner.Actions.RemoveAt(index);
            EditorUtility.SetDirty(_runner);
        }

        private void DrawActionProperties()
        {
            GUILayout.Label("Eylem Parametreleri (Properties)", EditorStyles.boldLabel);
            List<ActionNode> actions = _runner.Actions;

            SerializedProperty actionsProp = serializedObject.FindProperty("actions");
            if (actionsProp == null || actionsProp.arraySize != actions.Count)
            {
                return;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null) continue;

                if (!_foldoutStates.ContainsKey(action))
                {
                    _foldoutStates[action] = false;
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Başlık Çubuğu
                GUILayout.BeginHorizontal();
                _foldoutStates[action] = EditorGUILayout.Foldout(_foldoutStates[action], action.name, true);
                
                GUILayout.FlexibleSpace();
                
                SerializedProperty actionProp = actionsProp.GetArrayElementAtIndex(i);
                SerializedProperty isEnabledProp = actionProp.FindPropertyRelative("isEnabled");

                // Etkin/Pasif butonu
                if (isEnabledProp != null)
                {
                    EditorGUILayout.PropertyField(isEnabledProp, GUIContent.none, GUILayout.Width(20));
                    GUILayout.Label("Enabled", GUILayout.Width(50));
                }

                // Silme butonu
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Eylemi Sil", $"{action.name} silinecektir. Emin misiniz?", "Evet", "Hayır"))
                    {
                        DeleteActionNode(i);
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        break;
                    }
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

                // Detay Alanı
                if (_foldoutStates[action])
                {
                    EditorGUI.indentLevel++;

                    // Temel Alanlar
                    SerializedProperty nameProp = actionProp.FindPropertyRelative("name");
                    if (nameProp != null)
                    {
                        EditorGUILayout.PropertyField(nameProp);
                    }

                    SerializedProperty startTimeProp = actionProp.FindPropertyRelative("startTime");
                    if (startTimeProp != null)
                    {
                        EditorGUILayout.PropertyField(startTimeProp);
                    }

                    SerializedProperty durationProp = actionProp.FindPropertyRelative("duration");
                    if (durationProp != null)
                    {
                        EditorGUILayout.PropertyField(durationProp);
                    }

                    // Polimorfik alanları traverse et ve çiz
                    SerializedProperty prop = actionProp.Copy();
                    SerializedProperty endProp = prop.GetEndProperty();
                    
                    bool enterChildren = true;
                    while (prop.NextVisible(enterChildren))
                    {
                        if (SerializedProperty.EqualContents(prop, endProp))
                            break;
                        
                        enterChildren = false;

                        // Sistem alanlarını atla (zaten üstte çizdik)
                        if (prop.name == "name" || prop.name == "startTime" || prop.name == "duration" || prop.name == "isEnabled")
                            continue;

                        EditorGUILayout.PropertyField(prop, true);

                        // Animasyon Eğrileri için Hazır Butonlar
                        if (prop.propertyType == SerializedPropertyType.AnimationCurve)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            GUILayout.Label("Eğri Şablonları:", GUILayout.Width(100));
                            if (GUILayout.Button("EaseInOut", EditorStyles.miniButtonLeft, GUILayout.Width(75)))
                            {
                                prop.animationCurveValue = CurvePresetsUtility.GetEaseInOut();
                                serializedObject.ApplyModifiedProperties();
                            }
                            if (GUILayout.Button("Bounce", EditorStyles.miniButtonMid, GUILayout.Width(65)))
                            {
                                prop.animationCurveValue = CurvePresetsUtility.GetBounce();
                                serializedObject.ApplyModifiedProperties();
                            }
                            if (GUILayout.Button("Spring", EditorStyles.miniButtonRight, GUILayout.Width(65)))
                            {
                                prop.animationCurveValue = CurvePresetsUtility.GetSpring();
                                serializedObject.ApplyModifiedProperties();
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                GUILayout.EndVertical();
            }
        }

        // --- GÜVENLİ ÖNİZLEME (SAFE PREVIEW) MOTORU ---
        private void StartEditorPreview()
        {
            if (_isPreviewing) return;
            
            _isPreviewing = true;
            _previewStartTime = EditorApplication.timeSinceStartup;
            _lastTickTime = _previewStartTime;
            _currentPreviewTime = 0f;

            // Orijinal verileri belleğe kaydet
            _savedState = new TransformState
            {
                position = _runner.transform.position,
                rotation = _runner.transform.rotation,
                scale = _runner.transform.localScale,
                active = _runner.gameObject.activeSelf
            };

            EditorApplication.update += UpdatePreviewLoop;
        }

        private void StopEditorPreview()
        {
            if (!_isPreviewing) return;

            _isPreviewing = false;
            EditorApplication.update -= UpdatePreviewLoop;

            // Orijinal transformu geri yükle (Sıfır kalıntı!)
            _runner.transform.position = _savedState.position;
            _runner.transform.rotation = _savedState.rotation;
            _runner.transform.localScale = _savedState.scale;
            _runner.gameObject.SetActive(_savedState.active);

            _runner.TryGetComponent<IKinematicSolver>(out var solver);
            if (solver != null)
            {
                solver.ResetSolver();
            }

            EditorUtility.SetDirty(_runner.transform);
        }

        private void UpdatePreviewLoop()
        {
            double now = EditorApplication.timeSinceStartup;
            double delta = now - _lastTickTime;
            _lastTickTime = now;

            _currentPreviewTime += (float)delta;
            float totalLength = GetTotalTimelineLength();

            if (_currentPreviewTime >= Mathf.Max(totalLength, 1f))
            {
                _currentPreviewTime = 0f;
            }

            ApplyPreviewStateAt(_currentPreviewTime);
            Repaint();
        }

        private void ApplyPreviewStateAt(float time)
        {
            _runner.transform.position = _savedState.position;
            _runner.transform.rotation = _savedState.rotation;
            _runner.transform.localScale = _savedState.scale;
            _runner.gameObject.SetActive(_savedState.active);

            _runner.TryGetComponent<IKinematicSolver>(out var solver);
            if (solver != null)
            {
                solver.ResetSolver();
            }

            foreach (var action in _runner.Actions)
            {
                if (action == null || !action.isEnabled) continue;

                float localTime = time - action.startTime;
                if (localTime >= 0f)
                {
                    action.Evaluate(_runner.transform, solver, localTime);
                }
            }
        }

        private float GetTotalTimelineLength()
        {
            float maxTime = 0f;
            foreach (var action in _runner.Actions)
            {
                if (action != null)
                {
                    maxTime = Mathf.Max(maxTime, action.startTime + action.duration);
                }
            }
            return maxTime;
        }
    }
}
