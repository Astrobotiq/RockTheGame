using UnityEditor;
using UnityEngine;
using New_Scripts.Platform;

namespace New_Scripts.Editor
{
    /// <summary>
    /// SegmentedBreakablePlatform için özel Inspector görünümü.
    /// Blokların editörde dizilmesi ve parçalanma testlerinin yapılmasını sağlayan butonlar içerir.
    /// </summary>
    [CustomEditor(typeof(SegmentedBreakablePlatform))]
    public class SegmentedBreakablePlatformEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Varsayılan Inspector elemanlarını çiz
            DrawDefaultInspector();

            SegmentedBreakablePlatform platform = (SegmentedBreakablePlatform)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Editor & Level Design Tools", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("🔨 Editörde Blokları Oluştur / Yenile", GUILayout.Height(30)))
            {
                platform.GenerateBlocksInEditor();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.3f);
            if (GUILayout.Button("💥 Parçalanmayı Tetikle (Test)", GUILayout.Height(30)))
            {
                platform.TriggerBreakFromEditor();
            }

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("🔄 Platformu Sıfırla (Reset)", GUILayout.Height(25)))
            {
                platform.ResetToDefault();
            }
        }
    }
}
