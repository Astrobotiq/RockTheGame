using UnityEditor;
using UnityEngine;
using New_Scripts.Player.Visual;

namespace New_Scripts.Editor
{
    /// <summary>
    /// PlayerColorController için özel Inspector görünümü.
    /// Taç renk takasını (PaletteSwap shader) Play Mode'da elle tetiklemeyi ve
    /// geçişi yavaşça tarayıp yakından incelemeyi sağlayan butonlar içerir.
    /// </summary>
    [CustomEditor(typeof(PlayerColorController))]
    public class PlayerColorControllerEditor : UnityEditor.Editor
    {
        private const string BlendProperty = "_Blend";

        /// <summary>Tween çalışırken _Blend değerinin canlı görünmesi için sürekli çizim gerekir.</summary>
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            // Varsayılan Inspector elemanlarını çiz
            DrawDefaultInspector();

            PlayerColorController controller = (PlayerColorController)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Renk Takası Testi", EditorStyles.boldLabel);

            DrawFocusButton(controller);

            EditorGUILayout.Space(6);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Renk takası yalnızca Play Mode'da test edilebilir.\n\n" +
                    "Sebebi: takas, SpriteRenderer'ın çalışma zamanı materyal kopyası (renderer.material) " +
                    "üzerinde yapılır. Edit Mode'da bu kopya oluşmadığı için değişiklik paylaşılan " +
                    "materyal asset'ine yazılır ve .mat dosyasını kalıcı olarak kirletir.",
                    MessageType.Info);
                return;
            }

            Material bodyMaterial = GetBodyMaterial();
            if (bodyMaterial == null)
            {
                EditorGUILayout.HelpBox(
                    "Body Renderer atanmamış ya da materyali yok. Yukarıdaki 'Body Renderer' alanını doldur.",
                    MessageType.Warning);
                return;
            }

            if (!bodyMaterial.HasProperty(BlendProperty))
            {
                EditorGUILayout.HelpBox(
                    $"Materyalin shader'ında '{BlendProperty}' özelliği yok. " +
                    $"Beklenen shader: Sprites/Custom/PaletteSwap, mevcut: {bodyMaterial.shader.name}",
                    MessageType.Warning);
                return;
            }

            DrawTriggerButtons(controller);
            EditorGUILayout.Space(8);
            DrawBlendSlider(bodyMaterial);
        }

        /// <summary>Sahne görünümünü karakterin üzerine yakınlaştırır; takas küçük bir taç bölgesinde olduğu için gerekir.</summary>
        private void DrawFocusButton(PlayerColorController controller)
        {
            if (!GUILayout.Button("Sahne görünümünde karaktere yakınlaş"))
            {
                return;
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogWarning("Açık bir Scene view bulunamadı.");
                return;
            }

            // Kücük bir sinir kutusu ver ki kamera iyice yaklassin
            sceneView.Frame(new Bounds(controller.transform.position, Vector3.one * 2.5f), false);
            sceneView.Repaint();
        }

        private void DrawTriggerButtons(PlayerColorController controller)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Dash tükendi"))
                {
                    controller.SetDashExhausted();
                }

                if (GUILayout.Button("Slingshot tükendi"))
                {
                    controller.SetSlingshotExhausted();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Hepsini sıfırla"))
                {
                    controller.ResetAllColors();
                }

                if (GUILayout.Button("Gövde"))
                {
                    controller.ResetBodyColor();
                }

                if (GUILayout.Button("Kollar"))
                {
                    controller.ResetArmColors();
                }
            }
        }

        /// <summary>
        /// _Blend değerini elle tarar. Geçiş tween'i çalışırken değeri tween ezer;
        /// donmuş bir kare üzerinde incelemek için önce "Hepsini sıfırla" deyip
        /// tween bitince slider'ı kullanmak gerekir.
        /// </summary>
        private void DrawBlendSlider(Material bodyMaterial)
        {
            float current = bodyMaterial.GetFloat(BlendProperty);

            EditorGUI.BeginChangeCheck();
            float next = EditorGUILayout.Slider("Takas miktarı (_Blend)", current, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                bodyMaterial.SetFloat(BlendProperty, next);
                SceneView.RepaintAll();
            }

            EditorGUILayout.LabelField(
                current <= 0.001f ? "Şu an: özgün renk (sarı taç)"
                    : current >= 0.999f ? "Şu an: takas tam açık (kırmızı taç)"
                    : "Şu an: geçiş sürüyor",
                EditorStyles.miniLabel);
        }

        /// <summary>Body Renderer alanındaki SpriteRenderer'ın çalışma zamanı materyalini döndürür.</summary>
        private Material GetBodyMaterial()
        {
            SerializedProperty property = serializedObject.FindProperty("bodyRenderer");
            if (property == null || property.objectReferenceValue is not SpriteRenderer bodyRenderer)
            {
                return null;
            }

            return bodyRenderer.material;
        }
    }
}
