#!/usr/bin/env python3
"""
Rock The Game - halftone / Ben-Day doku ve parcacik sprite uretici.

Calistirma:  python generate_textures.py        (bu klasore PNG yazar)

Yontem notlari
- Tum dokular 1024x1024. Her nokta, 4x supersampling ile (4096x4096 tuval) analitik
  kenar yumusatmasi (antialias) ile cizilir, sonra 4x4 kutu ortalamasiyla (area filter,
  halkalanma yapmaz) 1024'e indirilir. Piksel izgarasina oturtma YOK; pixel-art degil.
- Tam tekrarlanabilirlik (seamless): donuk (rotated) ekran kafesi tamsayi (m, n)
  ciftiyle tanimlanir:  taban vektorleri a = L(m,-n)/N, b = L(n,m)/N,  N = m^2+n^2.
  Boylece (L,0) = m*a + n*b ve (0,L) = -n*a + m*b olur -> kafes L periyoduyla KESIN
  tekrar eder. Aci = atan2(-n, m), hucre sayisi = sqrt(N).
  Noktalar tile sinirinda modulo ile sarilarak cizilir (wrap).
- Mürekkep birlestirme: carpim (multiply). Mürekkep rengi kagit uzerinde AYNEN o rengi
  verecek sekilde T = min(1, renk/kagit) seffaflik katsayisina cevrilir; ust uste binen
  mürekkepler carpilarak ikincil renk (kirmizi, yesil, mor) uretir.
- Gradient ramp dikeyde tekrarlanmaz (yatayda tekrarlanir).
"""
import math
from pathlib import Path

import numpy as np
from PIL import Image

L = 1024          # cikti boyutu (piksel)
SS = 4            # supersampling carpani
OUT = Path(__file__).resolve().parent


def hexrgb(h):
    h = h.lstrip("#")
    return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)], dtype=np.float32) / 255.0


# role: y=sari, c=cyan benzeri mavi, m=magenta/pembe, k=siyah mürekkep, r=kirmizi
GAME = dict(y=hexrgb("#FCAE04"), c=hexrgb("#597DCE"), m=hexrgb("#FF51DE"),
            k=hexrgb("#140C1C"), r=hexrgb("#FC1E00"), paper=hexrgb("#F1E6C7"))
PRINT = dict(y=hexrgb("#FFE600"), c=hexrgb("#00A6E8"), m=hexrgb("#EC008C"),
             k=hexrgb("#231F20"), r=hexrgb("#EC008C"), paper=hexrgb("#FFFBEE"))


# ----------------------------------------------------------------- gurultu
def fft_noise(size, sigma, seed):
    """Periyodik (tileable) yumusatilmis beyaz gurultu, birim std."""
    rng = np.random.default_rng(seed)
    w = rng.standard_normal((size, size)).astype(np.float32)
    f = np.fft.fftfreq(size)
    fx, fy = np.meshgrid(f, f)
    g = np.exp(-2 * (math.pi ** 2) * (sigma ** 2) * (fx ** 2 + fy ** 2))
    out = np.fft.ifft2(np.fft.fft2(w) * g).real.astype(np.float32)
    return out / (out.std() + 1e-8)


def pnoise(x, y, seed, terms=4):
    """Tek noktada periyodik (L periyotlu) yumusak gurultu, yaklasik [-1, 1]."""
    rng = np.random.default_rng(seed)
    v = 0.0
    for _ in range(terms):
        fx, fy = rng.integers(0, 4, 2)
        if fx == 0 and fy == 0:
            fx = 1
        ph = rng.uniform(0, 2 * math.pi)
        v += math.sin(2 * math.pi * (fx * x + fy * y) / L + ph)
    return v / math.sqrt(terms / 2)


# ----------------------------------------------------------- ekran / nokta
class Screen:
    """Donuk halftone ekrani; (m, n) tamsayi cifti aciyi ve hucre sayisini belirler."""

    def __init__(self, m, n):
        self.m, self.n = m, n
        self.N = m * m + n * n
        self.cell = L / math.sqrt(self.N)                 # piksel cinsinden hucre
        self.angle = math.degrees(math.atan2(-n, m))

    def centers(self):
        m, n, N = self.m, self.n, self.N
        ax, ay = L * m / N, -L * n / N
        bx, by = L * n / N, L * m / N
        R = int(2 * math.sqrt(N)) + 3
        pts = []
        for i in range(-R, R + 1):
            for j in range(-R, R + 1):
                x = i * ax + j * bx
                y = i * ay + j * by
                if 0 <= x < L - 1e-6 and 0 <= y < L - 1e-6:
                    pts.append((x, y))
        assert abs(len(pts) - N) <= 1, (len(pts), N)
        return pts


