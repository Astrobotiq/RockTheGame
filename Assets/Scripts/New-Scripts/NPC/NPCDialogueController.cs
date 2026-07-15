using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Player;
using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.NPC
{
    public enum DialoguePlaybackMode
    {
        Single,      // Her yaklaşımda sadece ilk satırı yazar
        Sequential,  // Her yaklaşımda sıradaki satırı yazar (döngüsel)
        Random       // Her yaklaşımda rastgele bir satırı yazar
    }

    public enum DialogueAdvanceCondition
    {
        OnTriggerEnter, // Varsayılan: Her girip çıkıldığında sonraki diyaloğa geçer
        OnPlayerDeath   // Sadece oyuncu öldüğünde sonraki diyaloğa geçer
    }

    [RequireComponent(typeof(Collider2D))]
    public class NPCDialogueController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCDialogueBubble dialogueBubble;

        [Header("Dialogue Content")]
        [TextArea(3, 5)]
        [SerializeField] private string[] dialogueLines;
        [SerializeField] private DialoguePlaybackMode playbackMode = DialoguePlaybackMode.Single;
        [SerializeField] private DialogueAdvanceCondition advanceCondition = DialogueAdvanceCondition.OnTriggerEnter;

        [Header("Typewriter Settings")]
        [Tooltip("Her harfin ekranda belirmesi arasındaki süre (saniye).")]
        [SerializeField] private float charRevealSpeed = 0.04f;

        private Collider2D triggerCollider;
        private int currentLineIndex = 0;
        private int lastRandomIndex = -1;
        private CancellationTokenSource activeCts;
        private PlayerHealth cachedPlayerHealth;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
                Debug.LogWarning($"[{name}] Collider2D isTrigger olarak ayarlandı.", this);
            }
        }

        private void Start()
        {
            if (dialogueBubble == null)
            {
                dialogueBubble = NPCDialogueBubble.Instance;
                if (dialogueBubble == null)
                {
                    dialogueBubble = GetComponentInChildren<NPCDialogueBubble>();
                }
            }

            if (dialogueBubble == null)
            {
                Debug.LogError($"[{name}] NPCDialogueBubble bileşeni bulunamadı! Lütfen sahnede bir NPCDialogueBubble bulunduğundan veya referans atadığınızdan emin olun.", this);
            }
        }

        private void OnEnable()
        {
            if (cachedPlayerHealth == null)
            {
                cachedPlayerHealth = FindObjectOfType<PlayerHealth>();
            }

            if (cachedPlayerHealth != null)
            {
                cachedPlayerHealth.OnDeath -= HandlePlayerDeath;
                cachedPlayerHealth.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            CancelActiveDialogue();
            if (cachedPlayerHealth != null)
            {
                cachedPlayerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Yaklaşanın oyuncu olup olmadığını kontrol et
            if (other.TryGetComponent(out PlayerController player))
            {
                if (dialogueLines == null || dialogueLines.Length == 0)
                {
                    return;
                }

                // Önceki aktif işlemleri iptal et
                CancelActiveDialogue();

                // Yeni CancellationTokenSource oluştur
                activeCts = new CancellationTokenSource();

                // Hangi satırı göstereceğimizi seçelim
                string lineToShow = GetNextDialogueLine();

                // Asenkron olarak diyaloğu oynat
                ShowDialogueAsync(lineToShow, activeCts.Token).Forget();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                // Mevcut yazım işlemini iptal et ve balonu kapat
                CancelActiveDialogue();

                activeCts = new CancellationTokenSource();
                HideDialogueAsync(activeCts.Token).Forget();
            }
        }

        private void HandlePlayerDeath()
        {
            if (advanceCondition == DialogueAdvanceCondition.OnPlayerDeath)
            {
                AdvanceDialogueIndex();
            }
        }

        private void AdvanceDialogueIndex()
        {
            if (dialogueLines == null || dialogueLines.Length <= 1)
            {
                return;
            }

            if (playbackMode == DialoguePlaybackMode.Sequential)
            {
                currentLineIndex = (currentLineIndex + 1) % dialogueLines.Length;
            }
            else if (playbackMode == DialoguePlaybackMode.Random)
            {
                int index = UnityEngine.Random.Range(0, dialogueLines.Length);
                if (index == lastRandomIndex)
                {
                    index = (index + 1) % dialogueLines.Length;
                }
                lastRandomIndex = index;
                currentLineIndex = index;
            }
        }

        private string GetNextDialogueLine()
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                return string.Empty;
            }

            if (dialogueLines.Length == 1)
            {
                return dialogueLines[0];
            }

            if (advanceCondition == DialogueAdvanceCondition.OnTriggerEnter)
            {
                if (playbackMode == DialoguePlaybackMode.Sequential)
                {
                    string seqLine = dialogueLines[currentLineIndex];
                    currentLineIndex = (currentLineIndex + 1) % dialogueLines.Length;
                    return seqLine;
                }
                else if (playbackMode == DialoguePlaybackMode.Random)
                {
                    int index = UnityEngine.Random.Range(0, dialogueLines.Length);
                    if (index == lastRandomIndex)
                    {
                        index = (index + 1) % dialogueLines.Length;
                    }
                    lastRandomIndex = index;
                    currentLineIndex = index;
                    return dialogueLines[currentLineIndex];
                }
            }

            // Eğer OnPlayerDeath modundaysak, zaten index death event ile ilerletildi.
            // Direkt güncel indexteki satırı döndürürüz.
            return dialogueLines[currentLineIndex];
        }

        private async UniTaskVoid ShowDialogueAsync(string line, CancellationToken ct)
        {
            if (dialogueBubble != null)
            {
                await dialogueBubble.ShowDialogueAsync(line, charRevealSpeed, ct);
            }
        }

        private async UniTaskVoid HideDialogueAsync(CancellationToken ct)
        {
            if (dialogueBubble != null)
            {
                await dialogueBubble.HideDialogueAsync(ct);
            }
        }

        private void CancelActiveDialogue()
        {
            if (activeCts != null)
            {
                activeCts.Cancel();
                activeCts.Dispose();
                activeCts = null;
            }
        }

        private void OnDestroy()
        {
            CancelActiveDialogue();
        }
    }
}
