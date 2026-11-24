"""Bounce pad nota örneklerini sentezler (psychedelic, kısa).

Çalıştır:  python tools/audio/generate_bounce_notes.py
Çıktı:     Assets/Audio/SFX/Notes/<enstruman>_<midi>.wav  (mono, 16-bit, 44.1 kHz)

4 enstrüman x 3 kök nota (73, 80, 87 = C#5, G#5, D#6): A4-G6 aralığını +-4 yarım sesle kapatır.
Hepsi kısa (<= ~1.2 sn, echo kuyruğu dahil). Echo ve chorus örneğin içine pişirilmiştir; mixer'daki efektler üstüne eklenir.
Gereken: numpy. Rastgelelik sabit tohumlu, çıktı tekrarlanabilir.
"""
import os
import wave

import numpy as np

SR = 44100
ROOTS = [73, 80, 87]
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Audio", "SFX", "Notes")


def hz(midi):
    return 440.0 * 2.0 ** ((midi - 69) / 12.0)


def t_axis(seconds):
    return np.arange(int(SR * seconds)) / SR


def phase_of(freq):
    """Anlık frekans dizisinden faz (radyan)."""
    return 2 * np.pi * np.cumsum(freq) / SR


def sitar(midi):
    """Sitar/tanpura tarzı pluck: yukarıdan süzülen perde (meend), jawari vızıltısı, sempatik tel."""
    f0 = hz(midi)
    t = t_axis(0.75)
    bend = 1.0 + 0.018 * np.exp(-t / 0.035)  # ~30 cent yukarıdan başlar, 40 ms'de oturur
    ph = phase_of(f0 * bend)
    sig = np.zeros_like(t)
    for n in range(1, 14):
        if f0 * n > SR * 0.45:
            break
        sig += (1.0 / n) * np.sin(n * ph) * np.exp(-t * (2.5 + 2.2 * n))
    sig = np.tanh(3.0 * sig)  # jawari: köprü vızıltısı
    sym = 0.25 * np.sin(1.5 * phase_of(f0 * np.ones_like(t))) * np.exp(-t / 0.35)  # sempatik beşli
    return sig + sym


def fm_bell(midi):
    """FM çan: eşuyumsuz 1:3.5 modülatör, indeks hızla söner, hafif vibrato."""
    f0 = hz(midi)
    t = t_axis(0.9)
    vib = 1.0 + 0.004 * np.sin(2 * np.pi * 5.5 * t)
    car = phase_of(f0 * vib)
    mod = np.sin(3.5 * phase_of(f0 * np.ones_like(t)))
    index = 4.5 * np.exp(-t / 0.18) + 0.4
    return np.sin(car + index * mod) * np.exp(-t / 0.32)


def reverse_pluck(midi):
    """Ters pluck: kısa (0.16 sn) yükselen swell, hemen ardından düz pluck. Gecikme hissini kısa tutmak için swell kısa."""
    f0 = hz(midi)
    t = t_axis(0.5)
    ph = phase_of(f0 * np.ones_like(t))
    pluck = np.zeros_like(t)
    for n in range(1, 10):
        if f0 * n > SR * 0.45:
            break
        pluck += (1.0 / n) * np.sin(n * ph) * np.exp(-t * (3.0 + 2.0 * n))
    swell_n = int(SR * 0.16)
    swell = pluck[:swell_n][::-1].copy()
    swell *= np.linspace(0.0, 1.0, swell_n) ** 1.5  # sesin kendisi zaten tersten yükselir; sivriliği yumuşat
    return np.concatenate([swell * 0.8, pluck])


def phase_synth(midi):
    """Detune'lu 3 testere + hızla açılan rezonanslı süzgeç hissi (toplamsal sentez), flanger benzeri."""
    f0 = hz(midi)
    t = t_axis(0.8)
    cutoff = 350.0 + 3200.0 * np.exp(-t / 0.12)  # parlak başlar, kararır
    sig = np.zeros_like(t)
    for cents in (-13.0, 0.0, 13.0):
        f = f0 * 2.0 ** (cents / 1200.0)
        ph = phase_of(f * np.ones_like(t))
        for n in range(1, 24):
            if f * n > SR * 0.45:
                break
            gain = (1.0 / n) / (1.0 + (f * n / cutoff) ** 4)  # 4. derece alçak geçiren eğri
            sig += gain * np.sin(n * ph + 0.7 * n * cents)
    return sig * np.exp(-t / 0.4)


def fractional_delay(sig, delay_samples):
    """Zamanla değişen gecikme (örnek cinsinden dizi), doğrusal interpolasyon."""
    idx = np.arange(len(sig)) - delay_samples
    lo = np.floor(idx).astype(int)
    frac = idx - lo
    lo_c = np.clip(lo, 0, len(sig) - 1)
    hi_c = np.clip(lo + 1, 0, len(sig) - 1)
    out = sig[lo_c] * (1 - frac) + sig[hi_c] * frac
    out[idx < 0] = 0.0
    return out


def chorus(sig):
    t = np.arange(len(sig)) / SR
    delay = SR * (0.012 + 0.004 * np.sin(2 * np.pi * 0.8 * t))
    return sig + 0.5 * fractional_delay(sig, delay)


def echo(sig, delay_s=0.19, feedback=0.38, repeats=2):
    n = len(sig)
    out = np.concatenate([sig, np.zeros(int(SR * delay_s * repeats))])
    gain = 1.0
    for r in range(1, repeats + 1):
        gain *= feedback
        shift = int(SR * delay_s * r)
        out[shift:shift + n] += gain * sig
    return out


def finish(sig):
    sig = echo(chorus(sig))
    fade = int(SR * 0.04)
    sig[-fade:] *= np.linspace(1.0, 0.0, fade)
    sig[:64] *= np.linspace(0.0, 1.0, 64)  # tık önleyici
    sig = sig / np.max(np.abs(sig)) * 0.7
    return (sig * 32767).astype(np.int16)


def write_wav(path, data):
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


def pitch_check(data, midi):
    """Hedef frekansın +-50 cent çevresindeki en güçlü FFT tepesinin cent sapması (kaba sağlama)."""
    x = data.astype(np.float64)
    spec = np.abs(np.fft.rfft(x * np.hanning(len(x)), n=len(x) * 8))
    freqs = np.fft.rfftfreq(len(x) * 8, 1.0 / SR)
    target = hz(midi)
    mask = (freqs > target * 2 ** (-50 / 1200)) & (freqs < target * 2 ** (50 / 1200))
    peak = freqs[mask][np.argmax(spec[mask])]
    return 1200 * np.log2(peak / target)


INSTRUMENTS = {
    "sitar": sitar,
    "fmbell": fm_bell,
    "reverse": reverse_pluck,
    "phasesynth": phase_synth,
}


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, fn in INSTRUMENTS.items():
        for midi in ROOTS:
            data = finish(fn(midi))
            path = os.path.join(OUT, "%s_%d.wav" % (name, midi))
            write_wav(path, data)
            print("%-11s midi %d  %.2f sn  tepe sapmasi %+.1f cent" % (name, midi, len(data) / SR, pitch_check(data, midi)))


if __name__ == "__main__":
    main()