def render_screen(scr, tone_fn, offset=(0.0, 0.0), wrap_y=True, soft=1.3, jitter=0.0, seed=0):
    """
    tone_fn(x, y) -> alan kapsami (1.0 = hucrenin tamami). Yaricap r = sqrt(t/pi) hucre.
    Donus: OUT cozunurlugunde [0,1] kapsama matrisi (L x L).
    """
    W = L * SS
    cov = np.zeros((W, W), np.float32)
    rng = np.random.default_rng(seed)
    ox, oy = offset
    for (x, y) in scr.centers():
        t = tone_fn(x, y)
        if t <= 0.003:
            continue
        r = math.sqrt(t / math.pi) * scr.cell * SS
        if jitter:
            r *= 1.0 + jitter * rng.uniform(-1, 1)
        cx, cy = (x + ox) * SS, (y + oy) * SS
        x0, x1 = int(math.floor(cx - r - 3)), int(math.ceil(cx + r + 3))
        y0, y1 = int(math.floor(cy - r - 3)), int(math.ceil(cy + r + 3))
        xs = np.arange(x0, x1 + 1)
        ys = np.arange(y0, y1 + 1)
        d = np.hypot(xs[None, :] - cx, ys[:, None] - cy)
        p = np.clip((r - d) / soft + 0.5, 0, 1).astype(np.float32)
        ix = xs % W
        if wrap_y:
            iy = ys % W
        else:
            ok = (ys >= 0) & (ys < W)
            iy, p = ys[ok], p[ok]
        sl = np.ix_(iy, ix)
        cov[sl] = np.maximum(cov[sl], p)
    return cov.reshape(L, SS, L, SS).mean(axis=(1, 3))


# --------------------------------------------------------------- birlestirme
def compose_on_paper(pal, inks, paper_grain=0.035, seed=1, ink_grain=0.0, paper_override=None):
    """inks: [(kapsama, renk)]. Carpim ile kagit uzerine basar. Donus: uint8 RGB."""
    paper = pal["paper"] if paper_override is None else paper_override
    fine = fft_noise(L, 0.8, seed + 100)
    coarse = fft_noise(L, 22, seed + 101)
    img = np.ones((L, L, 3), np.float32) * paper
    img *= (1 + paper_grain * fine + 0.018 * coarse)[..., None]
    for k, (cov, col) in enumerate(inks):
        T = np.minimum(1.0, col / paper)
        dens = cov
        if ink_grain:
            n = fft_noise(L, 1.2, seed + 200 + k)
            dens = cov * np.clip(0.93 + ink_grain * n, 0, 1)
        img *= (1 - dens[..., None] * (1 - T)[None, None, :])
    return (np.clip(img, 0, 1) * 255 + 0.5).astype(np.uint8)


def save(arr, name):
    mode = "RGBA" if arr.shape[2] == 4 else "RGB"
    Image.fromarray(arr, mode).save(OUT / name, optimize=True)
    print("yazildi:", name)


# ----------------------------------------------------------------- dokular
def classic_benday(color, name, pal):
    s = Screen(12, -12)                      # 45 derece, ~17 hucre
    cov = render_screen(s, lambda x, y: 0.50, soft=1.4)
    save(compose_on_paper(pal, [(cov, color)], seed=11), name)


def cmyk_sparse(pal, name):
    sy, sc, sm, sk = Screen(14, 0), Screen(13, -4), Screen(4, -13), Screen(10, -10)
    cy = render_screen(sy, lambda x, y: 0.44 + 0.06 * pnoise(x, y, 1), offset=(0, 0), seed=1)
    cc = render_screen(sc, lambda x, y: 0.15 + 0.03 * pnoise(x, y, 2), offset=(6, -4), seed=2)
    cm = render_screen(sm, lambda x, y: 0.15 + 0.03 * pnoise(x, y, 3), offset=(-5, 5), seed=3)
    ck = render_screen(sk, lambda x, y: 0.035 + 0.015 * pnoise(x, y, 4), offset=(4, 6), seed=4)
    save(compose_on_paper(pal, [(cy, pal["y"]), (cc, pal["c"]), (cm, pal["m"]), (ck, pal["k"])], seed=21), name)


def cmyk_dense(pal, name):
    sy, sc, sm, sk = Screen(10, 0), Screen(10, -3), Screen(3, -10), Screen(7, -7)
    cy = render_screen(sy, lambda x, y: 0.78 + 0.40 * pnoise(x, y, 11), offset=(0, 0), jitter=0.03, seed=1)
    cm = render_screen(sm, lambda x, y: 0.62 + 0.36 * pnoise(x, y, 12), offset=(-9, 7), jitter=0.03, seed=2)
    cc = render_screen(sc, lambda x, y: 0.42 + 0.30 * pnoise(x, y, 13), offset=(8, -6), jitter=0.03, seed=3)
    ck = render_screen(sk, lambda x, y: 0.14 + 0.14 * pnoise(x, y, 14), offset=(5, 9), jitter=0.03, seed=4)
    save(compose_on_paper(pal, [(cy, pal["y"]), (cm, pal["m"]), (cc, pal["c"]), (ck, pal["k"])],
                          paper_grain=0.03, seed=31), name)


