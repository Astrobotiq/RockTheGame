using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Ana menünün hareketi (docs/ui-research/menu-juice.md). Hepsi yaylarla (<see cref="UiSpring"/>), unscaled zaman,
    /// yalnızca transform/renk değiştirir (mesh yeniden üretilmez):
    /// <list type="bullet">
    /// <item>Poz: her öğenin sabit bir sahne duruşu vardır (delik, başlık, kirişler); seçim değişince oraya yaylanıp yerleşir.</item>
    /// <item>İmleç: sarı levha satırlar arasında kayar (gerilir), gölgesi "çakılır", açık halka dönerken hızlanır.</item>
    /// <item>Etiket: seçili yazı büyür ve sağa kayar, diğerleri soluklaşır.</item>
    /// <item>Delik: halkalar sürekli nefes alır ve derinlik hissiyle hafif sürüklenir.</item>
    /// <item>Giriş: ~0,6 sn içinde parçalar sırayla yerine gelir; bu sürede gezinme kilitli.</item>
    /// </list>
    /// Süre/miktar değerleri başlangıç önerisi: gamepad ile ayarlanır.
    /// </summary>
    [AddComponentMenu("RockTheGame/UI/Menu Motion")]
    public sealed class UiMenuMotion : MonoBehaviour
    {
        /// <summary>Bir menü öğesi seçiliyken sahnenin duruşu. Konum/açı değerleri başlangıçtaki (Play) düzene göre farktır.</summary>
        [Serializable]
        public struct Pose
        {
            [Tooltip("Delik grubunun ölçeği (1 = normal).")] public float HoleScale;
            [Tooltip("İç halkaların ek büyümesi: dıştan içe 0'dan bu değere doğrusal artar (+ içe odaklı, - dışa).")] public float BandFocus;
            [Tooltip("Başlığın açısı (derece, + saat yönünün tersi).")] public float TitleRotation;
            [Tooltip("Başlığın dikey kayması (canvas birimi, + yukarı).")] public float TitleY;
            [Tooltip("Yatay kirişin uzaması (canvas birimi).")] public float BeamHStretch;
            [Tooltip("Yatay kirişin açısı (derece).")] public float BeamHRotation;
            [Tooltip("Dikey kirişin yatay kayması (canvas birimi).")] public float BeamVShift;
            [Tooltip("Dikey kirişin açısı (derece).")] public float BeamVRotation;
        }

        /// <summary>Play ev pozu; multiplayer deliğe yaklaşır ve her şey açılır; quit geri çekilir.</summary>
        public static Pose[] DefaultPoses() => new[]
        {
            // play: ev pozu
            new Pose { HoleScale = 1f },
            // multiplayer: delik yaklaşır, iç halkalar öne çıkar, başlık yatar ve yükselir, kirişler uzar
            new Pose { HoleScale = 1.12f, BandFocus = 0.10f, TitleRotation = -5f, TitleY = 14f, BeamHStretch = 420f, BeamHRotation = -1.6f, BeamVShift = 28f, BeamVRotation = 0.8f },
            // options (şu an seçilemez): hafif geri
            new Pose { HoleScale = 0.96f, BandFocus = -0.04f, TitleRotation = 2f, TitleY = -6f, BeamHStretch = -150f, BeamHRotation = 0.8f, BeamVShift = -18f, BeamVRotation = -0.5f },
            // quit: delik uzaklaşır, başlık öne yatar ve düşer, kirişler kısalır
            new Pose { HoleScale = 0.84f, BandFocus = -0.08f, TitleRotation = 6f, TitleY = -16f, BeamHStretch = -380f, BeamHRotation = 2.2f, BeamVShift = -44f, BeamVRotation = -1.4f }
        };

        [Header("Parçalar (MainMenuBuilder bağlar)")]
        [SerializeField] private Selectable[] items;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private RectTransform hole;
        [SerializeField] private RectTransform[] hookBands;
        [SerializeField] private RectTransform title;
        [SerializeField] private RectTransform beamH;
        [SerializeField] private RectTransform beamV;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private RectTransform cursorShadow;
        [SerializeField] private RectTransform cursorRing;
        [SerializeField] private CanvasGroup lockGroup;

        [Header("Ayarlar")]
        [Tooltip("Öğe sırasıyla pozlar; boşsa DefaultPoses.")]
        [SerializeField] private Pose[] poses;
        [Tooltip("Halka dalgası: her iç halkanın hedefe gitmeye başlama gecikmesi (sn).")]
        [SerializeField, Range(0f, 0.1f)] private float rippleDelay = 0.035f;
        [Tooltip("Boştaki halka nefesi (ölçek oranı); 0 = kapalı.")]
        [SerializeField, Range(0f, 0.1f)] private float idleBreath = 0.03f;
        [Tooltip("Boştaki halka sürüklenmesi (canvas birimi, en içteki halka için); 0 = kapalı.")]
        [SerializeField, Range(0f, 30f)] private float idleDrift = 9f;
        [Tooltip("Seçili etiketin sağa kayması (canvas birimi).")]
        [SerializeField, Range(0f, 60f)] private float labelShift = 28f;
        [SerializeField, Range(0f, 0.3f)] private float labelGrow = 0.08f;
        [Tooltip("Seçilmeyen etiketlerin görünürlüğü.")]
        [SerializeField, Range(0.3f, 1f)] private float labelDim = 0.7f;
        [Tooltip("Giriş animasyonunda gezinmenin kilitli kaldığı süre (sn).")]
        [SerializeField, Range(0f, 1.5f)] private float entranceLock = 0.6f;

        private UiSpring _holeScale, _titleRot, _titleY, _beamHStretch, _beamHRot, _beamVShift, _beamVRot;
        private UiSpring _cursorY, _cursorEnter, _shadowKick, _spin;
        private UiSpring[] _bandScale, _sel, _enter;
        private float[] _bandWait, _bandTarget, _enterWait;
        private Color[] _labelBase;
        private float _baseBeamHWidth, _baseBeamVX, _ringAngle, _age, _cursorWait;
        private Quaternion _baseTitleRot, _baseBeamHRot, _baseBeamVRot;
        private Vector2 _baseTitlePos, _shadowBase;
        private Vector2[] _labelPos;
        private int _current = -1;
        private bool _locked;

        public int CurrentPose => _current;

        public void Wire(Selectable[] menuItems, TMP_Text[] itemLabels, RectTransform holeRoot, RectTransform[] bands,
            RectTransform titleGroup, RectTransform horizontalBeam, RectTransform verticalBeam,
            RectTransform cursorRoot, RectTransform cursorShadowRect, RectTransform cursorRingRoot, CanvasGroup group)
        {
            items = menuItems;
            labels = itemLabels;
            hole = holeRoot;
            hookBands = bands;
            title = titleGroup;
            beamH = horizontalBeam;
            beamV = verticalBeam;
            cursor = cursorRoot;
            cursorShadow = cursorShadowRect;
            cursorRing = cursorRingRoot;
            lockGroup = group;
            poses = DefaultPoses();
        }

        private void OnEnable()
        {
            _holeScale = new UiSpring(150f, 9f);
            _titleRot = new UiSpring(170f, 8f);
            _titleY = new UiSpring(200f, 12f);
            _beamHStretch = new UiSpring(160f, 8f);
            _beamHRot = new UiSpring(190f, 9f);
            _beamVShift = new UiSpring(170f, 8f);
            _beamVRot = new UiSpring(190f, 9f);
            _cursorY = new UiSpring(320f, 20f);
            _cursorEnter = new UiSpring(220f, 18f);
            _shadowKick = new UiSpring(260f, 14f);
            _spin = new UiSpring(40f, 9f);

            int n = hookBands != null ? hookBands.Length : 0;
            _bandScale = new UiSpring[n];
            _bandWait = new float[n];
            _bandTarget = new float[n];
            for (int i = 0; i < n; i++) _bandScale[i] = new UiSpring(210f, 10f);

            int m = items != null ? items.Length : 0;
            _sel = new UiSpring[m];
            _enter = new UiSpring[m];
            _enterWait = new float[m];
            _labelBase = new Color[m];
            _labelPos = new Vector2[m];
            for (int i = 0; i < m; i++)
            {
                _sel[i] = new UiSpring(260f, 16f);
                _enter[i] = new UiSpring(200f, 18f);
                if (labels != null && i < labels.Length && labels[i] != null)
                {
                    _labelBase[i] = labels[i].color;
                    _labelPos[i] = labels[i].rectTransform.anchoredPosition;
                }
            }

            if (title != null) { _baseTitleRot = title.localRotation; _baseTitlePos = title.anchoredPosition; }
            if (beamH != null) { _baseBeamHWidth = beamH.sizeDelta.x; _baseBeamHRot = beamH.localRotation; }
            if (beamV != null) { _baseBeamVX = beamV.anchoredPosition.x; _baseBeamVRot = beamV.localRotation; }
            if (cursorShadow != null) _shadowBase = cursorShadow.anchoredPosition;

            _current = -1;
            _age = 0f;
            GoTo(0, true);
            PlayEntrance();
        }

        private void OnDisable()
        {
            if (hole != null) hole.localScale = Vector3.one;
            if (hookBands != null)
                foreach (var b in hookBands)
                    if (b != null) { b.localScale = Vector3.one; b.anchoredPosition = Vector2.zero; }
            if (lockGroup != null) lockGroup.interactable = true;
        }

        private void Update()
        {
            var system = EventSystem.current;
            GameObject selected = system != null ? system.currentSelectedGameObject : null;
            int index = IndexOf(selected);
            if (index >= 0 && index != _current && !_locked) GoTo(index, false);
            Apply(Time.unscaledDeltaTime);
        }

        private int IndexOf(GameObject go)
        {
            for (int i = 0; go != null && items != null && i < items.Length; i++)
                if (items[i] != null && items[i].gameObject == go) return i;
            return -1;
        }

        /// <summary><paramref name="index"/>. öğenin pozuna git; <paramref name="snap"/> animasyonsuz oturtur.</summary>
        public void GoTo(int index, bool snap)
        {
            var list = poses != null && poses.Length > 0 ? poses : DefaultPoses();
            Pose pose = list[Mathf.Clamp(index, 0, list.Length - 1)];
            int previous = _current;
            _current = index;

            _holeScale.Target = pose.HoleScale;
            _titleRot.Target = pose.TitleRotation;
            _titleY.Target = pose.TitleY;
            _beamHStretch.Target = pose.BeamHStretch;
            _beamHRot.Target = pose.BeamHRotation;
            _beamVShift.Target = pose.BeamVShift;
            _beamVRot.Target = pose.BeamVRotation;

            int n = _bandScale.Length;
            for (int i = 0; i < n; i++)
            {
                _bandTarget[i] = n > 1 ? pose.BandFocus * i / (n - 1) : 0f;
                _bandWait[i] = snap ? 0f : i * rippleDelay;
                if (snap) _bandScale[i].Target = _bandTarget[i];
            }

            for (int i = 0; i < _sel.Length; i++) _sel[i].Target = i == index ? 1f : 0f;
            _cursorY.Target = ItemY(index);

            if (snap)
            {
                SnapAll();
                Apply(0f);
                return;
            }

            int dir = index >= previous ? 1 : -1;
            _shadowKick.Value = 1.2f;
            _spin.Value = dir * 640f;
        }

        private float ItemY(int index) =>
            items != null && index >= 0 && index < items.Length && items[index] != null
                ? ((RectTransform)items[index].transform).anchoredPosition.y
                : 0f;

        private void SnapAll()
        {
            foreach (var s in new[] { _holeScale, _titleRot, _titleY, _beamHStretch, _beamHRot, _beamVShift, _beamVRot, _cursorY, _cursorEnter, _shadowKick, _spin })
            {
                s.Value = s.Target;
                s.Velocity = 0f;
            }

            foreach (var s in _bandScale) { s.Value = s.Target; s.Velocity = 0f; }
            foreach (var s in _sel) { s.Value = s.Target; s.Velocity = 0f; }
            foreach (var s in _enter) { s.Value = s.Target; s.Velocity = 0f; }
        }

        /// <summary>Açılış: parçalar uzaktan gelir, satırlar sırayla kayar, imleç sonra açılır. Gezinme kısa süre kilitli.</summary>
        private void PlayEntrance()
        {
            _locked = entranceLock > 0f;
            if (lockGroup != null) lockGroup.interactable = !_locked;

            _holeScale.Value = 0.45f;
            _titleY.Value += 170f;
            _titleRot.Value += -16f;
            _beamHStretch.Value -= _baseBeamHWidth * 0.9f;
            _beamVShift.Value -= 160f;
            for (int i = 0; i < _bandScale.Length; i++)
            {
                _bandScale[i].Value = -1f;
                _bandScale[i].Target = -1f;
                _bandWait[i] = 0.12f + i * 0.05f;
            }

            for (int i = 0; i < _enter.Length; i++)
            {
                _enter[i].Value = 1f;
                _enter[i].Target = 1f;
                _enterWait[i] = 0.2f + i * 0.06f;
            }

            _cursorEnter.Value = 1f;
            _cursorEnter.Target = 1f;
            _cursorWait = 0.2f + _enter.Length * 0.06f;
        }

        private void Apply(float dt)
        {
            _age += dt;
            if (_locked && _age >= entranceLock)
            {
                _locked = false;
                if (lockGroup != null) lockGroup.interactable = true;
            }

            float idleBlend = Mathf.Clamp01((_age - 0.5f) / 1f);
            ApplyPose(dt, idleBlend);
            ApplyItems(dt);
            ApplyCursor(dt);
        }

        private void ApplyPose(float dt, float idleBlend)
        {
            float t = _age;
            _holeScale.Step(dt);
            if (hole != null) hole.localScale = Vector3.one * _holeScale.Value;

            int n = _bandScale.Length;
            for (int i = 0; i < n; i++)
            {
                if (_bandWait[i] > 0f)
                {
                    _bandWait[i] -= dt;
                    if (_bandWait[i] <= 0f) _bandScale[i].Target = _bandTarget[i];
                }

                _bandScale[i].Step(dt);
                if (hookBands[i] == null) continue;

                float depth = n > 1 ? i / (float)(n - 1) : 0f;
                float breath = idleBreath * idleBlend * Mathf.Sin(t * 1.3f - i * 0.55f);
                hookBands[i].localScale = Vector3.one * (1f + _bandScale[i].Value + breath);
                // Derinlik: iç halkalar daha çok sürüklenir, her biri biraz farklı fazda.
                float a = t * 0.8f + i * 0.5f;
                hookBands[i].anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a * 1.2f)) * (idleDrift * idleBlend * depth);
            }

            _titleRot.Step(dt);
            _titleY.Step(dt);
            if (title != null)
            {
                title.localRotation = _baseTitleRot * Quaternion.Euler(0f, 0f, _titleRot.Value);
                title.anchoredPosition = _baseTitlePos + new Vector2(0f, _titleY.Value);
            }

            _beamHStretch.Step(dt);
            _beamHRot.Step(dt);
            if (beamH != null)
            {
                beamH.sizeDelta = new Vector2(Mathf.Max(0f, _baseBeamHWidth + _beamHStretch.Value), beamH.sizeDelta.y);
                beamH.localRotation = _baseBeamHRot * Quaternion.Euler(0f, 0f, _beamHRot.Value);
            }

            _beamVShift.Step(dt);
            _beamVRot.Step(dt);
            if (beamV != null)
            {
                beamV.anchoredPosition = new Vector2(_baseBeamVX + _beamVShift.Value, beamV.anchoredPosition.y);
                beamV.localRotation = _baseBeamVRot * Quaternion.Euler(0f, 0f, _beamVRot.Value);
            }
        }

        private void ApplyItems(float dt)
        {
            for (int i = 0; i < _sel.Length; i++)
            {
                if (labels == null || i >= labels.Length || labels[i] == null) continue;

                if (_enterWait[i] > 0f)
                {
                    _enterWait[i] -= dt;
                    if (_enterWait[i] <= 0f) _enter[i].Target = 0f;
                }

                _sel[i].Step(dt);
                _enter[i].Step(dt);

                float s = _sel[i].Value;
                float e = Mathf.Max(0f, _enter[i].Value);
                var rt = labels[i].rectTransform;
                rt.anchoredPosition = _labelPos[i] + new Vector2(labelShift * s - 360f * e, 0f);
                rt.localScale = Vector3.one * (1f + labelGrow * s);

                Color c = _labelBase[i];
                bool live = items[i] != null && items[i].interactable;
                float alpha = live ? Mathf.Lerp(labelDim, 1f, Mathf.Clamp01(s)) : labelDim * 0.5f; // pasif: seçili olmayan etkin satırdan açıkça soluk
                c.a = _labelBase[i].a * alpha * (1f - Mathf.Clamp01(e));
                if (labels[i].color != c) labels[i].color = c;
            }
        }

        private void ApplyCursor(float dt)
        {
            if (cursor == null) return;

            if (_cursorWait > 0f)
            {
                _cursorWait -= dt;
                if (_cursorWait <= 0f) _cursorEnter.Target = 0f;
            }

            _cursorY.Step(dt);
            _cursorEnter.Step(dt);
            _shadowKick.Step(dt);
            _spin.Step(dt);

            // Levha: satır arasında kayarken hareket yönünde gerilir; girişte soldan açılır.
            float stretch = Mathf.Min(0.18f, Mathf.Abs(_cursorY.Velocity) / 6000f);
            float open = 1f - Mathf.Clamp01(_cursorEnter.Value);
            cursor.anchoredPosition = new Vector2(cursor.anchoredPosition.x, _cursorY.Value);
            cursor.localScale = new Vector3(Mathf.Max(0.0001f, (1f - stretch * 0.3f) * open), 1f + stretch, 1f);

            if (cursorShadow != null) cursorShadow.anchoredPosition = _shadowBase * (1f + _shadowKick.Value);

            if (cursorRing != null)
            {
                _ringAngle += (110f + _spin.Value) * dt;
                cursorRing.localRotation = Quaternion.Euler(0f, 0f, -_ringAngle);
            }

        }
    }
}
