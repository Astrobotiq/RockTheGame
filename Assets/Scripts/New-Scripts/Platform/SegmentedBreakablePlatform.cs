using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Death;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Oyuncunun bastığı noktadaki kutudan başlayarak içten dışa doğru sprite animasyonuyla parçalanan 
    /// ve kırılmayı dalga halinde komşu bloklara yayan platform bileşeni.
    /// </summary>
    public class SegmentedBreakablePlatform : MonoBehaviour, IResettable
    {
        [Header("Settings")]
        [Tooltip("Sadece Player'ın bulunduğu Layer'ı seçin.")]
        [SerializeField] private LayerMask playerLayer;
        [Tooltip("Komşu bloklara kırılma dalgasının yayılma süresi (saniye).")]
        [SerializeField] private float propagationDelay = 0.08f;
        [Tooltip("Platformun kırıldıktan sonra yeniden doğma süresi (saniye).")]
        [SerializeField] private float respawnTime = 3f;

        [Header("Block Generation Settings")]
        [Tooltip("Platformdaki toplam blok sayısı.")]
        [SerializeField] private int blockCount = 5;
        [Tooltip("Her bir bloğun genişliği (birim cinsinden).")]
        [SerializeField] private float blockWidth = 1f;
        [Tooltip("Dinamik oluşturma için Blok Prefabı (Boş bırakılırsa var olan child bloklar kullanılır).")]
        [SerializeField] private BreakableBlockUnit blockPrefab;
        [Tooltip("Blokların yerleştirileceği parent Transform (Boş ise bu Transform kullanılır).")]
        [SerializeField] private Transform blocksHolder;

        [Header("Vibration")]
        [SerializeField] private PlatformVibrationSettingsSO vibrationSettings;

        [Header("VFX Prefabs")]
        [SerializeField] private ParticleSystem breakVFXPrefab;
        [SerializeField] private ParticleSystem reformVFXPrefab;

        [Header("Runtime Blocks")]
        [SerializeField] private List<BreakableBlockUnit> blocks = new List<BreakableBlockUnit>();

        private bool _isTriggered;
        private CancellationTokenSource _breakCts;

        private void Awake()
        {
            if (blocksHolder == null) blocksHolder = transform;

            SetupBlocks();
        }

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            CancelBreakSequence();
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Unregister(this);
            }
        }

        /// <summary>
        /// Inspector veya prefab yapısına göre blokların dizilimini sağlar.
        /// </summary>
        private void SetupBlocks()
        {
            // Eğer elle atanmış child bloklar yoksa veya listemiz boşsa mevcut child'ları topla
            if (blocks == null) blocks = new List<BreakableBlockUnit>();

            blocks.RemoveAll(b => b == null);

            if (blocks.Count == 0)
            {
                BreakableBlockUnit[] childBlocks = blocksHolder.GetComponentsInChildren<BreakableBlockUnit>();
                if (childBlocks.Length > 0)
                {
                    blocks.AddRange(childBlocks);
                }
            }

            // Eğer sahne/prefab içinde child blok yoksa ve Prefab atanmışsa çalışma zamanında dinamik üret
            if (blocks.Count == 0 && blockPrefab != null && blockCount > 0)
            {
                float totalWidth = blockCount * blockWidth;
                float startX = -totalWidth / 2f + blockWidth / 2f;

                for (int i = 0; i < blockCount; i++)
                {
                    Vector3 localPos = new Vector3(startX + (i * blockWidth), 0f, 0f);
                    BreakableBlockUnit unit = Instantiate(blockPrefab, blocksHolder);
                    unit.transform.localPosition = localPos;
                    unit.gameObject.name = $"BlockUnit_{i}";
                    blocks.Add(unit);
                }
            }
        }

        /// <summary>
        /// Editör içerisinde (Play mode dışındayken) blokları temizleyip Child Obje olarak yeniden üretir.
        /// Seviye tasarımında doğrudan sahne üzerinde görünmesini sağlar.
        /// </summary>
        [ContextMenu("Rebuild Blocks in Editor")]
        public void GenerateBlocksInEditor()
        {
#if UNITY_EDITOR
            if (blocksHolder == null) blocksHolder = transform;

            // Var olan tüm child blokları temizle
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in blocksHolder)
            {
                if (child.GetComponent<BreakableBlockUnit>() != null)
                {
                    childrenToDestroy.Add(child.gameObject);
                }
            }

            for (int i = childrenToDestroy.Count - 1; i >= 0; i--)
            {
                UnityEditor.Undo.DestroyObjectImmediate(childrenToDestroy[i]);
            }

            if (blocks == null) blocks = new List<BreakableBlockUnit>();
            blocks.Clear();

            if (blockCount <= 0) return;

            float totalWidth = blockCount * blockWidth;
            float startX = -totalWidth / 2f + blockWidth / 2f;

            for (int i = 0; i < blockCount; i++)
            {
                Vector3 localPos = new Vector3(startX + (i * blockWidth), 0f, 0f);
                BreakableBlockUnit unit = null;

                if (blockPrefab != null)
                {
                    unit = (BreakableBlockUnit)UnityEditor.PrefabUtility.InstantiatePrefab(blockPrefab, blocksHolder);
                }
                else
                {
                    GameObject go = new GameObject($"BlockUnit_{i}", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(BreakableBlockUnit));
                    go.transform.SetParent(blocksHolder);
                    unit = go.GetComponent<BreakableBlockUnit>();
                }

                if (unit != null)
                {
                    unit.transform.localPosition = localPos;
                    unit.gameObject.name = $"BlockUnit_{i}";
                    UnityEditor.Undo.RegisterCreatedObjectUndo(unit.gameObject, "Create Block Unit");
                    blocks.Add(unit);
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
            Debug.Log($"[SegmentedBreakablePlatform] {blockCount} adet blok editörde başarıyla oluşturuldu.", this);
#endif
        }

        /// <summary>
        /// Parçalanma animasyonunu ve yayılmasını manuel/test olarak tetikler.
        /// </summary>
        /// <param name="initialBlockIndex">-1 verilirse platformun ortasından başlar.</param>
        [ContextMenu("Test Trigger Break Sequence")]
        public void TriggerBreakFromEditor(int initialBlockIndex = -1)
        {
            if (blocks == null || blocks.Count == 0)
            {
                SetupBlocks();
            }

            if (blocks.Count == 0)
            {
                Debug.LogWarning("[SegmentedBreakablePlatform] Kırılacak blok bulunamadı!", this);
                return;
            }

            if (initialBlockIndex < 0 || initialBlockIndex >= blocks.Count)
            {
                initialBlockIndex = blocks.Count / 2;
            }

            StartBreakSequence(initialBlockIndex);
        }

        private void FixedUpdate()
        {
            if (_isTriggered || blocks == null || blocks.Count == 0) return;

            // Oyuncunun platform bloklarından birinin üzerine basıp basmadığını kontrol et
            for (int i = 0; i < blocks.Count; i++)
            {
                BreakableBlockUnit block = blocks[i];
                if (block == null || block.IsBroken || block.BlockCollider == null) continue;

                Vector2 boxCenter = (Vector2)block.transform.position + block.BlockCollider.offset + (Vector2.up * 0.05f);
                Vector2 boxSize = block.BlockCollider.bounds.size;
                boxSize.y += 0.05f;

                Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, playerLayer);
                if (hit != null)
                {
                    StartBreakSequence(i);
                    break;
                }
            }
        }

        public void StartBreakSequence(int initialBlockIndex)
        {
            CancelBreakSequence();
            _breakCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            BreakSequenceAsync(initialBlockIndex, _breakCts.Token).Forget();
        }

        private void CancelBreakSequence()
        {
            if (_breakCts != null)
            {
                _breakCts.Cancel();
                _breakCts.Dispose();
                _breakCts = null;
            }
        }

        private async UniTaskVoid BreakSequenceAsync(int initialIndex, CancellationToken ct)
        {
            _isTriggered = true;

            if (vibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.StartContinuousVibration(vibrationSettings.BreakablePlatformProfile);
            }

            try
            {
                // Dalga kırılmasını başlat (İçten dışa yayılma)
                await PropagateBreakWaveAsync(initialIndex, ct);
            }
            finally
            {
                if (HapticManager.Instance != null)
                {
                    HapticManager.Instance.StopContinuousVibration();
                }
            }

            // VFX Efekti
            if (breakVFXPrefab != null)
            {
                Instantiate(breakVFXPrefab, transform.position, Quaternion.identity);
            }

            // Respawn süresini bekle
            if (respawnTime > 0)
            {
                float rewindVFXDuration = 0.4f;
                float waitTime = Mathf.Max(0f, respawnTime - rewindVFXDuration);

                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ct);

                if (reformVFXPrefab != null)
                {
                    Instantiate(reformVFXPrefab, transform.position, Quaternion.identity);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(rewindVFXDuration), cancellationToken: ct);

                ResetToDefault();
            }
        }

        /// <summary>
        /// İlk basılan bloktan başlayarak sola ve sağa dalga şeklinde kırılma animasyonlarını tetikler.
        /// </summary>
        private async UniTask PropagateBreakWaveAsync(int initialIndex, CancellationToken ct)
        {
            int maxDistance = Mathf.Max(initialIndex, blocks.Count - 1 - initialIndex);
            List<UniTask> currentWaveTasks = new List<UniTask>();

            // İlk basılan bloğu anında kırılmaya başlat
            if (initialIndex >= 0 && initialIndex < blocks.Count)
            {
                currentWaveTasks.Add(blocks[initialIndex].BreakBlockAsync(ct));
            }

            for (int dist = 1; dist <= maxDistance; dist++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(propagationDelay), cancellationToken: ct);

                int leftIndex = initialIndex - dist;
                int rightIndex = initialIndex + dist;

                if (leftIndex >= 0 && leftIndex < blocks.Count)
                {
                    currentWaveTasks.Add(blocks[leftIndex].BreakBlockAsync(ct));
                }

                if (rightIndex >= 0 && rightIndex < blocks.Count)
                {
                    currentWaveTasks.Add(blocks[rightIndex].BreakBlockAsync(ct));
                }
            }

            // Tüm blok animasyonlarının bitmesini bekle
            await UniTask.WhenAll(currentWaveTasks);
        }

        /// <summary>
        /// Platformu varsayılan sağlam durumuna geri getirir.
        /// </summary>
        [ContextMenu("Reset Platform")]
        public void ResetToDefault()
        {
            CancelBreakSequence();

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != null)
                {
                    blocks[i].ResetBlock();
                }
            }

            _isTriggered = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);

            if (blocks != null && blocks.Count > 0)
            {
                foreach (var block in blocks)
                {
                    if (block != null && block.BlockCollider != null)
                    {
                        Vector2 boxCenter = (Vector2)block.transform.position + block.BlockCollider.offset + (Vector2.up * 0.05f);
                        Vector2 boxSize = block.BlockCollider.bounds.size;
                        boxSize.y += 0.05f;
                        Gizmos.DrawWireCube(boxCenter, boxSize);
                    }
                }
            }
            else
            {
                float totalWidth = blockCount * blockWidth;
                float startX = -totalWidth / 2f + blockWidth / 2f;
                for (int i = 0; i < blockCount; i++)
                {
                    Vector3 pos = transform.position + new Vector3(startX + (i * blockWidth), 0f, 0f);
                    Gizmos.DrawWireCube(pos, new Vector3(blockWidth * 0.95f, 1f, 0f));
                }
            }
        }
#endif
    }
}
