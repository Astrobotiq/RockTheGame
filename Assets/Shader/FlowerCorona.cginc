#ifndef FLOWER_CORONA_INCLUDED
#define FLOWER_CORONA_INCLUDED

// İplik tacı (pasiflora corona) için ortak yerleşim ve şerit şekli (Flower/Corona, Flower/CoronaShadow). Tek mesh'te halka
// başına tüm şeritler durur (FlowerCoronaMesh); her köşe kendi şeridinin azimutunu ve boy çarpanını TEXCOORD1'de taşır.
// Şerit yaprakla aynı bükmeyi kullanır (FlowerBend.cginc): eğrilik, rulo ve çözülme aynı parametrelerle sürülür.

#include "FlowerBend.cginc"

float _BaseRadius, _BaseHeight;

// uv: şerit ızgarası (x boy, y en). uv2.x: azimut (radyan, yerel +X'ten çiçek ekseni +Y etrafında), uv2.y: boy çarpanı.
// Yerleşim yaprak GameObject'lerininkiyle aynıdır: yerel +X azimuta bakar, taban eksenden _BaseRadius uzakta ve çiçek
// ekseni boyunca _BaseHeight yüksekte durur (iplik tacı yaprakların üstünde yüzer; yoksa kase biçimli yapraklar örter).
void FlowerCoronaVertex(float2 uv, float2 uv2, out float3 pos, out float3 nObj)
{
    FlowerBendL(uv, uv2.y, pos, nObj);

    float s, c;
    sincos(uv2.x, s, c);
    pos = float3(pos.x * c - pos.z * s, pos.y, pos.x * s + pos.z * c) + float3(c * _BaseRadius, _BaseHeight, s * _BaseRadius);
    nObj = float3(nObj.x * c - nObj.z * s, nObj.y, nObj.x * s + nObj.z * c);
}

// Şerit şekli: uçta yarım daire (yuvarlak uçlu kalın şerit), geri kalanı düz. Yalnızca fragment'ta çağrılır (fwidth).
// Dönüş: pikselde kenara uzaklık (negatif = içeride, 0 = kenar).
float FlowerCoronaEdgeDpx(float2 uv)
{
    float u = saturate(uv.x);
    float v = uv.y * 2.0 - 1.0;

    // Uç yarım dairenin yarıçapı = yarım en; boy ekseninde u biriminde (_Width / 2) / _Length.
    float capU = clamp(0.5 * _Width / max(_Length, 1e-4), 1e-3, 0.9);
    float q = saturate((u - (1.0 - capU)) / capU);
    float w = sqrt(saturate(1.0 - q * q));

    float d = abs(v) - w;
    return d / max(fwidth(d), 1e-5);
}

#endif