def ramp(s, s0, s1, v0, v1):
    t = min(1.0, max(0.0, (s - s0) / (s1 - s0)))
    t = t * t * (3 - 2 * t)
    return v0 + (v1 - v0) * t


def gradient_ramp(pal, name):
    sy, sc, sm, sk = Screen(20, 0), Screen(19, -5), Screen(5, -19), Screen(14, -14)
    cy = render_screen(sy, lambda x, y: ramp(y / L, 0.0, 1.0, 0.05, 1.00), wrap_y=False, seed=1)
    cm = render_screen(sm, lambda x, y: ramp(y / L, 0.12, 1.0, 0.00, 0.75), offset=(-3, 3), wrap_y=False, seed=2)
    cc = render_screen(sc, lambda x, y: ramp(y / L, 0.30, 1.0, 0.00, 0.55), offset=(4, -2), wrap_y=False, seed=3)
    ck = render_screen(sk, lambda x, y: ramp(y / L, 0.65, 1.0, 0.00, 0.26), offset=(2, 4), wrap_y=False, seed=4)
    save(compose_on_paper(pal, [(cy, pal["y"]), (cm, pal["m"]), (cc, pal["c"]), (ck, pal["k"])], seed=41), name)


def riso_grain(pal, a, b, name):
    sa, sb = Screen(19, -5), Screen(5, -19)
    ca = render_screen(sa, lambda x, y: 0.58 + 0.10 * pnoise(x, y, 21), offset=(0, 0), soft=1.8, jitter=0.04, seed=1)
    cb = render_screen(sb, lambda x, y: 0.46 + 0.10 * pnoise(x, y, 22), offset=(7, -5), soft=1.8, jitter=0.04, seed=2)
    save(compose_on_paper(pal, [(ca, a), (cb, b)], paper_grain=0.05, ink_grain=0.20, seed=51), name)


# ----------------------------------------------------------------- sprite'lar
S = 256
SW = S * SS


def _grid():
    ys, xs = np.mgrid[0:SW, 0:SW].astype(np.float32)
    return xs, ys


def blob_cov(xs, ys, cx, cy, r0, harm, soft=2.5, halo=0.0, bumps=()):
    """Duzensiz (acisal harmonik) mürekkep lekesi kapsami. Birimler: SW piksel."""
    dx, dy = xs - cx, ys - cy
    th = np.arctan2(dy, dx)
    d = np.hypot(dx, dy)
    rr = np.ones_like(d)
    for (k, amp, ph) in harm:
        rr += amp * np.sin(k * th + ph)
    for (t0, amp, wd) in bumps:
        dth = np.arctan2(np.sin(th - t0), np.cos(th - t0))
        rr += amp * np.exp(-(dth / wd) ** 2)
    r = r0 * rr
    core = np.clip((r - d) / soft + 0.5, 0, 1)
    if halo > 0:   # ince murekkep sizmasi (bleed) halesi
        tail = np.clip(1 - (d - r) / (0.10 * r0), 0, 1) * (d > r)
        core = np.maximum(core, halo * tail ** 2)
    edge = np.exp(-(((d - r * 0.92) / (r0 * 0.05)) ** 2))   # kenarda hafif koyulasma
    return core.astype(np.float32), edge.astype(np.float32)


def sprite_rgba(layers, rim=0.12, grain=0.10, seed=7):
    """layers: [(kapsama, renk, edge)]. Seffaf zemin, carpim birlestirme. Donus uint8 RGBA."""
    w = np.ones((SW, SW, 3), np.float32)
    keep = np.ones((SW, SW), np.float32)
    edge = np.zeros((SW, SW), np.float32)
    for cov, col, e in layers:
        w *= 1 - cov[..., None] * (1 - col)[None, None, :]
        keep *= 1 - cov
        edge = np.maximum(edge, e * cov)
    alpha = 1 - keep
    ink = (w - keep[..., None]) / np.maximum(alpha[..., None], 1e-4)
    ink = np.clip(ink, 0, 1) * (1 - rim * edge[..., None])
    n = fft_noise(SW, 2.2, seed)
    alpha = np.clip(alpha * (1 - grain * np.clip(n, 0, None) * (alpha > 0.85)), 0, 1)
    rgba = np.dstack([ink, alpha])
    rgba = rgba.reshape(S, SS, S, SS, 4).copy()
    # premultiplied ortalama ile indir (kenarda siyah halo olusmasin)
    a = rgba[..., 3:4]
    pre = (rgba[..., :3] * a).mean(axis=(1, 3))
    am = a.mean(axis=(1, 3))
    rgb = pre / np.maximum(am, 1e-4)
    out = np.dstack([rgb, am])
    return (np.clip(out, 0, 1) * 255 + 0.5).astype(np.uint8)


