using System;
using UnityEngine;

namespace New_Scripts.Audio
{
    /// <summary>Bir enstrümanın örnek bölgesi: tek klipli, pitch 1–1 bir cue ve o klibin kök MIDI notası.</summary>
    [Serializable]
    public class NoteZone
    {
        [Tooltip("Tek klipli, pitchMin = pitchMax = 1 olan cue (rastgele detune notayı bozar).")]
        public AudioCueSO cue;
        [Tooltip("Klibin gerçek notası (MIDI). A4 = 69.")]
        public int rootMidi = 69;
    }

    /// <summary>Bounce pad enstrümanı: 2–3 kök örnek bölgesi.</summary>
    [Serializable]
    public class NoteInstrument
    {
        public string name;
        public NoteZone[] zones;
    }

    /// <summary>Seçilen bir notanın ses ve görsel için çözülmüş hali.</summary>
    public readonly struct NoteInfo
    {
        public readonly int Midi;
        public readonly float Frequency;
        public readonly int DegreeClass;
        public readonly int Octave;
        public readonly int Instrument;
        public readonly AudioCueSO Cue;
        public readonly float PitchMultiplier;
        public readonly Color Color;

        public NoteInfo(int midi, float frequency, int degreeClass, int octave, int instrument,
            AudioCueSO cue, float pitchMultiplier, Color color)
        {
            Midi = midi;
            Frequency = frequency;
            DegreeClass = degreeClass;
            Octave = octave;
            Instrument = instrument;
            Cue = cue;
            PitchMultiplier = pitchMultiplier;
            Color = color;
        }
    }

    /// <summary>
    /// Bounce pad nota paleti: gam, kök, oktav sayısı, enstrümanlar ve derece renkleri.
    /// Varsayılanlar karar belgesindeki gibi A minör pentatonik, 2 oktav (A4–G6); oda başına kök değişirse
    /// rootMidi o odaya özel bir palette tutulur.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNotePalette", menuName = "Audio/Note Palette")]
    public class NotePaletteSO : ScriptableObject
    {
        [Header("Scale")]
        [Tooltip("Gam adımları (yarım ses). Minör pentatonik: 0 3 5 7 10.")]
        [SerializeField] private int[] scaleSteps = { 0, 3, 5, 7, 10 };
        [Tooltip("Kök nota (MIDI). A4 = 69.")]
        [SerializeField] private int rootMidi = 69;
        [Min(1)]
        [SerializeField] private int octaves = 2;

        [Header("Instruments")]
        [SerializeField] private NoteInstrument[] instruments;

        [Header("Visual")]
        [Tooltip("Her gam derecesi için bir renk (gam uzunluğu kadar). Yer tutucu; CRT'li çekimde elle ayarlanır.")]
        [SerializeField] private Color[] degreeColors =
        {
            // Mockup renkleri (bounce-pad-yonler.html COLORS): A C D E G
            new Color32(0xFC, 0xAE, 0x04, 255),
            new Color32(0x7C, 0xFF, 0x4F, 255),
            new Color32(0x22, 0xE6, 0xFF, 255),
            new Color32(0xA3, 0x6B, 0xFF, 255),
            new Color32(0xFF, 0x2E, 0x8A, 255),
        };

        public int[] ScaleSteps => scaleSteps;
        public int RootMidi => rootMidi;
        public int Octaves => octaves;
        public int InstrumentCount => instruments?.Length ?? 0;

        /// <summary>Gam derece sayısı, oktavlar dahil (sequencer'ın aralığı).</summary>
        public int DegreeCount => scaleSteps.Length * Mathf.Max(1, octaves);

        /// <summary>
        /// Seçimi ses (cue + pitch oranı) ve görsel (renk, frekans) verisine çevirir.
        /// Enstrümanın bölgesi yoksa Cue null döner; çağıran sessizce atlamalı.
        /// </summary>
        public NoteInfo Resolve(NoteChoice choice)
        {
            int midi = NoteMath.Midi(rootMidi, scaleSteps, choice.Degree);
            int degreeClass = NoteMath.DegreeClass(choice.Degree, scaleSteps.Length);
            int octave = NoteMath.Octave(choice.Degree, scaleSteps.Length);

            AudioCueSO cue = null;
            float pitch = 1f;
            NoteInstrument instrument = instruments != null && choice.Instrument < instruments.Length
                ? instruments[choice.Instrument]
                : null;
            if (instrument?.zones != null && instrument.zones.Length > 0)
            {
                int[] roots = new int[instrument.zones.Length];
                for (int i = 0; i < roots.Length; i++) roots[i] = instrument.zones[i].rootMidi;
                NoteZone zone = instrument.zones[NoteMath.NearestZone(roots, midi)];
                cue = zone.cue;
                pitch = NoteMath.PitchRatio(midi - zone.rootMidi);
            }

            Color color = degreeColors != null && degreeClass < degreeColors.Length ? degreeColors[degreeClass] : Color.white;
            return new NoteInfo(midi, NoteMath.Frequency(midi), degreeClass, octave, choice.Instrument, cue, pitch, color);
        }
    }
}
