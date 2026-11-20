using System;
using UnityEngine;

namespace New_Scripts.Flower
{
    public enum FlowerPhase
    {
        /// <summary>Kapalı, tutulamaz. Son <see cref="FlowerCycleTimings.hint"/> saniyesi açılış ipucu verir.</summary>
        Closed,
        /// <summary>Açılış patlaması (görsel); tutma penceresi <see cref="FlowerCycleTimings.grabAt"/> saniye sonra başlar.</summary>
        Opening,
        /// <summary>Tam açık, tutulabilir.</summary>
        Open,
        /// <summary>Kapanış uyarısı: titrer, renk kayar; hâlâ tutulabilir.</summary>
        Warning,
        /// <summary>Kapanış hamlesi (snap); kısa bir coyote süresi dışında tutulamaz.</summary>
        Closing
    }

    [Serializable]
    public struct FlowerCycleTimings
    {
        [Min(0.1f)] public float closed;
        [Min(0.1f)] public float opening;
        [Min(0.1f)] public float open;
        [Min(0.1f)] public float warning;
        [Min(0.05f)] public float snap;
        [Tooltip("Kapalı fazın sonunda 'şimdi açılacak' ipucunun süresi.")]
        [Min(0f)] public float hint;
        [Tooltip("Opening fazının kaçıncı saniyesinde tutma penceresi başlar (görsel açıklık ~0,6'ya denk).")]
        [Min(0f)] public float grabAt;
        [Tooltip("Snap başladıktan sonra hâlâ tutulabildiği süre (coyote).")]
        [Min(0f)] public float coyote;

        public static FlowerCycleTimings Default => new FlowerCycleTimings
        {
            closed = 1.8f,
            opening = 0.5f,
            open = 2.2f,
            warning = 0.7f,
            snap = 0.15f,
            hint = 0.4f,
            grabAt = 0.3f,
            coyote = 0.1f
        };

        public float CycleLength => closed + opening + open + warning + snap;
    }

    public readonly struct FlowerCycleState
    {
        public readonly FlowerPhase Phase;
        /// <summary>Fazın başından bu yana geçen saniye.</summary>
        public readonly float PhaseTime;
        public readonly bool Grabbable;
        /// <summary>Warning fazında 0..1, diğer zamanlarda 0.</summary>
        public readonly float Warn01;
        /// <summary>Kapalı fazın son hint saniyesinde 0..1, diğer zamanlarda 0.</summary>
        public readonly float Hint01;

        public FlowerCycleState(FlowerPhase phase, float phaseTime, bool grabbable, float warn01, float hint01)
        {
            Phase = phase;
            PhaseTime = phaseTime;
            Grabbable = grabbable;
            Warn01 = warn01;
            Hint01 = hint01;
        }
    }

    /// <summary>Zamanlı çiçek node'unun döngüsü için saf hesap: zaman -> faz, tutulabilirlik, uyarı ve ipucu.</summary>
    public static class FlowerCycleMath
    {
        public static FlowerCycleState Evaluate(float time, FlowerCycleTimings tm)
        {
            float length = Mathf.Max(0.01f, tm.CycleLength);
            float t = Mathf.Repeat(time, length);

            float openingStart = tm.closed;
            float openStart = openingStart + tm.opening;
            float warningStart = openStart + tm.open;
            float snapStart = warningStart + tm.warning;

            if (t < openingStart)
            {
                float intoHint = t - (tm.closed - tm.hint);
                float hint01 = tm.hint > 0f ? Mathf.Clamp01(intoHint / tm.hint) : 0f;
                return new FlowerCycleState(FlowerPhase.Closed, t, false, 0f, hint01);
            }

            if (t < openStart)
            {
                float into = t - openingStart;
                return new FlowerCycleState(FlowerPhase.Opening, into, into >= tm.grabAt, 0f, 0f);
            }

            if (t < warningStart)
                return new FlowerCycleState(FlowerPhase.Open, t - openStart, true, 0f, 0f);

            if (t < snapStart)
            {
                float into = t - warningStart;
                return new FlowerCycleState(FlowerPhase.Warning, into, true, Mathf.Clamp01(into / Mathf.Max(0.01f, tm.warning)), 0f);
            }

            float intoSnap = t - snapStart;
            return new FlowerCycleState(FlowerPhase.Closing, intoSnap, intoSnap < tm.coyote, 0f, 0f);
        }
    }
}