def sprites():
    xs, ys = _grid()
    c = SW / 2
    g = GAME
    # 1) tek yuvarlak mürekkep noktasi, hafif sizma kenari
    cov, e = blob_cov(xs, ys, c, c, 0.36 * SW, [(3, 0.022, 1.0), (5, 0.016, 2.0), (9, 0.008, 0.5)], halo=0.30)
    save(sprite_rgba([(cov, g["r"], e)], seed=1), "trail_ink_dot.png")
    # 2) misregistered cift (pembe + mavi), kesisimde carpim
    h = [(3, 0.02, 0.4), (5, 0.014, 1.3), (8, 0.008, 2.2)]
    d = 0.058 * SW
    c1, e1 = blob_cov(xs, ys, c - d, c + d * 0.6, 0.29 * SW, h, halo=0.22)
    c2, e2 = blob_cov(xs, ys, c + d, c - d * 0.6, 0.29 * SW, h[::-1], halo=0.22)
    save(sprite_rgba([(c1, g["m"], e1), (c2, g["c"], e2)], seed=2), "trail_misreg_pair.png")
    # 3) kucuk halftone nokta kumesi (hex yerlesim, farkli mürekkepler)
    layers = []
    pitch = 0.215 * SW
    pos = [(0, 0, 0.100, "y"), (1, 0, 0.085, "m"), (-1, 0, 0.085, "c"), (0.5, 0.866, 0.070, "y"),
           (-0.5, 0.866, 0.055, "m"), (0.5, -0.866, 0.075, "c"), (-0.5, -0.866, 0.050, "y"),
           (1.5, 0.866, 0.035, "m"), (-1.5, -0.866, 0.035, "c")]
    for (i, j, rr, ink) in pos:
        cv, ee = blob_cov(xs, ys, c + i * pitch + 0.012 * SW * (1 if ink == "m" else -0.6),
                          c + j * pitch, rr * SW, [(3, 0.015, i + 1.0)], halo=0.12)
        layers.append((cv, g[ink], ee))
    save(sprite_rgba(layers, seed=3), "trail_dot_cluster.png")
    # 4) yumusak "ink splat": duzensiz leke, dil cikintilari, uydu damlalar
    bumps = [(0.4, 0.34, 0.16), (2.0, 0.26, 0.13), (3.6, 0.42, 0.11), (5.1, 0.22, 0.15)]
    cov, e = blob_cov(xs, ys, c, c, 0.25 * SW,
                      [(2, 0.12, 0.5), (3, 0.09, 1.7), (5, 0.06, 0.3), (7, 0.04, 2.0)],
                      soft=3.0, bumps=bumps, halo=0.18)
    sat = np.zeros_like(cov)
    for (th, rad, rr) in [(0.4, 0.43, 0.030), (2.0, 0.40, 0.022), (3.6, 0.45, 0.028), (5.1, 0.38, 0.020), (1.2, 0.41, 0.016)]:
        cv, _ = blob_cov(xs, ys, c + math.cos(th) * rad * SW, c + math.sin(th) * rad * SW, rr * SW, [], soft=2.5)
        sat = np.maximum(sat, cv)
    cov = np.maximum(cov, sat)
    save(sprite_rgba([(cov, g["m"], e)], seed=4), "trail_ink_splat.png")


# ----------------------------------------------------------------- ana akis
def main():
    classic_benday(GAME["r"], "classic_benday_red_game.png", GAME)
    classic_benday(GAME["m"], "classic_benday_pink_game.png", GAME)
    classic_benday(PRINT["m"], "classic_benday_magenta_print.png", PRINT)
    cmyk_sparse(GAME, "cmyk_sparse_game.png")
    cmyk_sparse(PRINT, "cmyk_sparse_print.png")
    cmyk_dense(GAME, "cmyk_dense_game.png")
    cmyk_dense(PRINT, "cmyk_dense_print.png")
    gradient_ramp(GAME, "gradient_ramp_game.png")
    gradient_ramp(PRINT, "gradient_ramp_print.png")
    riso_grain(GAME, GAME["m"], GAME["c"], "riso_grain_game.png")
    riso_grain(PRINT, PRINT["m"], PRINT["c"], "riso_grain_print.png")
    sprites()


if __name__ == "__main__":
    main()
