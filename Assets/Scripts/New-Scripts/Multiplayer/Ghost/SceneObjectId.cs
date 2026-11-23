using System.Text;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Sahnedeki bir nesnenin iki oyuncuda da aynı çıkan kimliği: kökten nesneye yol, her basamakta kardeş sırasıyla
    /// ("RoomNew-11:4/DynamicContent:2/Platform:7"), FNV-1a 32 bit ile özetlenir. İki oyuncu aynı build'i ve aynı sahneyi
    /// oynadığı için yol iki tarafta aynı; aynı adlı kardeşleri kardeş sırası ayırır. Yalnızca sahne yüklemesinde var olan
    /// nesneler için geçerli: çalışırken yaratılan ya da kardeş sırası değişen nesne farklı kimlik alır.
    /// </summary>
    public static class SceneObjectId
    {
        private const uint FnvOffsetBasis = 2166136261u;
        private const uint FnvPrime = 16777619u;

        /// <summary>Kökten <paramref name="transform"/>'a "ad:kardeşSırası" basamakları, "/" ile birleşik.</summary>
        public static string BuildPath(Transform transform)
        {
            var sb = new StringBuilder();
            Append(sb, transform);
            return sb.ToString();
        }

        private static void Append(StringBuilder sb, Transform t)
        {
            if (t.parent != null)
            {
                Append(sb, t.parent);
                sb.Append('/');
            }
            sb.Append(t.name).Append(':').Append(t.GetSiblingIndex());
        }

        /// <summary>FNV-1a 32 bit, UTF-8 baytları üzerinden.</summary>
        public static uint FromPath(string path)
        {
            uint hash = FnvOffsetBasis;
            foreach (byte b in Encoding.UTF8.GetBytes(path))
            {
                hash ^= b;
                hash *= FnvPrime;
            }
            return hash;
        }

        public static uint For(Transform transform) => FromPath(BuildPath(transform));
    }
}
