#ifndef FLOWER_SHADOW_INCLUDED
#define FLOWER_SHADOW_INCLUDED

// Kesim gölgesinin ortak köşe hesabı (Flower/PetalShadow, Flower/CoronaShadow): parçanın ekran uzayında sağ-alta ofsetlenmiş
// koyu kopyası. Ofset çiçeğin boyuyla orantılıdır (alt sınırı piksel); _Warn uyarıda ofseti büyütür.
// Kopya eğime göre geriye itilir, böylece kendi yaprağının önüne geçmez (ayrıntı FlowerPetalShadow.shader).

#include "UnityCG.cginc"

fixed4 _CutShadowCol;
float _CutShadowRel, _CutShadowPx, _CutShadowWarnMul, _CutShadowDepth, _Warn;

// posObj, nObj: parçanın nesne uzayındaki bükülmüş konumu ve normali. Dönüş: kırpma uzayı konumu.
float4 FlowerShadowClip(float3 posObj, float3 nObj)
{
    float4 posV = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(posObj, 1.0)));

    // Bir piksel kaç dünya birimi? Ortografikte sabit, perspektifte derinliğe göre. Ofset görünüm uzayında
    // (sağ, aşağı) verilir; projeksiyon çevirmesi (render texture) bunu etkilemez.
    float worldPerPx = unity_OrthoParams.w > 0.5
        ? 2.0 * unity_OrthoParams.y / _ScreenParams.y
        : 2.0 * abs(posV.z) / (UNITY_MATRIX_P._m11 * _ScreenParams.y);
    // Ofset çiçeğin boyuyla orantılı (_CutShadowRel, nesne birimi * nesne ölçeği): ekranda ne kadar yakın ya da uzak olursa olsun
    // gölge çiçeğe oranla aynı kalınlıkta görünür. _CutShadowPx alt sınırdır (çok küçük çiçekte gölge kaybolmasın).
    float scale = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
    float offWorld = max(_CutShadowRel * scale, _CutShadowPx * worldPerPx) * (1.0 + _Warn * _CutShadowWarnMul);
    float2 off = float2(1.0, -1.0) * offWorld;
    posV.xy += off;

    // Kameraya doğru dikilen (eğimli) parçada ofsetli kopya, kendi parçasının önüne geçerdi: yüzey ofset kadar yan kayınca
    // derinliği de eğim kadar değişir. Düzlem yaklaşımıyla bu fark kadar (eğim * ofset) geriye itilir; düz parçalarda itme
    // ~0'dır, böylece gölge altındaki parçanın üstüne düşebilir. _CutShadowDepth küçük sabit paydır.
    float3 nV = mul((float3x3)UNITY_MATRIX_V, UnityObjectToWorldNormal(nObj));
    float tilt = abs(dot(nV.xy, off)) / max(abs(nV.z), 0.1);
    posV.z -= _CutShadowDepth + tilt;   // kameradan uzağa

    return mul(UNITY_MATRIX_P, posV);
}

#endif
