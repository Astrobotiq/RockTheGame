using New_Scripts.Platform;
using New_Scripts.Player;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace New_Scripts.Editor
{
    /// <summary>
    /// CameraOverrideZone için FrameArea araçları: sahnede sürüklenebilir çerçeve (BoxBoundsHandle) ve
    /// "çerçeveyi tehlikeye sığdır" düğmesi (seçilen platformun hareket yolu + tetik alanı).
    /// </summary>
    [CustomEditor(typeof(CameraOverrideZone))]
    public class CameraOverrideZoneEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle frameHandle = new BoxBoundsHandle();
        private PlatformController hazard;

        private bool IsFrameArea
        {
            get
            {
                SerializedProperty type = serializedObject.FindProperty("settings.focusType");
                return type != null && type.enumValueIndex == (int)CameraFocusType.FrameArea;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();
            if (!IsFrameArea) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Çerçeve Araçları", EditorStyles.boldLabel);
            hazard = (PlatformController)EditorGUILayout.ObjectField(
                new GUIContent("Tehlike", "Hareket yolu çerçeveye yazılacak platform (PlatformController)."),
                hazard, typeof(PlatformController), true);

            using (new EditorGUI.DisabledScope(hazard == null))
            {
                if (GUILayout.Button("Çerçeveyi tehlikeye sığdır", GUILayout.Height(26)))
                {
                    FitFrameToHazard();
                }
            }

            EditorGUILayout.HelpBox(
                "Çerçeve = tetik alanı + tehlikenin bütün hareket yolu (platform boyutu dahil). Sahnede sarı kutuyu sürükleyerek de ayarlanır. " +
                "Cyan kutu kameranın göstereceği alan; turuncuysa oda sınırı dışına taşıyor (uyuyan oda/boşluk görünmesin).",
                MessageType.None);
        }

        private void OnSceneGUI()
        {
            if (!IsFrameArea) return;

            var zone = (CameraOverrideZone)target;
            SerializedProperty frameProp = serializedObject.FindProperty("settings.frame");
            if (frameProp == null) return;

            serializedObject.Update();
            Rect frame = frameProp.rectValue;

            using (new Handles.DrawingScope(Color.yellow, zone.transform.localToWorldMatrix))
            {
                frameHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
                frameHandle.center = frame.center;
                frameHandle.size = frame.size;

                EditorGUI.BeginChangeCheck();
                frameHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 size = frameHandle.size;
                    Vector3 center = frameHandle.center;
                    frameProp.rectValue = new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void FitFrameToHazard()
        {
            var zone = (CameraOverrideZone)target;
            MovementStrategy strategy = hazard.GetComponentInChildren<MovementStrategy>();
            if (strategy == null || !strategy.TryGetPathBounds(out Rect path))
            {
                EditorUtility.DisplayDialog("Çerçeve", "Bu platformun hareket stratejisi yol sınırı vermiyor (strateji yok ya da uç noktalar atanmamış).", "Tamam");
                return;
            }

            // Yol platform merkezinin gittiği alan; platformun kendi boyutunu (yarı genişlik/yükseklik) ekle.
            Vector2 halfBody = Vector2.zero;
            Collider2D[] colliders = hazard.GetComponentsInChildren<Collider2D>();
            if (colliders.Length > 0)
            {
                Bounds body = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++) body.Encapsulate(colliders[i].bounds);
                halfBody = body.extents;
            }

            Vector2 min = new Vector2(path.xMin - halfBody.x, path.yMin - halfBody.y);
            Vector2 max = new Vector2(path.xMax + halfBody.x, path.yMax + halfBody.y);

            Collider2D trigger = zone.GetComponent<Collider2D>();
            if (trigger != null)
            {
                min = Vector2.Min(min, trigger.bounds.min);
                max = Vector2.Max(max, trigger.bounds.max);
            }

            // Dünya dikdörtgeni → bölgenin yerel uzayı.
            Vector2 a = zone.transform.InverseTransformPoint(min);
            Vector2 b = zone.transform.InverseTransformPoint(max);
            Vector2 localMin = Vector2.Min(a, b);
            Vector2 localMax = Vector2.Max(a, b);

            serializedObject.Update();
            SerializedProperty frameProp = serializedObject.FindProperty("settings.frame");
            frameProp.rectValue = Rect.MinMaxRect(localMin.x, localMin.y, localMax.x, localMax.y);
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }
    }
}
