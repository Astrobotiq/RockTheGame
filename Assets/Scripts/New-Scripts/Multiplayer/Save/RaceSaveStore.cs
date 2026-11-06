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

        public RaceSaveStore(string path = null)
        {
            Path = path ?? System.IO.Path.Combine(Application.persistentDataPath, FileName);
        }

        public bool Exists => File.Exists(Path);

        public void Write(RaceSaveData data)
        {
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
