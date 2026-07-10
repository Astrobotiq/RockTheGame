using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Karakter kıyafetinin (etek/pelerin) procedural olarak sallanmasını test etmek için 
    /// bağımsız (standalone) bir bileşen. Verlet entegrasyonu (Verlet Integration) kullanarak 
    /// gerçekçi bir zincir fiziği simüle eder.
    /// Sahnede Play modunda veya Edit modunda (ExecuteAlways sayesinde) bu objeyi 
    /// sürükleyerek hareketi ve salınımı test edebilirsiniz.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public class ProceduralSkirtTest : MonoBehaviour
    {
        [System.Serializable]
        public class SkirtSegment
        {
            public Transform Transform;
            [HideInInspector] public Vector2 CurrentPosition;
            [HideInInspector] public Vector2 PreviousPosition;
        }

        public enum SkirtUpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        [Header("Target Follow Options")]
        [Tooltip("Eteğin bağlanacağı / takip edeceği Transform. Boş bırakılırsa bu scriptin kendi transform'unu takip eder.")]
        [SerializeField] private Transform targetTransform;
        [Tooltip("Simülasyonun hangi döngüde güncelleneceğini seçin. Oyuncu FixedUpdate'de hareket ediyorsa FixedUpdate veya LateUpdate titremeyi önler.")]
        [SerializeField] private SkirtUpdateMode updateMode = SkirtUpdateMode.LateUpdate;

        [Header("Segment Configuration")]
        [Tooltip("Zinciri oluşturan parçaların listesi. Boş bırakıp 'Generate Segments' diyerek otomatik oluşturabilirsiniz.")]
        [SerializeField] private List<SkirtSegment> segments = new List<SkirtSegment>();
        
        [Range(3, 20)]
        [SerializeField] private int autoSegmentCount = 10;
        [SerializeField] private float topSegmentWidth = 0.5f;
        [SerializeField] private float bottomSegmentWidth = 1.5f;
        [SerializeField] private float segmentHeight = 0.2f;
        [SerializeField] private float verticalOffsetMultiplier = 0.85f; // Parçaların üst üste binme miktarı
        
        [Header("Visual Styling")]
        [SerializeField] private Gradient skirtGradient = new Gradient();
        [SerializeField] private Sprite segmentSprite; // Eğer boşsa default beyaz kare sprite'ı oluşturulur

        [Header("Physics Settings")]
        [SerializeField] private float segmentLength = 0.18f; // Segmentler arası sabit mesafe
        [SerializeField] private Vector2 gravity = new Vector2(0f, -9.81f);
        [SerializeField] private float airResistance = 2f; // Sönümlenme/Sürtünme (Drag)
        [Range(1, 20)]
        [SerializeField] private int constraintIterations = 8; // Bağ kısıtı çözüm tekrarı (Daha yüksek = Daha sert zincir)

        [Header("Angle Constraints")]
        [Tooltip("Aşağı segmentlerin yukarı bükülmesini/kalkmasını engeller.")]
        [SerializeField] private bool enableAngleConstraint = true;
        [Range(0f, 180f)]
        [Tooltip("En üst segmentin dikey aşağı yöne göre yapabileceği maksimum sapma açısı (Derece).")]
        [SerializeField] private float topMaxSwingAngle = 15f;
        [Range(0f, 180f)]
        [Tooltip("En alt segmentin dikey aşağı yöne göre yapabileceği maksimum sapma açısı (Derece).")]
        [SerializeField] private float bottomMaxSwingAngle = 80f;

        [Header("Pixel Snap Settings")]
        [SerializeField] private bool enablePixelSnap = false;
        [SerializeField] private float pixelsPerUnit = 16f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.green;

        private Vector2 _lastPosition;

        private Vector2 TargetPosition => targetTransform != null ? (Vector2)targetTransform.position : (Vector2)transform.position;

        private void OnEnable()
        {
            _lastPosition = TargetPosition;
            InitializePositions();
        }

        private void Start()
        {
            if (segments.Count == 0 && Application.isPlaying)
            {
                GenerateSkirtSegments();
            }
            InitializePositions();
        }

        private void InitializePositions()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] == null || segments[i].Transform == null) continue;
                
                segments[i].CurrentPosition = segments[i].Transform.position;
                segments[i].PreviousPosition = segments[i].Transform.position;
            }
        }

        private void Update()
        {
            // Edit modunda (oyun çalışmıyorken) sürüklemeyi anlık görmek için her zaman Update kullanırız.
            if (!Application.isPlaying)
            {
                float dt = Time.deltaTime;
                if (dt > 0.1f) dt = 0.1f;
                if (dt <= 0f) dt = 0.016f;
                SimulatePhysics(dt);
                return;
            }

            if (updateMode == SkirtUpdateMode.Update)
            {
                float dt = Time.deltaTime;
                if (dt > 0.1f) dt = 0.1f;
                if (dt <= 0f) dt = 0.016f;
                SimulatePhysics(dt);
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying && updateMode == SkirtUpdateMode.FixedUpdate)
            {
                SimulatePhysics(Time.fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && updateMode == SkirtUpdateMode.LateUpdate)
            {
                float dt = Time.deltaTime;
                if (dt > 0.1f) dt = 0.1f;
                if (dt <= 0f) dt = 0.016f;
                SimulatePhysics(dt);
            }
        }

        private void SimulatePhysics(float deltaTime)
        {
            if (segments.Count == 0) return;

            // 1. Verlet Integration: Hız hesaplama ve kuvvetlerin (Yerçekimi) uygulanması
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment.Transform == null) continue;

                // Mevcut konum ile önceki konum farkı bize hızı verir. Sürtünme uyguluyoruz.
                Vector2 velocity = (segment.CurrentPosition - segment.PreviousPosition) * (1f - airResistance * deltaTime);
                
                segment.PreviousPosition = segment.CurrentPosition;
                // Yeni pozisyon = Eski pozisyon + Hız + (İvme * dt^2)
                segment.CurrentPosition += velocity + gravity * (deltaTime * deltaTime);
            }

            // 2. Çapa Noktasını Güncelle (En üst segment her zaman hedef pozisyona bağlıdır)
            if (segments[0].Transform != null)
            {
                segments[0].CurrentPosition = TargetPosition;
            }

            // 3. Bağ Kısıtlarını Çözme (Distance & Angle Constraints):
            // Her segment bir önceki segment ile tam olarak 'segmentLength' mesafede olmalı ve açısı sınırlandırılmalı.
            for (int iteration = 0; iteration < constraintIterations; iteration++)
            {
                // En üst parça sabitlendiği için 1. indexten başlıyoruz
                for (int i = 1; i < segments.Count; i++)
                {
                    var parent = segments[i - 1];
                    var child = segments[i];

                    if (parent.Transform == null || child.Transform == null) continue;

                    // A. Mesafe Kısıtı
                    Vector2 delta = child.CurrentPosition - parent.CurrentPosition;
                    float distance = delta.magnitude;

                    if (distance > 0.0001f)
                    {
                        float difference = segmentLength - distance;
                        // Hata payını iki uca dağıtıyoruz (Eğer parent en üst parça ise onu oynatmamak için düzeltmeyi sadece child'a yükleriz)
                        Vector2 correction = (delta / distance) * difference;

                        if (i == 1)
                        {
                            // En üst segment sabit olduğu için tüm düzeltmeyi altındakine uygula
                            child.CurrentPosition += correction;
                        }
                        else
                        {
                            // Düzeltmeyi iki segment arasında yarı yarıya paylaş
                            parent.CurrentPosition -= correction * 0.5f;
                            child.CurrentPosition += correction * 0.5f;
                        }
                    }

                    // B. Açısal Kısıt (Aşağı parçaların yukarı kalkmasını / katlanmasını önler)
                    if (enableAngleConstraint)
                    {
                        Vector2 currentDelta = child.CurrentPosition - parent.CurrentPosition;
                        float dist = currentDelta.magnitude;
                        if (dist > 0.0001f)
                        {
                            float angle = Mathf.Atan2(currentDelta.y, currentDelta.x) * Mathf.Rad2Deg;
                            // Dikey aşağı yön (-90 derece) ile arasındaki sapmayı bul (-180 ile 180 derece arasında)
                            float deviation = Mathf.DeltaAngle(-90f, angle);
                            
                            // Zincir boyunca yukarıdan aşağıya doğru izin verilen sapma açısını yumuşakça artırırız
                            float t = (float)i / (segments.Count - 1);
                            float segmentMaxAngle = Mathf.Lerp(topMaxSwingAngle, bottomMaxSwingAngle, t);
                            
                            // Sapmayı sınırla
                            float clampedDeviation = Mathf.Clamp(deviation, -segmentMaxAngle, segmentMaxAngle);
                            
                            // Yeni açıyı ve yönü hesaplayıp pozisyonu güncelle
                            float clampedAngle = -90f + clampedDeviation;
                            float rad = clampedAngle * Mathf.Deg2Rad;
                            Vector2 clampedDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                            
                            child.CurrentPosition = parent.CurrentPosition + clampedDir * dist;
                        }
                    }
                }
            }

            // 4. Transform Pozisyonlarını ve Rotasyonlarını Güncelle
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment.Transform == null) continue;

                Vector3 targetWorldPos = segment.CurrentPosition;

                // Pixel Snap seçeneği aktifse piksellere hizala (Pixel Art estetiğini korumak için)
                if (enablePixelSnap && pixelsPerUnit > 0)
                {
                    float snapX = Mathf.Round(targetWorldPos.x * pixelsPerUnit) / pixelsPerUnit;
                    float snapY = Mathf.Round(targetWorldPos.y * pixelsPerUnit) / pixelsPerUnit;
                    segment.Transform.position = new Vector3(snapX, snapY, segment.Transform.position.z);
                }
                else
                {
                    segment.Transform.position = new Vector3(targetWorldPos.x, targetWorldPos.y, segment.Transform.position.z);
                }

                // Rotasyon istemiyoruz, parçalar sadece sağa/sola kaysın ve düz dursunlar.
                segment.Transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Test etmeyi kolaylaştırmak için otomatik olarak hiyerarşide 
        /// çocuk objeler oluşturur ve SpriteRenderer ekler.
        /// </summary>
        [ContextMenu("Generate Segments")]
        public void GenerateSkirtSegments()
        {
            // Eski oluşturulmuş çocuk objeleri temizle
            ClearGeneratedSegments();

            segments.Clear();

            // Default sprite yoksa basit bir beyaz 2D kare oluştur/bul
            Sprite spriteToUse = segmentSprite;
            if (spriteToUse == null)
            {
                spriteToUse = CreateDefaultSprite();
            }

            Vector3 spawnPos = TargetPosition;

            for (int i = 0; i < autoSegmentCount; i++)
            {
                GameObject segmentObj = new GameObject($"Skirt_Segment_{i}");
                segmentObj.transform.parent = this.transform;
                
                // Aşağı doğru sıralı yerleşim
                float verticalOffset = i * segmentHeight * verticalOffsetMultiplier;
                segmentObj.transform.position = spawnPos + Vector3.down * verticalOffset;

                // Oran (0 en üst, 1 en alt segment)
                float t = (float)i / (Mathf.Max(1, autoSegmentCount - 1));
                float currentWidth = Mathf.Lerp(topSegmentWidth, bottomSegmentWidth, t);

                // Görsel bileşenler
                var spriteRenderer = segmentObj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteToUse;
                
                // Pixel art için ayarlar
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = new Vector2(currentWidth, segmentHeight);
                
                // Sıralama (en üstteki parça en önde veya arkada görünecek şekilde ayarlanabilir)
                spriteRenderer.sortingOrder = 100 - i;

                // Gradient rengini uygula
                spriteRenderer.color = skirtGradient.Evaluate(t);

                // Listeye ekle
                SkirtSegment newSegment = new SkirtSegment
                {
                    Transform = segmentObj.transform,
                    CurrentPosition = segmentObj.transform.position,
                    PreviousPosition = segmentObj.transform.position
                };
                segments.Add(newSegment);
            }

            // Segment mesafesini otomatik hesapla
            segmentLength = segmentHeight * verticalOffsetMultiplier;
            _lastPosition = TargetPosition;
        }

        [ContextMenu("Clear Segments")]
        public void ClearGeneratedSegments()
        {
            // Skirt_Segment_ adıyla başlayan çocukları sil
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Skirt_Segment_"))
                {
                    if (Application.isPlaying)
                        Destroy(child);
                    else
                        DestroyImmediate(child);
                }
            }
            segments.Clear();
        }

        private Sprite CreateDefaultSprite()
        {
            // Programatik olarak 4x4 beyaz bir doku oluşturup Sprite'a dönüştürür.
            Texture2D tex = new Texture2D(4, 4);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || segments == null || segments.Count == 0) return;

            Gizmos.color = gizmoColor;
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] == null || segments[i].Transform == null) continue;

                Gizmos.DrawWireSphere(segments[i].Transform.position, 0.05f);

                if (i < segments.Count - 1 && segments[i + 1] != null && segments[i + 1].Transform != null)
                {
                    Gizmos.DrawLine(segments[i].Transform.position, segments[i + 1].Transform.position);
                }
            }
        }
    }
}
