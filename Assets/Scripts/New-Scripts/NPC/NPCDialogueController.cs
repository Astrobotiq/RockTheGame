using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.NPC
{
    public enum DialoguePlaybackMode
    {
        Single,      // Her yaklaşımda sadece ilk satırı yazar
        Sequential,  // Her yaklaşımda sıradaki satırı yazar (döngüsel)
        Random       // Her yaklaşımda rastgele bir satırı yazar
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

        [Header("Typewriter Settings")]
        [Tooltip("Her harfin ekranda belirmesi arasındaki süre (saniye).")]
        [SerializeField] private float charRevealSpeed = 0.04f;

        private Collider2D triggerCollider;
        private int currentLineIndex = 0;
        private int lastRandomIndex = -1;
        private CancellationTokenSource activeCts;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
                Debug.LogWarning($"[{name}] Collider2D isTrigger olarak ayarlandı.", this);
            }

            if (dialogueBubble == null)
            {
                dialogueBubble = GetComponentInChildren<NPCDialogueBubble>();
                if (dialogueBubble == null)
                {
                    Debug.LogError($"[{name}] NPCDialogueBubble bileşeni bulunamadı! Lütfen referans atayın.", this);
                }
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

        private string GetNextDialogueLine()
        {
            if (dialogueLines.Length == 1)
            {
                return dialogueLines[0];
            }

            switch (playbackMode)
            {
                case DialoguePlaybackMode.Single:
                    return dialogueLines[0];

                case DialoguePlaybackMode.Sequential:
                    string seqLine = dialogueLines[currentLineIndex];
                    currentLineIndex = (currentLineIndex + 1) % dialogueLines.Length;
                    return seqLine;

                case DialoguePlaybackMode.Random:
                    int index = UnityEngine.Random.Range(0, dialogueLines.Length);
                    // Üst üste aynı satırı seçmemek için kontrol
                    if (index == lastRandomIndex)
                    {
                        index = (index + 1) % dialogueLines.Length;
                    }
                    lastRandomIndex = index;
                    return dialogueLines[index];

                default:
                    return dialogueLines[0];
            }
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

        private void OnDisable()
        {
            CancelActiveDialogue();
        }

        private void OnDestroy()
        {
            CancelActiveDialogue();
        }
    }
}
