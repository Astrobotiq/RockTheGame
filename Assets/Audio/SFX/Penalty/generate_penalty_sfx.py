# Balon cezasi (BB5) icin yer tutucu sesleri uretir: python generate_penalty_sfx.py
# 44.1 kHz, mono, 16 bit. Kulakla ayarlanir; gercek sesler gelince bu dosyalar degisir.
import math, random, struct, wave, os

RATE = 44100
HERE = os.path.dirname(os.path.abspath(__file__))


def write(name, samples):
    peak = max(1e-9, max(abs(s) for s in samples))
    path = os.path.join(HERE, name)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s / peak * 0.8)) * 32767)) for s in samples))
    print(name, len(samples) / RATE, "sn")


def bloop():
    # Sisme: sinus, ustel yukselen perde (280 -> 760 Hz), hizli atak, yumusak sonum.
    n = int(0.22 * RATE)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        u = i / n
        freq = 280.0 * (760.0 / 280.0) ** u
        phase += 2 * math.pi * freq / RATE
        env = min(1.0, t / 0.006) * math.exp(-t / 0.09) * min(1.0, (n - i) / (0.012 * RATE))
        out.append((math.sin(phase) + 0.25 * math.sin(2 * phase)) * env)
    return out


def hum():
    # Yolculuk uguldamasi: 2,5 sn, dikissiz dongu. Tum frekanslar 2,5 sn'de tam sayi cevrim (k * 0,4 Hz),
    # tremolo ve faz modulasyonu 1,2 Hz (= balonun salinimi, 3 cevrim).
    length = 2.5
    n = int(length * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        lfo = math.sin(2 * math.pi * 1.2 * t)
        pm = 0.5 * lfo
        s = (math.sin(2 * math.pi * 96 * t + pm)
             + 0.5 * math.sin(2 * math.pi * 144 * t + pm)
             + 0.2 * math.sin(2 * math.pi * 192 * t))
        out.append(s * (0.75 + 0.25 * lfo))
    return out


def pop():
    # Patlama: kisa gurultu patlamasi + dusen sinus (900 -> 300 Hz).
    random.seed(7)
    n = int(0.2 * RATE)
    out, phase, lp = [], 0.0, 0.0
    for i in range(n):
        t = i / RATE
        u = i / n
        freq = 900.0 * (300.0 / 900.0) ** u
        phase += 2 * math.pi * freq / RATE
        lp += 0.35 * ((random.random() * 2 - 1) - lp)  # hafif alcak geciren: sert cizirtiyi alir
        env_noise = math.exp(-t / 0.02)
        env_tone = min(1.0, t / 0.002) * math.exp(-t / 0.06)
        out.append(lp * env_noise * 1.4 + math.sin(phase) * env_tone)
    return out


if __name__ == "__main__":
    write("BubbleInflate.wav", bloop())
    write("BubbleHum.wav", hum())
    write("BubblePop.wav", pop())
