// Sıvı ışık ped shader'ları (LiquidLightPad, LiquidLightBurst) için ortak kod: SDF şekiller, sıvı alan, palet, ton kaydırma.
// Dünya birimi, merkez orijin, SDF negatif = içeride. Sprite 1x1 birim ve nesne uzayında ±0.5 olmalı.
#ifndef LIQUID_LIGHT_COMMON_INCLUDED
#define LIQUID_LIGHT_COMMON_INCLUDED

#include "UnityCG.cginc"

float _Shape;       // 0 dikdörtgen, 1 dik üçgen (90-45-45, dik açı sol-altta), 2 daire
float4 _BodySize;   // gövde boyutu (dünya)
float _Round;       // dikdörtgen köşe yarıçapı (dünya)

// Anti-aliased eşik: genişlik ekran türevinden.
float aastep(float threshold, float value)
{
    float w = max(fwidth(value), 1e-5) * 0.75;
    return smoothstep(threshold - w, threshold + w, value);
}

float sdRect(float2 p, float2 half, float r)
{
    float2 q = abs(p) - (half - r);
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}

// Genel üçgen SDF'i (iq): a, b, c köşeleri, negatif = içeride.
float sdTriangle3(float2 p, float2 a, float2 b, float2 c)
{
    float2 e0 = b - a, e1 = c - b, e2 = a - c;
    float2 v0 = p - a, v1 = p - b, v2 = p - c;
    float2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    float2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    float2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    float s = sign(e0.x * e2.y - e0.y * e2.x);
    float2 d = min(min(float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                       float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                   float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    return -sqrt(d.x) * sign(d.y);
}

// 90-45-45 dik üçgen, seviye köşeleri için: dik açı SOL-ALTTA, dik kenarlar _BodySize kadar (kutu orijinde ortalı),
// hipotenüs sağ-üste bakar. Pad döndürülerek diğer köşelere oturur.
float sdRightTriangle(float2 p, float2 size)
{
    float2 h = size * 0.5;
    return sdTriangle3(p, float2(-h.x, -h.y), float2(h.x, -h.y), float2(-h.x, h.y));
}

float bodySdf(float2 p)
{
    if (_Shape > 1.5) return length(p) - min(_BodySize.x, _BodySize.y) * 0.5;
    if (_Shape > 0.5) return sdRightTriangle(p, _BodySize.xy);
    return sdRect(p, _BodySize.xy * 0.5, _Round);
}

// Sıvı ışık: domain-warp edilmiş yumuşak alan (mockup'taki liquid() ile aynı).
float liquid(float2 uv, float t)
{
    float x = uv.x, y = uv.y;
    float qx = x + 0.9 * sin(y * 1.3 + t * 0.6) + 0.5 * sin(y * 2.7 - t * 0.9);
    float qy = y + 0.9 * sin(x * 1.1 - t * 0.5) + 0.5 * sin(x * 2.3 + t * 0.8);
    float v = sin(qx * 1.2 + t * 0.3) * cos(qy * 1.4 - t * 0.4) + 0.5 * sin((qx + qy) * 0.9 + t * 0.2);
    return saturate(0.5 + v / 3.0);
}

// mor → magenta → kırmızı → turuncu → pembe → pembe-beyaz (sRGB 0..1)
float3 liquidPalette(float v)
{
    const float stops[7] = { 0.0, 0.22, 0.38, 0.55, 0.68, 0.80, 1.0 };
    const float3 cols[7] = {
        float3(80, 0, 255) / 255.0,
        float3(150, 0, 255) / 255.0,
        float3(255, 0, 255) / 255.0,
        float3(255, 0, 40) / 255.0,
        float3(255, 80, 0) / 255.0,
        float3(255, 90, 140) / 255.0,
        float3(255, 210, 245) / 255.0 };
    float3 c = cols[6];
    [unroll]
    for (int i = 6; i >= 1; i--)
    {
        float k = saturate((v - stops[i - 1]) / (stops[i] - stops[i - 1]));
        c = (v <= stops[i]) ? lerp(cols[i - 1], cols[i], k) : c;
    }
    return c;
}

// CSS hue-rotate eşdeğeri (YIQ), açı radyan.
float3 hueRotate(float3 c, float a)
{
    float cs = cos(a), sn = sin(a);
    float3x3 m = float3x3(
        0.213 + cs * 0.787 - sn * 0.213, 0.715 - cs * 0.715 - sn * 0.715, 0.072 - cs * 0.072 + sn * 0.928,
        0.213 - cs * 0.213 + sn * 0.143, 0.715 + cs * 0.285 + sn * 0.140, 0.072 - cs * 0.072 - sn * 0.283,
        0.213 - cs * 0.213 - sn * 0.787, 0.715 - cs * 0.715 + sn * 0.715, 0.072 + cs * 0.928 + sn * 0.072);
    return saturate(mul(m, c));
}

float3 toProjectSpace(float3 srgb)
{
#ifdef UNITY_COLORSPACE_GAMMA
    return srgb;
#else
    return GammaToLinearSpace(srgb);
#endif
}

#endif
