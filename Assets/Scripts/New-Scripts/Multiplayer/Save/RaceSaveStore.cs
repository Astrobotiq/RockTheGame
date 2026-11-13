using System;
using System.IO;
using UnityEngine;

namespace New_Scripts.Multiplayer.Save
{
    /// <summary>
    /// Tek yuvalık yarış kaydı dosyası. Varsayılan yol persistentDataPath/multiplayer-race.json; testler kendi yolunu verir.
    /// Yazma önce geçici dosyaya yapılır, sonra yerine taşınır (yarım kalan yazma eski kaydı bozmasın).
    /// </summary>
    public sealed class RaceSaveStore
    {
        public const string FileName = "multiplayer-race.json";

        public string Path { get; }

        private readonly bool _isDefaultPath;

        public RaceSaveStore(string path = null)
        {
            _isDefaultPath = path == null;
            Path = path ?? System.IO.Path.Combine(Application.persistentDataPath, DefaultFileName());
        }

        /// <summary>
        /// Editör ve build aynı persistentDataPath'i paylaşır; editör denemesinin kaydı build'de "Devam et"i açmasın diye
        /// editör kendi dosyasını yazar. Multiplayer Play Mode'un sanal oyuncusu (Library/VP/&lt;id&gt;) da ana editörle aynı
        /// yolu paylaşır; onun dosya adına kimliği eklenir.
        /// </summary>
        private static string DefaultFileName()
        {
#if UNITY_EDITOR
            string dataPath = Application.dataPath.Replace('\\', '/');
            int vp = dataPath.IndexOf("/Library/VP/", StringComparison.Ordinal);
            if (vp >= 0)
            {
                string id = dataPath.Substring(vp + "/Library/VP/".Length).Split('/')[0];
                return $"multiplayer-race-{id}.json";
            }
            return "multiplayer-race-editor.json";
#else
            return FileName;
#endif
        }

        public bool Exists => File.Exists(Path);

        public void Write(RaceSaveData data)
        {
#if UNITY_EDITOR
            // Editörde deneme kaydı bırakmamak için menüden kapatılabilir; testlerin verdiği özel yola etki etmez.
            if (_isDefaultPath && !New_Scripts.Multiplayer.Dev.MultiplayerDevSettings.SaveEnabled) return;
#endif
            string temp = Path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, prettyPrint: true));
            if (File.Exists(Path)) File.Delete(Path);
            File.Move(temp, Path);
        }

        public bool TryRead(out RaceSaveData data)
        {
            data = null;
            if (!File.Exists(Path)) return false;
            try
            {
                data = JsonUtility.FromJson<RaceSaveData>(File.ReadAllText(Path));
                return data != null && data.IsValid;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
        }

        public void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
