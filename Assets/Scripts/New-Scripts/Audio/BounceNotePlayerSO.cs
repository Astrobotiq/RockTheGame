using UnityEngine;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Bounce pad notalarını çalan ortak sistem. Zincir, ses sınırı ve nota geçmişi pad'den pad'e geçtiği için
    /// pad başına değil burada, tek asset'te tutulur (<c>NoteGate</c> + <c>NoteSequencer</c>). Çalışma zamanı durumu
    /// asset'e kaydedilmez. Pad sadece <see cref="TryPlay"/> çağırır, dönen <see cref="NoteInfo"/> görsele gider.
    /// </summary>
    [CreateAssetMenu(fileName = "BounceNotePlayer", menuName = "Audio/Bounce Note Player")]
    public class BounceNotePlayerSO : ScriptableObject
    {
        [SerializeField] private NotePaletteSO palette;
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;

        [Header("Limits")]
        [Tooltip("Son notadan bu kadar saniye içinde gelen pad vuruşu zincir sayılır (gamda tırmanır).")]
        [Min(0f)] [SerializeField] private float chainWindow = 1.5f;
        [Tooltip("İki nota arası en kısa süre (sn). İkiside aynı anda tetiklenirse ikincisi atlanır.")]
        [Min(0f)] [SerializeField] private float minInterval = 0.08f;
        [Tooltip("Aynı anda duyulabilen en fazla nota sesi. Dolunca yeni nota atlanır.")]
        [Min(1)] [SerializeField] private int maxVoices = 3;
        [Tooltip("Bir nota sesinin sayılma süresi (sn); kliplerin echo kuyruğu dahil uzunluğuna yakın olmalı.")]
        [Min(0.1f)] [SerializeField] private float voiceDuration = 1.2f;

        private NoteGate gate;
        private NoteSequencer sequencer;
        private int sequencerDegrees = -1;
        private int sequencerInstruments = -1;

        public NotePaletteSO Palette => palette;

        /// <summary>
        /// Sıradaki notayı seçip çalar. Kapı (aralık/ses sınırı) izin vermezse ya da palet/kanal eksikse false döner
        /// ve hiçbir şey çalmaz.
        /// </summary>
        public bool TryPlay(out NoteInfo note)
        {
            note = default;
            if (palette == null || sfxPlayChannel == null || palette.InstrumentCount == 0) return false;

            gate ??= new NoteGate(chainWindow, minInterval, maxVoices, voiceDuration);
            if (!gate.TryAcquire(Time.unscaledTime, out bool chained)) return false;

            if (sequencer == null || sequencerDegrees != palette.DegreeCount || sequencerInstruments != palette.InstrumentCount)
            {
                sequencerDegrees = palette.DegreeCount;
                sequencerInstruments = palette.InstrumentCount;
                sequencer = new NoteSequencer(sequencerDegrees, sequencerInstruments, () => Random.value);
            }

            note = palette.Resolve(sequencer.Next(chained));
            if (note.Cue == null) return false;

            var playParams = AudioCuePlayParams.Default;
            playParams.PitchMultiplier = note.PitchMultiplier;
            sfxPlayChannel.RaisePlayEvent(note.Cue, playParams);
            return true;
        }

        private void OnValidate()
        {
            gate = null; // inspector'dan sınırlar değişirse yeniden kurulsun
        }
    }
}
