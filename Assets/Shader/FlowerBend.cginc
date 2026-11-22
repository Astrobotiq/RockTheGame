#ifndef FLOWER_BEND_INCLUDED
#define FLOWER_BEND_INCLUDED

// Çiçek parçalarının ortak bükmesi ve yaprak şekli (Flower/Petal, Flower/PetalShadow). Bükme tek yerde durur ki boya ve
// kesim gölgesi aynı yüzeyi çizsin; bükme değişirse hepsi birlikte değişir.
//
// Pivot yaprağın tabanıdır; yaprak +X'e uzanır, bükülme XY düzleminde (dikey düzlem), en yönü Z'dedir.
// Boyuna eğrilik: k(t) = _Turn * smoothstep(_Reach, _Reach + _Soft, t) + _BaseCurl   (t = 0 taban, 1 uç)
//   _Reach'in gerisi düz kalır, ilerisi _Turn ile RULO halinde sarılır; _Reach 1'e giderken rulo çözülür.
// Enine rulo: _Roll radyan, yaprağın eni boyunca sarılır (kağıdı boyuna rulo yapmak).
// Bu değerleri FlowerBloom her yaprak için MaterialPropertyBlock ile yazar; shader'ların ortak property'leri aynı adı taşır.

#include "UnityCG.cginc"

#define FLOWER_PI 3.14159265359
#define FLOWER_STEPS 14

float _Shape, _Roundness, _Soft, _TipSkew;
float _Length, _Width, _Pitch, _Reach, _Turn, _BaseCurl, _Roll;

// uv: x boy (0..1), y en (0..1). Dönüş: nesne uzayında konum ve yüzey normali.
void FlowerBend(float2 uv, out float3 pos, out float3 nObj)
{
    float u = uv.x;
    float v = uv.y * 2.0 - 1.0;

    // Merkez çizgisini u'ya kadar sayısal entegre et (kıvrım profili parçalı, kapalı form yok).
    float a = _Pitch;
    float2 P = float2(0, 0);
    float h = u / FLOWER_STEPS;
    [unroll]
    for (int i = 0; i < FLOWER_STEPS; i++)
    {
        float t = (i + 0.5) * h;
        float k = _Turn * smoothstep(_Reach, _Reach + _Soft, t) + _BaseCurl;
        float am = a + 0.5 * k * h;
        P += _Length * h * float2(cos(am), sin(am));
        a += k * h;
    }
    float2 N = float2(-sin(a), cos(a));   // bükülme düzleminde iç (içbükey) yüz yönü

    // Enine rulo: en boyunca psi radyan sarılır, kenarlar içbükey yüze doğru kalkar.
    float psi = max(_Roll, 1e-3);
    float R = _Width / psi;
    float phi = psi * v * 0.5;
    float across = R * sin(phi);
    float lift = R * (1.0 - cos(phi));

    pos = float3(P + N * lift, across);
    nObj = float3(N * cos(phi), -sin(phi));
}

// Yaprak şekli (damla): tabanda dar, ~%60'ta en geniş, uçta yuvarlak bir sivrilik. Yalnızca fragment'ta çağrılır (fwidth).
// Dönüş: pikselde kenara uzaklık (negatif = içeride, 0 = kenar).
float FlowerEdgeDpx(float2 uv)
{
    float u = saturate(uv.x);
    float v = uv.y * 2.0 - 1.0;
    float w = pow(saturate(sin(FLOWER_PI * pow(u, _Shape))), _Roundness);
    // Elle kesim: uç bir yana yatık (_TipSkew; 0 = simetrik). Kayma en geniş noktadan sonra başlar (orada şekil mesh
    // sınırına değer), uçta en fazla 2 * _TipSkew (v biriminde, tam en = 2) kadar olur.
    float t = saturate((u - 0.55) / 0.45);
    float vs = v - 2.0 * _TipSkew * t * t;
    float d = abs(vs) - w;
    return d / max(fwidth(d), 1e-5);
}

#endif
