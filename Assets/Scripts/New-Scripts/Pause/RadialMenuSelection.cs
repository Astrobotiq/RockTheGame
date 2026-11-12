using UnityEngine;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Radyal menünün saf seçim mantığı: dilim 0 tepede, saat yönünde dizilir. Sol stick yönü (<c>atan2</c>) dilimi seçer,
    /// seçilemeyen (soluk) dilimler atlanır. Unity bileşeni yok, EditMode'da test edilir (<c>RadialMenuSelectionTests</c>).
    /// </summary>
    public static class RadialMenuSelection
    {
        /// <summary>Stick bu uzunluğun altındaysa seçim değişmez (kazara dokunma, merkez titremesi).</summary>
        public const float DefaultDeadzone = 0.5f;

        /// <summary>Yönün tepeden saat yönünde açısı, 0..360 derece.</summary>
        public static float AngleClockwiseFromUp(Vector2 direction)
        {
            float degrees = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            return degrees < 0f ? degrees + 360f : degrees;
        }

        /// <summary>Yönün gösterdiği dilim; ölü bölgedeyse -1. Her dilimin ortası <c>i * 360 / count</c> derecedir.</summary>
        public static int SliceAt(Vector2 direction, int sliceCount, float deadzone = DefaultDeadzone)
        {
            if (sliceCount <= 0 || direction.magnitude < deadzone) return -1;

            float width = 360f / sliceCount;
            return Mathf.FloorToInt((AngleClockwiseFromUp(direction) + width * 0.5f) / width) % sliceCount;
        }

        /// <summary>Dilim ortasının birim yönü (x = sin, y = cos); etiket ve dışarı çıkma yönü için.</summary>
        public static Vector2 SliceDirection(int index, int sliceCount)
        {
            float radians = index * 360f / sliceCount * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }

        /// <summary>
        /// Stick yönüne göre yeni seçim. Ölü bölgede ya da seçilemeyen dilimi gösterirken <paramref name="current"/> kalır.
        /// </summary>
        public static int Pick(Vector2 direction, int current, bool[] enabled, float deadzone = DefaultDeadzone)
        {
            int slice = SliceAt(direction, enabled.Length, deadzone);
            return slice >= 0 && enabled[slice] ? slice : current;
        }

        /// <summary>
        /// <paramref name="step"/> dilim kadar saat yönünde (+) ya da tersine (-) döner, seçilemeyenleri atlar ve sarar.
        /// Hiçbiri seçilemezse (ya da yalnızca mevcut seçilebilirse) <paramref name="current"/> kalır.
        /// </summary>
        public static int Rotate(int current, int step, bool[] enabled)
        {
            int count = enabled.Length;
            if (count == 0 || step == 0) return current;

            int direction = step > 0 ? 1 : -1;
            int index = current;
            for (int moved = 0; moved < Mathf.Abs(step); moved++)
            {
                int next = index;
                for (int tries = 0; tries < count; tries++)
                {
                    next = ((next + direction) % count + count) % count;
                    if (enabled[next]) break;
                }

                if (!enabled[next]) return current;
                index = next;
            }

            return index;
        }
    }
}
