using UnityEngine;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Küçük yay (sönümlü salınım): <see cref="Target"/> noktasına hedefi biraz aşarak yerleşir; ayrıca <see cref="Kick"/> ile itilebilir.
    /// Tween'in aksine her an yeniden hedeflenebilir (hızlı gezintide üst üste biner, DOKill gerekmez) ve hep aynı noktada durur.
    /// Saf mantık, EditMode'da test edilir.
    /// </summary>
    public sealed class UiSpring
    {
        public float Value;
        /// <summary>Yayın dinlenme noktası; <see cref="Value"/> buraya yerleşir (varsayılan 0).</summary>
        public float Target;
        public float Velocity;

        private readonly float _stiffness;
        private readonly float _damping;

        /// <param name="stiffness">Büyük = daha hızlı salınır.</param>
        /// <param name="damping">Küçük = daha çok aşar (ζ = damping / (2·√stiffness); 0,4 civarı belirgin aşım verir).</param>
        public UiSpring(float stiffness = 180f, float damping = 11f)
        {
            _stiffness = stiffness;
            _damping = damping;
        }

        public void Kick(float velocity) => Velocity += velocity;

        public void Step(float dt)
        {
            dt = Mathf.Min(dt, 1f / 20f);
            const int sub = 4;
            float h = dt / sub;
            for (int i = 0; i < sub; i++)
            {
                Velocity += (-_stiffness * (Value - Target) - _damping * Velocity) * h;
                Value += Velocity * h;
            }
        }

        public bool IsRest => Mathf.Abs(Value - Target) < 1e-4f && Mathf.Abs(Velocity) < 1e-3f;
    }
}
