# Bounce pad nota örnekleri

Bu klasördeki `*.wav` dosyalarının hepsi bu projede **sentezlenmiştir**; dış kaynaktan örnek, kayıt ya da ses kütüphanesi kullanılmamıştır. Lisans kısıtı yoktur.

- Üretici: `tools/audio/generate_bounce_notes.py` (numpy, sabit parametreli, tekrarlanabilir).
- Enstrümanlar: `sitar` (jawari vızıltılı pluck), `fmbell` (1:3.5 FM çan), `reverse` (kısa ters swell + pluck), `phasesynth` (detune'lu testere).
- Kökler: MIDI 73 / 80 / 87 (C#5 / G#5 / D#6); A4–G6 aralığı +-4 yarım sesle kapanır.
- Echo ve chorus örneklerin içine pişirilmiştir. Yeniden üretmek için scripti çalıştırıp Unity'de reimport et; cue ve palet asset'leri dosya adlarını değil klip referanslarını kullanır.
