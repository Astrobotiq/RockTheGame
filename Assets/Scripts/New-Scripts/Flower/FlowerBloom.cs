using System;
using System.Collections.Generic;
using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Shader ile çizilen cel-shaded çiçek (kameraya bakan, top-down). Her yaprak ayrı bir plane
    /// (<see cref="FlowerPetalMesh"/> ızgarası) ve <c>Flower/Petal</c> shader'ı; ortası <c>Flower/Center</c>, açılışta
    /// dışa yayılan halka <c>Flower/Halo</c>. Çiçeğin ekseni yerel +Y; <see cref="viewTilt"/> 90 iken tam ekrana bakar.
    ///
    /// Tek sürücü <see cref="Open"/>: 0 kapalı (yumruk / rulo), 1 tam açık, &gt; 1 overshoot (yapraklar biraz fazla yatar,
    /// çiçek hafif büyür ve ters yönde döner), &lt; 0 anticipation (çiçek hafifçe büzülür). Oyunda <see cref="OpenBloom"/> /
    /// <see cref="CloseBloom"/> / <see cref="SetWarn"/> / <see cref="SetHint"/> açılış değerini yaylarla sürer
    /// (<see cref="FlowerCycle"/> bunları zamanlayıcıyla çağırır); sürücü yoksa <c>open</c> slider'ı doğrudan uygulanır.
    /// </summary>
    [ExecuteAlways]
    public class FlowerBloom : MonoBehaviour
    {
        private static readonly int OpenId = Shader.PropertyToID("_Open");
        private static readonly int WarnId = Shader.PropertyToID("_Warn");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int LengthId = Shader.PropertyToID("_Length");
        private static readonly int WidthId = Shader.PropertyToID("_Width");
        private static readonly int PitchId = Shader.PropertyToID("_Pitch");
        private static readonly int ReachId = Shader.PropertyToID("_Reach");
        private static readonly int TurnId = Shader.PropertyToID("_Turn");
        private static readonly int BaseCurlId = Shader.PropertyToID("_BaseCurl");
        private static readonly int RollId = Shader.PropertyToID("_Roll");
        private static readonly int SoftId = Shader.PropertyToID("_Soft");
        private static readonly int ShapeId = Shader.PropertyToID("_Shape");
        private static readonly int RoundnessId = Shader.PropertyToID("_Roundness");
        private static readonly int TipSkewId = Shader.PropertyToID("_TipSkew");
        private static readonly int BaseRadiusId = Shader.PropertyToID("_BaseRadius");
        private static readonly int BaseHeightId = Shader.PropertyToID("_BaseHeight");
        private static readonly int DashesId = Shader.PropertyToID("_Dashes");
        private static readonly int CoreRadiusId = Shader.PropertyToID("_CoreRadius");
        private static readonly int RingId = Shader.PropertyToID("_Ring");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int RevealId = Shader.PropertyToID("_Reveal");

        [Serializable]
        private class Petal
        {
            public Transform Pivot;
            public MeshRenderer Renderer;
            public int Layer;
            public int Index;
            public float Seed;
        }

        /// <summary>İplik tacının bir halkası: şerit boyutları, yerleşimi ve kapalı/açık duruşu.</summary>
        [Serializable]
        private class CoronaRing
        {
            [Tooltip("Halkanın dönüklüğü, şerit adımının oranı (iç halka 0.5: dış halkayla dişlenir).")]
            [Range(0f, 1f)] public float phase;
            [Tooltip("Açıkken şerit boyu (petal boyu biriminde).")]
            [Min(0.02f)] public float length = 0.55f;
            [Tooltip("Şerit eni. Komşu şeritlerle birleşmesin: taban çevresi / şerit sayısından küçük olmalı.")]
            [Min(0.02f)] public float width = 0.14f;
            [Tooltip("Açıkken tabanın eksenden uzaklığı.")]
            [Min(0f)] public float baseRadius = 0.3f;
            [Tooltip("Açıkken yaprakların üstündeki yükseklik (kase biçimli yapraklar şeritleri örtmesin).")]
            [Min(0f)] public float baseHeight = 0.3f;
            [Range(-30f, 90f)] public float openPitchDeg = 20f;
            [Range(0f, 110f)] public float closedPitchDeg = 70f;
            [Tooltip("Boy boyunca eğrilik, açıkken / kapalıyken (radyan / boy birimi).")]
            public float openCurl = 0.4f;
            public float closedCurl = 1.4f;
        }

        [Header("Açılma")]
        [Tooltip("Oyun dışında / sürücü yokken uygulanan değer. 0 kapalı, 1 açık, >1 overshoot, <0 anticipation.")]
        [SerializeField, Range(-0.2f, 1.3f)] private float open = 1f;
        [Tooltip("Dış katman önce açılır; iç katmanın gecikmesi (açılma değerinin oranı).")]
        [SerializeField, Range(0f, 0.6f)] private float layerDelay = 0.25f;
        [Tooltip("Aynı katmanda altın oran sırasıyla yapraklar arası yayılma (açılma değerinin oranı).")]
        [SerializeField, Range(0f, 0.5f)] private float indexStagger = 0.15f;

        [Header("Yaylar")]
        [Tooltip("Açılış yayı. ζ = damping / (2·√stiffness): 180/14,8 -> ζ≈0,55, ~%12 aşım, ~0,5 sn yerleşme.")]
        [SerializeField] private float openStiffness = 180f;
        [SerializeField] private float openDamping = 14.8f;
        [Tooltip("Kapanış hamlesi (snap): sert ve az aşımlı. 900/48 -> ζ≈0,8, ~0,15 sn.")]
        [SerializeField] private float closeStiffness = 900f;
        [SerializeField] private float closeDamping = 48f;
        [Tooltip("Açılış başında yayın hedefi kısa süre bu (negatif) değere çekilir: çiçek bir an büzülür (anticipation). Gerçek dip ~yarısı kadardır.")]
        [SerializeField, Range(-0.3f, 0f)] private float anticipationDip = -0.08f;
        [Tooltip("Anticipation süresi (saniye). Sonra yay 1'e fırlar.")]
        [SerializeField, Range(0f, 0.2f)] private float anticipationTime = 0.08f;
        [Tooltip("Ortadaki nokta 'pop' yayı: 260/12 -> ζ≈0,37, belirgin aşım.")]
        [SerializeField] private float popStiffness = 260f;
        [SerializeField] private float popDamping = 12f;
        [Tooltip("Ortadaki nokta, açılma bu değeri geçince pop yapar.")]
        [SerializeField, Range(0f, 1f)] private float centerRevealAt = 0.45f;
        [Tooltip("Kapalıyken görünen tomurcuk çekirdeğinin boyu (açık merkeze oranla).")]
        [SerializeField, Range(0.3f, 1f)] private float budScale = 0.8f;

        [Header("Top-down hareket")]
        [Tooltip("Kapalıdan açığa çiçeğin kendi ekseninde dönüşü (derece). Açılırken çözülür, overshoot'ta ters yöne taşar.")]
        [SerializeField] private float openTwist = 22f;
        [Tooltip("Overshoot'ta (open > 1) her +1 için yaprakların ekstra yatma açısı (derece).")]
        [SerializeField] private float overshootPitch = 24f;
        [Tooltip("Overshoot'ta her +1 için çiçeğin ekstra büyümesi.")]
        [SerializeField] private float overshootScale = 0.5f;
        [Tooltip("Kapanış uyarısında yaprak uçları bu açıklığa kadar geri kıvrılır.")]
        [SerializeField, Range(0.4f, 1f)] private float warnCurlTarget = 0.8f;
        [Tooltip("Uyarıda titreme genliği (dünya birimi, ~px/100) ve frekans aralığı (Hz).")]
        [SerializeField] private float trembleAmplitude = 0.025f;
        [SerializeField] private Vector2 trembleHz = new Vector2(8f, 20f);
        [Tooltip("Kapalıyken 'şimdi açılacak' ipucunun şişme miktarı (open birimi).")]
        [SerializeField] private float hintSwell = 0.07f;

        [Header("Halo")]
        [SerializeField] private float haloDuration = 0.35f;
        [SerializeField] private float haloRadius = 1.7f;

        [Header("Uyarı halkası")]
        [Tooltip("Kapanış uyarısında çiçeğin etrafında yanıp sönen kesikli halkanın yarıçapı (petal boyu birimi; çiçeğin dışında kalmalı).")]
        [SerializeField, Min(0.5f)] private float warnRingRadius = 1.35f;
        [SerializeField, Range(4, 24)] private int warnRingDashes = 12;
        [Tooltip("Halka kalınlığı (halo quad yarıçapının oranı).")]
        [SerializeField, Range(0.02f, 0.3f)] private float warnRingWidth = 0.09f;
        [Tooltip("Yanıp sönme hızı (Hz). Erişilebilirlik için 3'e kırpılır (FlowerWarnMath.MaxBlinkHz).")]
        [SerializeField, Range(0.5f, 3f)] private float warnBlinkHz = 2f;

        [Header("Şekil")]
        [SerializeField, Min(3)] private int petalCount = 5;
        [SerializeField, Range(1, 4)] private int layerCount = 1;
        [SerializeField, Min(0.05f)] private float coreRadius = 0.22f;
        [SerializeField, Min(0.05f)] private float petalLength = 1f;
        [Tooltip("Yaprak eni. Kâğıt çiçekte 5 geniş yaprak komşusuyla hafifçe örtüşür.")]
        [SerializeField, Min(0.05f)] private float petalWidth = 1f;
        [Tooltip("Elle kesim: her yaprağın boy, en ve uç yatıklığı kendi Seed'inden gelir (FlowerCutMath). Kapatınca yapraklar eşit olur.")]
        [SerializeField] private bool handCut = true;
        [Tooltip("Her iç katmanın boy çarpanı.")]
        [SerializeField, Range(0.3f, 1f)] private float layerLengthScale = 0.65f;
        [Tooltip("Her iç katmanın en çarpanı.")]
        [SerializeField, Range(0.3f, 1.2f)] private float layerWidthScale = 0.92f;

        [Header("Kapalı hal (yumruk / rulo)")]
        [Tooltip("Yaprağın yataydan dikilme açısı (derece). 90 = dik.")]
        [SerializeField, Range(0f, 110f)] private float closedPitch = 50f;
        [Tooltip("Kapalıyken yaprağın tabandan dik kalan kısmı (0-1); ruloya bundan sonra başlar.")]
        [SerializeField, Range(0f, 0.9f)] private float closedStraight = 0.5f;
        [Tooltip("Kapalıyken yaprak tabanlarının eksenden uzaklığı (çekirdek yarıçapının oranı).")]
        [SerializeField, Range(0f, 1f)] private float closedBaseRadius = 0.5f;
        [Tooltip("Sarılı kısımdaki tur sayısı. Rulo çözülürken yarıçap sabit kalır, tur azalır.")]
        [SerializeField, Range(0.2f, 3f)] private float closedTurns = 1f;
        [Tooltip("Yaprağın eni boyunca sarılma açısı (radyan); kağıdı boyuna rulo yapmak.")]
        [SerializeField, Range(0f, 6f)] private float closedRoll = 2.4f;

        [Header("Açık hal")]
        [Tooltip("Yaprağın yataydan kalkma açısı (derece). 0 = düz yatık.")]
        [SerializeField, Range(-30f, 90f)] private float openPitch = 12f;
        [Tooltip("Her iç katman için eklenen dik açı (derece).")]
        [SerializeField] private float innerPitchBonus = 8f;
        [Tooltip("Açıkken yaprağa verilen hafif kase kıvrımı (radyan, tüm boy boyunca).")]
        [SerializeField, Range(-1f, 2f)] private float openCurl = 0.55f;
        [SerializeField, Range(0f, 3f)] private float openRoll = 0.5f;

        [Header("Görünüm")]
        [Tooltip("Çiçeğin kameraya doğru eğilmesi (derece). 90 = tam ekrana bakar (top-down), 0 = yandan.")]
        [SerializeField, Range(0f, 90f)] private float viewTilt = 90f;
        [Tooltip("Çiçeğin tamamı kameradan bu kadar uzağa itilir (petal boyu birimiyle). Yapraklar derinlik yazdığı için " +
                 "kameraya doğru çıkan yaprak z = 0'daki oyuncu ve diğer sprite'ların önüne geçerdi; en öne çıkan yaprak bile " +
                 "arkada kalmalı. Kapalı hal en yükseği (~1 × petal boyu) verir.")]
        [SerializeField, Range(0f, 3f)] private float depthBehind = 1.25f;
        [Tooltip("Rulonun çözülürken geçiş yumuşaklığı (shader _Soft ile aynı değer).")]
        [SerializeField, Range(0.05f, 0.6f)] private float reachSoftness = 0.25f;
        [SerializeField] private Mesh petalMesh;
        [SerializeField] private Material petalMaterial;
        [Tooltip("Yaprağın kesim gölgesi (Flower/PetalShadow): aynı mesh'e ikinci materyal olarak çizilir. Boşsa gölge yok. " +
                 "Atandıktan sonra Rebuild gerekir.")]
        [SerializeField] private Material petalShadowMaterial;
        [SerializeField] private Material centerMaterial;
        [SerializeField] private Material haloMaterial;
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int baseSortingOrder;

        [Header("İplik tacı (pasiflora)")]
        [Tooltip("İplik tacı materyali (Flower/Corona). Boşsa taç yok. Atandıktan sonra Rebuild gerekir.")]
        [SerializeField] private Material coronaMaterial;
        [Tooltip("İplik tacı gölgesi (Flower/CoronaShadow): aynı mesh'e ikinci materyal. Boşsa gölge yok.")]
        [SerializeField] private Material coronaShadowMaterial;
        [Tooltip("Halka başına şerit sayısı. Değişince Rebuild gerekir.")]
        [SerializeField, Range(3, 16)] private int coronaStrips = 10;
        [Tooltip("Açılma değeri bu eşiği geçince taç açılmaya başlar (yapraklar önce açılır).")]
        [SerializeField, Range(0f, 0.9f)] private float coronaOpenStart = FlowerCoronaMath.DefaultOpenStart;
        [SerializeField] private CoronaRing coronaOuter = new CoronaRing
        {
            phase = 0f, length = 0.55f, width = 0.14f, baseRadius = 0.3f, baseHeight = 0.3f,
            openPitchDeg = 20f, closedPitchDeg = 70f, openCurl = 0.4f, closedCurl = 1.4f
        };
        [SerializeField] private CoronaRing coronaInner = new CoronaRing
        {
            phase = 0.5f, length = 0.32f, width = 0.11f, baseRadius = 0.2f, baseHeight = 0.4f,
            openPitchDeg = 40f, closedPitchDeg = 75f, openCurl = 0.6f, closedCurl = 1.6f
        };

        [SerializeField, HideInInspector] private MeshRenderer[] coronaRenderers;
        [SerializeField, HideInInspector] private int builtCoronaStrips;
        [SerializeField, HideInInspector] private List<Petal> petals = new List<Petal>();
        [SerializeField, HideInInspector] private MeshRenderer centerRenderer;
        [SerializeField, HideInInspector] private MeshRenderer haloRenderer;
        [SerializeField, HideInInspector] private Transform model;

        private static MaterialPropertyBlock _block;

        private UiSpring _spring = new UiSpring();
        private UiSpring _pop = new UiSpring();
        private bool _driven;
        private bool _wantOpen;
        private float _warn;
        private float _hint;
        private float _haloT = -1f;
        private float _tremblePhase;
        private float _blinkClock;
        private float _anticipateLeft;

        /// <summary>Şu an çizilen açılma değeri (spring sürüyorsa spring değeri).</summary>
        public float Open
        {
            get => open;
            set { open = value; _driven = false; }
        }

        /// <summary>Kapanış uyarısı 0..1 (Warn fazı). Editörde önizleme için doğrudan da ayarlanabilir.</summary>
        public float Warn01 => _warn;

        /// <summary>Halka ilerlemesi 0..1; &lt; 0 = görünmez. Oyunda açılışta otomatik çalışır, editörde önizleme için ayarlanabilir.</summary>
        public float HaloProgress
        {
            get => _haloT;
            set => _haloT = value;
        }

        /// <summary>Açılış: anticipation, ardından overshoot'lu yayla 1'e. Halka dalgası başlar.</summary>
        public void OpenBloom()
        {
            _anticipateLeft = anticipationTime;
            BeginDrive(openStiffness, openDamping, anticipationTime > 0f ? anticipationDip : 1f);
            _wantOpen = true;
            _warn = 0f;
            _haloT = 0f;
        }

        /// <summary>Kapanış hamlesi: sert yay, yaprak uçları öne geçerek rulo olur.</summary>
        public void CloseBloom()
        {
            BeginDrive(closeStiffness, closeDamping, 0f);
            _wantOpen = false;
            _warn = 0f;
        }

        /// <summary>Kapanış uyarısı (0..1): yaprak uçları geri kıvrılır, çiçek titrer, renk kayar.</summary>
        public void SetWarn(float warn01)
        {
            _warn = Mathf.Clamp01(warn01);
            if (_wantOpen && _anticipateLeft <= 0f) _spring.Target = Mathf.Lerp(1f, warnCurlTarget, _warn);
        }

        /// <summary>Kapalıyken "şimdi açılacak" ipucu (0..1): tomurcuk hafifçe şişer.</summary>
        public void SetHint(float hint01)
        {
            _hint = Mathf.Clamp01(hint01);
            if (!_wantOpen) _spring.Target = hintSwell * _hint;
        }

        /// <summary>Yayları sıfırlayıp çiçeği anında bu açıklığa koyar (sürücü modunda).</summary>
        public void Snap(float value)
        {
            _driven = true;
            _wantOpen = value >= 0.5f;
            _spring = new UiSpring(openStiffness, openDamping) { Value = value, Target = value };
            _pop = new UiSpring(popStiffness, popDamping) { Value = value >= centerRevealAt ? 1f : 0f, Target = value >= centerRevealAt ? 1f : 0f };
            _haloT = -1f;
            open = value;
        }

        private void BeginDrive(float stiffness, float damping, float target)
        {
            // Yaylar sabit katsayılı: açılış/kapanış katsayısı değişince durumu (değer + hız) yeni yaya taşı.
            var s = new UiSpring(stiffness, damping) { Value = _spring.Value, Velocity = _spring.Velocity, Target = target };
            _spring = s;
            _driven = true;
        }

        private void OnEnable()
        {
            if (!IsBuilt()) Rebuild();
            EnsureCoronaMeshes();
            _spring = new UiSpring(openStiffness, openDamping) { Value = open, Target = open };
            _pop = new UiSpring(popStiffness, popDamping);
        }

        private void Update()
        {
            float dt = Application.isPlaying ? Time.deltaTime : 0f;

            if (Application.isPlaying && _driven)
            {
                if (_anticipateLeft > 0f)
                {
                    _anticipateLeft -= dt;
                    if (_anticipateLeft <= 0f) _spring.Target = Mathf.Lerp(1f, warnCurlTarget, _warn);
                }

                _spring.Step(dt);
                open = _spring.Value;
            }

            // Orta: açılma eşiğini geçince pop yayıyla büyür; kapanınca anında sıfırlanmaya yakın küçülür.
            float popTarget = open >= centerRevealAt ? 1f : 0f;
            _pop.Target = popTarget;
            if (Application.isPlaying)
            {
                // Gecikmeli "pop": kapanırken yay yerine hızlı çekil (negatif aşım görünmesin).
                if (popTarget < 0.5f) { _pop.Value = Mathf.MoveTowards(_pop.Value, 0f, dt * 12f); _pop.Velocity = 0f; }
                else _pop.Step(dt);
            }
            else
            {
                _pop.Value = popTarget;
                _pop.Velocity = 0f;
            }

            // Halka dalgası
            if (_haloT >= 0f && Application.isPlaying)
            {
                _haloT += dt / Mathf.Max(0.05f, haloDuration);
                if (_haloT >= 1f) _haloT = -1f;
            }

            if (_warn > 0f && Application.isPlaying)
                _tremblePhase += dt * Mathf.Lerp(trembleHz.x, trembleHz.y, _warn) * Mathf.PI * 2f;

            // Uyarı halkasının yanıp sönme saati: her uyarı yanık başlar (saat 0), uyarı bitince sıfırlanır.
            if (_warn > FlowerWarnMath.VisibleAbove) _blinkClock += dt;
            else _blinkClock = 0f;

            Apply();
        }

        private bool IsBuilt()
        {
            if (model == null || centerRenderer == null || haloRenderer == null || petals.Count != petalCount * layerCount) return false;
            foreach (var p in petals)
                if (p == null || p.Pivot == null) return false;

            // İplik tacı: materyal varsa iki halka renderer'ı ve şerit sayısı tutmalı; materyal yoksa taç hiç üretilmemiş olmalı.
            if (coronaMaterial != null)
            {
                if (coronaRenderers == null || coronaRenderers.Length != 2 || builtCoronaStrips != coronaStrips) return false;
                foreach (var r in coronaRenderers)
                    if (r == null) return false;
            }
            else if (coronaRenderers != null && coronaRenderers.Length > 0)
            {
                return false;
            }

            return true;
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            // Yalnızca kendi ürettiği "Model" silinir: yanına konan çocuklar (kanca node'u collider'ı gibi) yaşar.
            Transform old = transform.Find("Model");
            if (old != null) Kill(old.gameObject);
            petals.Clear();

            model = new GameObject("Model").transform;
            model.SetParent(transform, false);

            var grid = petalMesh != null ? petalMesh : FlowerPetalMesh.Build();
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            // Halo: çiçeğin biraz arkasında, yaprakların altında kalan halka (yatay plane, normali +Y).
            haloRenderer = CreateRenderer("Halo", model, quad, haloMaterial, baseSortingOrder);
            haloRenderer.transform.localPosition = new Vector3(0f, -0.03f, 0f);
            haloRenderer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Orta: yatay plane; derinlik testiyle kapalıyken yapraklar örter.
            centerRenderer = CreateRenderer("Center", model, quad, centerMaterial, baseSortingOrder + 1);
            centerRenderer.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            centerRenderer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            int order = baseSortingOrder + 2;
            var rng = new System.Random(petalCount * 31 + layerCount);
            for (int layer = 0; layer < layerCount; layer++)
            {
                for (int i = 0; i < petalCount; i++)
                {
                    float az = PetalAzimuth(layer, i);
                    var go = new GameObject($"Petal L{layer}_{i}");
                    go.transform.SetParent(model, false);
                    // Yerel +X yaprağın uzandığı yöne baksın (XZ düzleminde az açısı).
                    go.transform.localRotation = Quaternion.Euler(0f, -az * Mathf.Rad2Deg, 0f);

                    var r = go.AddComponent<MeshRenderer>();
                    go.AddComponent<MeshFilter>().sharedMesh = grid;
                    // Gölge materyali aynı mesh'i ikinci kez çizer (son alt mesh tekrarlanır); kuyruğu yapraklardan sonradır.
                    r.sharedMaterials = petalShadowMaterial != null
                        ? new[] { petalMaterial, petalShadowMaterial }
                        : new[] { petalMaterial };
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = false;
                    r.sortingLayerName = sortingLayerName;
                    r.sortingOrder = order;

                    petals.Add(new Petal { Pivot = go.transform, Renderer = r, Layer = layer, Index = i, Seed = (float)rng.NextDouble() });
                }
            }

            // İplik tacı: iki halka (dış, iç), yapraklardan sonra ve üstte çizilir. Mesh'ler EnsureCoronaMeshes'te atanır.
            coronaRenderers = null;
            builtCoronaStrips = 0;
            if (coronaMaterial != null)
            {
                coronaRenderers = new MeshRenderer[2];
                string[] names = { "CoronaOuter", "CoronaInner" };
                for (int r = 0; r < 2; r++)
                {
                    var go = new GameObject(names[r]);
                    go.transform.SetParent(model, false);
                    go.AddComponent<MeshFilter>();
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = coronaShadowMaterial != null
                        ? new[] { coronaMaterial, coronaShadowMaterial }
                        : new[] { coronaMaterial };
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    mr.sortingLayerName = sortingLayerName;
                    mr.sortingOrder = order + 1 + r;
                    coronaRenderers[r] = mr;
                }

                builtCoronaStrips = coronaStrips;
                EnsureCoronaMeshes();
            }

            Apply();
        }

        // Halka mesh'leri kaydedilen asset değil, çalışma zamanında üretilir (şerit sayısı ve dönüklüğe göre ortak önbellek);
        // böylece prefab'a gömülü mesh referansı bozulmaz.
        private static readonly Dictionary<(int, float), Mesh> CoronaMeshes = new Dictionary<(int, float), Mesh>();

        private static Mesh GetCoronaMesh(int strips, float phase)
        {
            var key = (strips, phase);
            if (!CoronaMeshes.TryGetValue(key, out var mesh) || mesh == null)
            {
                mesh = FlowerCoronaMesh.Build(strips, phase);
                mesh.hideFlags = HideFlags.HideAndDontSave;
                CoronaMeshes[key] = mesh;
            }

            return mesh;
        }

        private void EnsureCoronaMeshes()
        {
            if (coronaMaterial == null || coronaRenderers == null || coronaRenderers.Length != 2) return;
            var rings = new[] { coronaOuter, coronaInner };
            for (int r = 0; r < 2; r++)
            {
                if (coronaRenderers[r] == null) continue;
                var filter = coronaRenderers[r].GetComponent<MeshFilter>();
                if (filter != null) filter.sharedMesh = GetCoronaMesh(coronaStrips, rings[r].phase);
            }
        }

        private float PetalAzimuth(int layer, int index)
        {
            float step = 360f * Mathf.Deg2Rad / petalCount;
            return index * step + (layer % 2 == 1 ? step * 0.5f : 0f);
        }

        private MeshRenderer CreateRenderer(string objName, Transform parent, Mesh mesh, Material mat, int order)
        {
            var go = new GameObject(objName);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingLayerName = sortingLayerName;
            r.sortingOrder = order;
            return r;
        }

        private void Apply()
        {
            if (!IsBuilt()) return;
            _block ??= new MaterialPropertyBlock();

            float over = Mathf.Max(0f, open - 1f);
            float under = Mathf.Max(0f, -open);
            float oc = Mathf.Clamp01(open);

            // Tüm çiçek: kendi ekseninde dönüş (açılırken çözülür, overshoot'ta ters yöne taşar), overshoot büyümesi,
            // anticipation büzülmesi ve uyarı titremesi.
            float twist = openTwist * (1f - open);
            model.localRotation = Quaternion.Euler(-viewTilt, 0f, 0f) * Quaternion.Euler(0f, twist, 0f);
            model.localScale = Vector3.one * Mathf.Max(0.1f, 1f + over * overshootScale - under * 0.6f);
            float tr = _warn * trembleAmplitude;
            model.localPosition = new Vector3(Mathf.Sin(_tremblePhase) * tr, Mathf.Cos(_tremblePhase * 1.31f) * tr, petalLength * depthBehind);

            // Rulo eğriliği sabit (yarıçap değişmez): kapalıyken sarılı kısım (1 - closedStraight) boyunca closedTurns tur atar.
            float closedTurnRate = closedTurns * Mathf.PI * 2f / Mathf.Max(0.1f, 1f - closedStraight);
            bool hasShape = petalMaterial != null && petalMaterial.HasProperty(ShapeId);
            bool hasRoundness = petalMaterial != null && petalMaterial.HasProperty(RoundnessId);
            float shape = hasShape ? petalMaterial.GetFloat(ShapeId) : 0f;
            float roundness = hasRoundness ? petalMaterial.GetFloat(RoundnessId) : 0f;
            foreach (var p in petals)
            {
                float prog = FlowerBloomMath.PetalProgress(oc, p.Layer, layerCount, p.Index, layerDelay, indexStagger);

                // Kapalıyken yaprak tabanları eksene toplanır (rulolar ortayı örter), açılırken çekirdeğin kenarına açılır.
                float baseRadius = coreRadius * Mathf.Lerp(closedBaseRadius, 0.9f, prog);
                float az = PetalAzimuth(p.Layer, p.Index);
                p.Pivot.localPosition = new Vector3(Mathf.Cos(az) * baseRadius, 0f, Mathf.Sin(az) * baseRadius);

                float pitchDeg = Mathf.Lerp(closedPitch, openPitch + p.Layer * innerPitchBonus, prog) - over * overshootPitch;
                float reach = Mathf.Lerp(closedStraight, 1f, prog);

                var cut = handCut ? FlowerCutMath.ForSeed(p.Seed) : FlowerCutMath.Cut.None;

                _block.Clear();
                _block.SetFloat(LengthId, petalLength * Mathf.Pow(layerLengthScale, p.Layer) * cut.Length);
                _block.SetFloat(WidthId, petalWidth * Mathf.Pow(layerWidthScale, p.Layer) * cut.Width);
                _block.SetFloat(TipSkewId, cut.TipSkew);
                // Yaprak şekli (damla) materyalden okunup bloğa yazılır: kesim gölgesi materyali aynı şekli buradan alır,
                // iki materyalin değeri birbirinden ayrılamaz.
                if (hasShape) _block.SetFloat(ShapeId, shape);
                if (hasRoundness) _block.SetFloat(RoundnessId, roundness);
                _block.SetFloat(PitchId, pitchDeg * Mathf.Deg2Rad);
                _block.SetFloat(ReachId, reach);
                _block.SetFloat(SoftId, reachSoftness);
                _block.SetFloat(TurnId, closedTurnRate);
                _block.SetFloat(BaseCurlId, openCurl * prog);
                _block.SetFloat(RollId, Mathf.Lerp(closedRoll, openRoll, prog));
                _block.SetFloat(OpenId, prog);
                _block.SetFloat(WarnId, _warn);
                _block.SetFloat(SeedId, p.Seed);
                p.Renderer.SetPropertyBlock(_block);
            }

            ApplyCorona();

            // Orta: disk shader'daki _CoreRadius (quad yarıçapının oranı) çiçek çekirdeğine oturacak şekilde ölçeklenir;
            // pop yayı ile 0 -> ~1,3 -> 1. Uyarıda hızlanan bir nabız (±%8).
            float fill = centerMaterial != null && centerMaterial.HasProperty(CoreRadiusId)
                ? Mathf.Max(0.05f, centerMaterial.GetFloat(CoreRadiusId))
                : 0.9f;
            float pulse = 1f + 0.08f * _warn * Mathf.Sin(_tremblePhase * 1.5f);
            // Kapalıyken küçük bir tomurcuk çekirdeği (budScale) görünür; açılınca sarıya dönüp pop yapar.
            float pop = _pop.Value;
            float popScale = pop <= 1f ? Mathf.Lerp(budScale, 1f, Mathf.Max(0f, pop)) : pop;
            float diameter = coreRadius * 2f / fill * popScale * pulse;
            centerRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);

            _block.Clear();
            _block.SetFloat(WarnId, _warn);
            _block.SetFloat(RevealId, pop);
            centerRenderer.SetPropertyBlock(_block);

            // Halka: açılışta yarıçap ease-out ile büyür, alfa söner. Uyarıda aynı renderer kesikli amber halkaya döner
            // (dalga ve uyarı hiçbir zaman üst üste gelmez).
            bool haloOn = _haloT >= 0f;
            bool warnRing = FlowerWarnMath.RingVisible(_warn, haloOn);
            haloRenderer.enabled = haloOn || warnRing;
            if (haloOn)
            {
                float e = 1f - (1f - _haloT) * (1f - _haloT);
                haloRenderer.transform.localScale = new Vector3(haloRadius * 2f, haloRadius * 2f, 1f);
                _block.Clear();
                _block.SetFloat(RingId, Mathf.Lerp(0.18f, 0.95f, e));
                _block.SetFloat(AlphaId, (1f - _haloT) * 0.9f);
                haloRenderer.SetPropertyBlock(_block);
            }
            else if (warnRing)
            {
                haloRenderer.transform.localScale = new Vector3(haloRadius * 2f, haloRadius * 2f, 1f);
                _block.Clear();
                _block.SetFloat(DashesId, warnRingDashes);
                _block.SetFloat(RingId, FlowerWarnMath.RingRadius01(warnRingRadius, haloRadius));
                _block.SetFloat(WidthId, warnRingWidth);
                _block.SetFloat(AlphaId, FlowerWarnMath.BlinkOn(_blinkClock, warnBlinkHz) ? 1f : 0f);
                haloRenderer.SetPropertyBlock(_block);
            }
        }

        // İplik tacı: açılma değerinden (ve uyarıdan) türeyen duruş (FlowerCoronaMath) her halkanın bloğuna yazılır;
        // bükme yaprakla aynı shader hesabı, yani eğrilik açıldıkça çözülür. Tamamen kapalıyken halkalar çizilmez.
        private void ApplyCorona()
        {
            if (coronaMaterial == null || coronaRenderers == null || coronaRenderers.Length != 2) return;

            var rings = new[] { coronaOuter, coronaInner };
            for (int r = 0; r < 2; r++)
            {
                var mr = coronaRenderers[r];
                var ring = rings[r];
                if (mr == null || ring == null) continue;

                var pose = FlowerCoronaMath.Evaluate(open, _warn, ring.openPitchDeg * Mathf.Deg2Rad,
                    ring.closedPitchDeg * Mathf.Deg2Rad, ring.openCurl, ring.closedCurl, coronaOpenStart);
                mr.enabled = pose.Visible;
                if (!pose.Visible) continue;

                _block.Clear();
                _block.SetFloat(LengthId, ring.length * pose.Length);
                _block.SetFloat(WidthId, ring.width);
                _block.SetFloat(PitchId, pose.Pitch);
                _block.SetFloat(BaseCurlId, pose.Curl);
                _block.SetFloat(RollId, 0.1f);
                _block.SetFloat(TurnId, 0f);
                _block.SetFloat(ReachId, 1f);
                _block.SetFloat(SoftId, reachSoftness);
                _block.SetFloat(BaseRadiusId, ring.baseRadius * pose.Radius);
                _block.SetFloat(BaseHeightId, ring.baseHeight * pose.Height);
                _block.SetFloat(WarnId, _warn);
                mr.SetPropertyBlock(_block);
            }
        }

        private static void Kill(GameObject go)
        {
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }
}
