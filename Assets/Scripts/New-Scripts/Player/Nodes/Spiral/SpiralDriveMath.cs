using UnityEngine;

namespace New_Scripts.Player.Nodes.Spiral
{
    /// <summary>
    /// Spiral motorunun saf hesapları. Hızlar işaretlidir: +1 = saat yönünün tersi (CCW), −1 = saat yönü (CW).
    /// SwingingState teğeti (d.y, −d.x) yönünde pozitif hız CCW'dir.
    /// </summary>
    public static class SpiralDriveMath
    {
        /// <summary>Stick'in değiştirdiği hedef teğet hız. input [−1, 1]; influence &lt; 1 olduğu sürece işaret hep spinSign'dır.</summary>
        public static float EffectiveTarget(int spinSign, float baseSpeed, float input, float influence, float speedMultiplier)
        {
            return spinSign * baseSpeed * (1f + influence * Mathf.Clamp(input, -1f, 1f)) * speedMultiplier;
        }

        /// <summary>
        /// Teğet hızı hedefe sınırlı ivmeyle yaklaştırır. Ters yönde opposeAccel; aynı yönde altındaysa driveAccel;
        /// üstündeyse overspeedDecay > 0 ise o ivmeyle iner, değilse hız korunur. speed == 0 aynı yön sayılır.
        /// </summary>
        public static float StepTangentialSpeed(float speed, float target, float driveAccel, float opposeAccel, float overspeedDecay, float dt)
        {
            bool opposite = speed * target < 0f;
            if (opposite)
                return Mathf.MoveTowards(speed, target, opposeAccel * dt);

            if (Mathf.Abs(speed) < Mathf.Abs(target))
                return Mathf.MoveTowards(speed, target, driveAccel * dt);

            return overspeedDecay > 0f ? Mathf.MoveTowards(speed, target, overspeedDecay * dt) : speed;
        }

        /// <summary>Her iki ivme de yerçekiminden büyükse motor her ip boyunda tepeyi aşar.</summary>
        public static bool GuaranteesLoop(float driveAccel, float opposeAccel, float gravity)
        {
            float g = Mathf.Abs(gravity);
            return driveAccel > g && opposeAccel > g;
        }

        /// <summary>Komut verilen hızın ratio'su kadarı bile gerçekleşmediyse adım takılmış sayılır.</summary>
        public static bool IsBlocked(float commandedSpeed, float actualSpeed, float ratio)
        {
            float magnitude = Mathf.Abs(commandedSpeed);
            return magnitude > 0.5f && actualSpeed * Mathf.Sign(commandedSpeed) < ratio * magnitude;
        }

        /// <summary>
        /// Gönüllü bırakışta teğete eklenecek işaretli hız (SP8, docs/spiral-cikis-hizi.md). Spin yönünde ve
        /// hız ≥ minSpeedRatio × targetSpeed ise spinSign × boost; spinin tersine ya da yavaşken 0.
        /// </summary>
        public static float ExitBoost(float speedAlongTangent, int spinSign, float targetSpeed, float minSpeedRatio, float boost)
        {
            float withSpin = speedAlongTangent * spinSign;
            if (boost <= 0f || withSpin <= 0f || withSpin < minSpeedRatio * Mathf.Abs(targetSpeed)) return 0f;
            return spinSign * boost;
        }
    }
}
