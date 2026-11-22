using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin karaktere tepkisinin o anki durumu: <see cref="FlowerReactor"/> yazar, <see cref="FlowerBloom"/> okuyup çizer.
    /// Diziler bir kez ayrılır ve yerinde güncellenir (her karede allocation yok). Okunabilir olduğu için testler ve
    /// gamepad ayarı materyal bloğundan değil buradan bakar.
    /// </summary>
    public sealed class FlowerReactionState
    {
        /// <summary>Yaprak başına yana itme (radyan, işaretli; pozitif = saat yönünün tersi). Çiçek-yerel azimutta artan yön.</summary>
        public readonly float[] PetalYaw;
        /// <summary>Yaprak başına aşağı bastırma (radyan, yaylandıktan sonra ≥ 0'a yakın; bırakışta küçük aşım olabilir).</summary>
        public readonly float[] PetalDip;
        /// <summary>Tutunma yumruğu 0..1 (yaylı; bırakışta kısa süre 1'i ya da 0'ı aşabilir).</summary>
        public float Grip01;
        /// <summary>Tepki kapısı 0..1 (<see cref="FlowerReactMath.Gate"/>).</summary>
        public float Gate;
        /// <summary>Tutunan kolun çiçek-yerel azimutu (radyan, gecikmeli izlenen, sarılmamış).</summary>
        public float ArmAzimuth;
        /// <summary>Tam kavramada iplik tacı şeritlerinin duruşu (ayar SO'sundan).</summary>
        public FlowerCoronaMath.GripShape Shape = FlowerCoronaMath.GripShape.Default;

        public FlowerReactionState(int petalCount)
        {
            PetalYaw = new float[Mathf.Max(1, petalCount)];
            PetalDip = new float[Mathf.Max(1, petalCount)];
        }

        /// <summary>Yaprak yaw ve dip büyüklüklerinin en büyüğü (radyan): "çiçek şu an tepki veriyor mu" sorusu için.</summary>
        public float MaxPetalMotion
        {
            get
            {
                float m = 0f;
                for (int i = 0; i < PetalYaw.Length; i++)
                    m = Mathf.Max(m, Mathf.Abs(PetalYaw[i]), Mathf.Abs(PetalDip[i]));
                return m;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < PetalYaw.Length; i++) { PetalYaw[i] = 0f; PetalDip[i] = 0f; }
            Grip01 = 0f;
            Gate = 0f;
        }
    }
}
