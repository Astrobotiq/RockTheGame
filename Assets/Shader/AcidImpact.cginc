// Asit temas tepkisi: PixelArtAcid ve FlowingPixelAcid ortak kullanır.
// _AcidImpact = (x px, y px, yaş sn, güç). Yaşı AcidReaction (CPU) her kare yazar; shader _Time kullanmaz,
// çünkü URP'de _Time sahne yüklemede sıfırlanmaz. Yuva canlı değilse bütün fonksiyonlar tam 0 döndürür,
// böylece darbe yokken çıktı eskisiyle piksel piksel aynı kalır.
// Properties bloğu include edilemediği için property'ler her shader'da ayrıca tanımlanır.
#ifndef ACID_IMPACT_INCLUDED
#define ACID_IMPACT_INCLUDED

float4 _AcidImpact;
float _ImpactFPS;
float _ImpactDuration;
float _CraterDepth;
float _CraterWidth;
float _CraterRecover;
fixed4 _FlashColor;
float _FlashRadius;
float _FlashTime;
float _RippleAmp;
float _RippleSpeed;
float _RippleWavelength;
float _RippleDecay;
float _RippleFalloff;
float _BoilRadius;
float _BoilThreshold;

inline bool AcidLive()
{
    return _AcidImpact.w > 0.0 && _AcidImpact.z >= 0.0 && _AcidImpact.z < _ImpactDuration;
}

// Yaş, darbenin kendi kare hızına (_ImpactFPS) kesilir.
inline float AcidAge()
{
    return floor(_AcidImpact.z * _ImpactFPS) / max(_ImpactFPS, 1.0);
}

// Yüzey ofseti (px): temas sütununda çöken ve toparlanan çukur. Negatif.
float AcidCrater(float px)
{
    if (!AcidLive()) return 0.0;
    float t = AcidAge();
    float dx = abs(px + 0.5 - _AcidImpact.x);
    float w = max(_CraterWidth, 1e-3);
    return -_CraterDepth * exp(-(dx * dx) / (w * w)) * exp(-_CraterRecover * t) * _AcidImpact.w;
}

// Yüzey ofseti (px): temas noktasından iki yana yayılan, zamanla ve uzaklıkla sönen dalga treni.
// Ön cephe _RippleSpeed ile ilerler; cephenin önü düz, arkası dalgalı. Oyuncu yeniden doğduğunda
// (≈1.3 sn) hâlâ görünür: "biraz önce burada öldüm" izi.
float AcidRipple(float px)
{
    if (!AcidLive()) return 0.0;
    float t = AcidAge();
    float lambda = max(_RippleWavelength, 1.0);
    float dx = abs(px + 0.5 - _AcidImpact.x);
    float ahead = dx - _RippleSpeed * t;                       // > 0: cephe henüz gelmedi
    float front = saturate(1.0 - ahead / lambda);              // cephede bir dalga boyu yumuşak giriş
    float env = exp(-_RippleDecay * t) * exp(-dx / max(_RippleFalloff, 1.0)) * front;
    float fade = saturate(1.0 - t / _ImpactDuration);          // süre sonunda tam 0'a iner
    return _RippleAmp * env * fade * sin(6.2831853 * ahead / lambda) * _AcidImpact.w;
}

// 0..1: temas çevresinde kabarcık yoğunluğu artışı (kaynama), süre boyunca söner.
float AcidBoil(float2 p)
{
    if (!AcidLive()) return 0.0;
    float d = length(p + 0.5 - _AcidImpact.xy);
    return saturate(1.0 - d / max(_BoilRadius, 1.0)) * saturate(1.0 - AcidAge() / _ImpactDuration) * _AcidImpact.w;
}

// 1 = parlama yarıçapı içinde ve parlama süresi dolmamış.
float AcidFlash(float2 p)
{
    if (!AcidLive() || AcidAge() > _FlashTime) return 0.0;
    return step(length(p + 0.5 - _AcidImpact.xy), _FlashRadius);
}

#endif
