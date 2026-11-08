# Menü "canlılığı": araştırma ve seçim

Tarih: 2026-11-09. Sorun: seçim değişince yalnızca dikdörtgen yer değiştiriyor. Üç ajan web'de aradı (Persona 5/3R, diğer oyunlar, uGUI+DOTween).

## Dürüst not
Hiçbir kaynakta süre/easing değeri çıkmadı. Aşağıdaki sayılar **başlangıç önerisi**, playtest ile ayarlanacak. Okunan en somut kaynaklar: mechanicsofmagic.com (P5 görsel tasarımı), itch.io blog "Making UI come to life: study of Persona 5", Hashino röportajları (PC Gamer/Kotaku). Diğer oyunlar için menü zamanlaması bulunamadı; yalnızca yaklaşımlar var.

## Bulunanlar
- **Persona 5:** seçili kelime büyür ve vurgu rengi alır; seçime göre arka plan görseli değişir; paneller eğik ve hızlı girer; her menü ayrı tasarlanır, ilk sürümler okunmaz çıkmış (okunabilirlik iterasyonu). Tuş renkleri gamepad ile eşleşir.
- **Pizza Tower, Inscryption:** menü dünyanın içinde (diegetik). **Cuphead:** menü dünyanın malzemesiyle konuşur. **Hades II:** arka plan döngüleri elle çizilmiş animasyon. **Hi-Fi Rush:** varış anı ritme oturur.
- **Teknik:** tween yap, her seçimde `DOKill`; ilk seçimde `OnSelect` tetiklenmeyebilir; menü tween'lerine `SetUpdate(true)`; animasyonlu öğeleri ayrı Canvas'a koy; `moveRepeatRate` tween süresinden uzun olmalı.

## Bize uyan (öneri)
1. ~~**Kanca ipi (diegetik imleç).** Delikteki kanca halkasından seçili satıra bir ip çizgisi uzanır; seçim değişince halka kıpırdar, ip yeni satıra "atılır" (~0.15 sn, OutBack). Pizza Tower fikri + oyunun kendi mekaniği. En büyük etki.
2. **Levha kayar, belirmez.** Sarı levha satırlar arasında kayar (DOAnchorPos), halka imleç kayarken döner.
3. **Seçili yazı Persona gibi büyür:** ~1.08x + 16 px sağa; seçilmeyenler hafif soluk. Gölge ofseti "çakılır" (önce büyür, sonra oturur).
4. **Giriş (≤0.5 sn):** satırlar soldan 0.06 sn arayla kayar, başlık ekstrüzyonu büyüyerek açılır; geçiş bitene kadar gezinme kilitli.
5. **Idle:** kanca halkası çok yavaş döner; başka sürekli hareket yok.
6. **Tık sesi + çok hafif rumble** (`*VibrationSettingsSO` deseni); ilk seçimde çalmaz.

## Bilerek almadıklarımız
Aşırı eğim/kolaj (Persona'nın punk dili, bizimki geometrik), su/akışkanlık (betonla çelişir), uzun geçişler ve sürekli hareketli arka plan (platformer menüsü hızlı gezilmeli).

## Uygulananlar (2026-11-09)
Poz sistemi, kayan levha (gerilme), gölge çakılması, açık halka imleci (dönen), etiket büyüme/kayma/soluklaşma, deliğin sürekli nefesi ve derinlik sürüklenmesi, açılış (~0,6 sn, gezinme kilitli). Kod: `UiMenuMotion`, `UiSpring`. Görüntü: `main-menu/juice_quit.png`.

Yapılmadı: tık sesi ve titreşim (projede UI ses dosyası yok, menü sahnesinde `HapticManager` yok). Pozlar, süreler ve miktarlar başlangıç değeri; gamepad ile ayarlanacak.

Kanca ipi denendi ve iptal edildi (kullanıcı kararı, 2026-11-09).
