using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace New_Scripts.Editor
{
    /// <summary>
    /// Unity Editor penceresi aracılığıyla Gradient veya Renk Listesi (Color List) üzerinden 
    /// 1D Ramp Texture (.png) üreten araç.
    /// Renk Listesi modu sayesinde Unity Gradient editöründeki 8 renk sınırını aşabilirsiniz.
    /// </summary>
    public class RampTextureGeneratorWindow : EditorWindow
    {
        private enum Mode
        {
            Gradient,
            ColorList
        }

        private Mode _mode = Mode.ColorList;
        private Gradient _gradient = new Gradient();
        private List<Color> _colors = new List<Color>
        {
            Color.red,
            new Color(1f, 0.5f, 0f), // Turuncu
            Color.yellow,
            Color.green,
            Color.cyan,
            new Color(0f, 0.5f, 1f), // Açık Mavi
            Color.blue,
            new Color(0.5f, 0f, 1f), // Mor
            Color.magenta,
            Color.white
        };

        private int _ringCount = 10;
        private string _fileName = "RampTex_10Rings";
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Ramp Texture Generator")]
        public static void ShowWindow()
        {
            RampTextureGeneratorWindow window = GetWindow<RampTextureGeneratorWindow>("Ramp Gen");
            window.minSize = new Vector2(350, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // Varsayılan kırmızı-mavi bir geçiş ile gradient'ı başlat
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.blue, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            _gradient.SetKeys(colorKeys, alphaKeys);
        }

        private void OnGUI()
        {
            GUILayout.Label("Ramp Texture Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Mod seçimi
            _mode = (Mode)EditorGUILayout.EnumPopup("Generation Mode", _mode);
            EditorGUILayout.Space();

            if (_mode == Mode.Gradient)
            {
                _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
                _ringCount = EditorGUILayout.IntField("Ring Count (Resolution)", _ringCount);
                _ringCount = Mathf.Clamp(_ringCount, 1, 4096);
                
                EditorGUILayout.HelpBox("Unity'nin dahili Gradient yapısı en fazla 8 renk anahtarı destekler. Daha fazla renk için 'Color List' moduna geçebilirsiniz.", MessageType.Info);
            }
            else
            {
                // Renk Listesi Arayüzü
                int currentCount = _colors.Count;
                int newCount = EditorGUILayout.IntField("Color Count (Rings)", currentCount);
                newCount = Mathf.Clamp(newCount, 1, 128); // Mantıklı bir sınır

                if (newCount != currentCount)
                {
                    while (_colors.Count < newCount) _colors.Add(Color.white);
                    while (_colors.Count > newCount) _colors.RemoveAt(_colors.Count - 1);
                }

                EditorGUILayout.Space();
                GUILayout.Label("Colors List:", EditorStyles.miniBoldLabel);

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(200));
                for (int i = 0; i < _colors.Count; i++)
                {
                    _colors[i] = EditorGUILayout.ColorField($"Ring Color {i + 1}", _colors[i]);
                }
                EditorGUILayout.EndScrollView();

                _ringCount = _colors.Count;
            }

            EditorGUILayout.Space();
            _fileName = EditorGUILayout.TextField("Default File Name", _fileName);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate and Save Texture"))
            {
                GenerateAndSave();
            }
        }

        private void GenerateAndSave()
        {
            if (_ringCount <= 0) return;

            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save Ramp Texture",
                _fileName,
                "png",
                "Please enter a file name to save the texture to."
            );

            if (string.IsNullOrEmpty(savePath))
                return;

            // Dokuyu oluştur (Genişliği halka sayısı kadar, yüksekliği 1 piksel)
            Texture2D texture = new Texture2D(_ringCount, 1, TextureFormat.RGBA32, false);
            
            Color[] colors = new Color[_ringCount];
            if (_mode == Mode.Gradient)
            {
                for (int i = 0; i < _ringCount; i++)
                {
                    float t = (i + 0.5f) / _ringCount;
                    colors[i] = _gradient.Evaluate(t);
                }
            }
            else
            {
                for (int i = 0; i < _ringCount; i++)
                {
                    colors[i] = _colors[i];
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            // PNG formatına dönüştür
            byte[] bytes = texture.EncodeToPNG();
            DestroyImmediate(texture);

            // Diske yaz
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.ImportAsset(savePath);

            // İçe aktarma (Import) ayarlarını halka çizimine uygun olarak yapılandır
            TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;

                // Renklerin bozulmaması için sıkıştırmayı kapat
                TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
                defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(defaultSettings);

                importer.SaveAndReimport();
                
                Debug.Log($"[Ramp Generator] Doku başarıyla oluşturuldu ve içe aktarıldı: {savePath}");
                
                // Oluşturulan görseli Project penceresinde odakla
                Object obj = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }
            else
            {
                Debug.LogError("[Ramp Generator] Doku içe aktarılamadı.");
            }
        }
    }
}
