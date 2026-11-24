# İkili kombinasyon listesi (taslak)

18 elemandan C(18,2) = 153 çift. İşaretler benim ilk değerlendirmem, oyun hissi tasarımcının (senin) kararı: gamepad ile denenmeden kesin değil.

- ✅ anlamlı, doğrudan oda fikri çıkar
- ➖ olur ama dikkat ister (okunurluk, benzer his, üst üste binen fırlatma vb.)
- ❌ elenir (aynı aileden, anlamsız ya da birbirini bozar)

**Elemanlar:** Node: normal, dönen, spiral, ters spiral, çiçek · Teknik: çift kanca, slingshot, duvar · Platform: hareketli, tetiklenen, kırılan, tek yönlü, bounce pad · Tehlike: asit, dönen/hareketli ölümcül · Diğer: slingshot yenileyici, anahtar-kapı, yere basmadan ödül

## 1. Node × Node (10)
- ✅ normal + dönen
- ✅ normal + spiral
- ✅ normal + ters spiral
- ✅ normal + çiçek
- ➖ dönen + çiçek
- ➖ spiral + çiçek
- ➖ ters spiral + çiçek
- ❌ dönen + spiral
- ❌ dönen + ters spiral
- ❌ spiral + ters spiral

## 2. Node × Teknik (15)
- ✅ normal + çift kanca
- ✅ normal + slingshot
- ✅ normal + duvar
- ✅ dönen + duvar
- ✅ spiral + duvar
- ✅ ters spiral + duvar
- ✅ çiçek + çift kanca
- ✅ çiçek + duvar
- ➖ dönen + çift kanca
- ➖ dönen + slingshot
- ➖ spiral + çift kanca
- ➖ ters spiral + çift kanca
- ➖ çiçek + slingshot
- ❌ spiral + slingshot
- ❌ ters spiral + slingshot

## 3. Node × Platform (25)
- ✅ normal + hareketli / tetiklenen / kırılan / tek yönlü / bounce pad (5)
- ✅ dönen + hareketli / tetiklenen / tek yönlü (3)
- ✅ spiral + hareketli / tetiklenen / tek yönlü (3)
- ✅ ters spiral + hareketli / tetiklenen / tek yönlü (3)
- ✅ çiçek + hareketli / tetiklenen / kırılan / tek yönlü / bounce pad (5)
- ➖ dönen + kırılan
- ➖ dönen + bounce pad
- ➖ spiral + kırılan
- ➖ spiral + bounce pad
- ➖ ters spiral + kırılan
- ➖ ters spiral + bounce pad

## 4. Node × Tehlike (10)
- ✅ normal + asit
- ✅ normal + ölümcül nesne
- ✅ dönen + asit
- ✅ spiral + asit
- ✅ ters spiral + asit
- ✅ çiçek + asit
- ✅ çiçek + ölümcül nesne
- ➖ dönen + ölümcül nesne
- ➖ spiral + ölümcül nesne
- ➖ ters spiral + ölümcül nesne

## 5. Node × Diğer (15)
- ✅ her node (normal, dönen, spiral, ters spiral, çiçek) + slingshot yenileyici / anahtar-kapı / yere basmadan ödül (15)

## 6. Teknik × Teknik (3)
- ✅ çift kanca + slingshot
- ✅ slingshot + duvar
- ➖ çift kanca + duvar

## 7. Teknik × Platform (15)
- ✅ çift kanca + hareketli
- ✅ çift kanca + tetiklenen
- ✅ çift kanca + tek yönlü
- ✅ çift kanca + bounce pad
- ✅ slingshot + hareketli
- ✅ slingshot + tek yönlü
- ✅ duvar + tetiklenen
- ✅ duvar + kırılan
- ✅ duvar + bounce pad
- ➖ çift kanca + kırılan
- ➖ slingshot + tetiklenen
- ➖ slingshot + kırılan
- ➖ slingshot + bounce pad (iki fırlatma üst üste)
- ➖ duvar + hareketli
- ❌ duvar + tek yönlü

## 8. Teknik × Tehlike (6)
- ✅ çift kanca / slingshot / duvar + asit (3)
- ✅ çift kanca / slingshot / duvar + ölümcül nesne (3)

## 9. Teknik × Diğer (9)
- ✅ çift kanca + anahtar-kapı
- ✅ çift kanca + yere basmadan ödül
- ✅ slingshot + slingshot yenileyici
- ✅ slingshot + anahtar-kapı
- ✅ slingshot + yere basmadan ödül
- ✅ duvar + anahtar-kapı
- ✅ duvar + yere basmadan ödül
- ➖ çift kanca + slingshot yenileyici
- ➖ duvar + slingshot yenileyici

## 10. Platform × Platform (10)
- ✅ hareketli + kırılan
- ✅ hareketli + tek yönlü
- ✅ hareketli + bounce pad
- ✅ tetiklenen + tek yönlü
- ✅ tetiklenen + bounce pad
- ✅ kırılan + tek yönlü
- ✅ kırılan + bounce pad
- ✅ tek yönlü + bounce pad (GDD'deki sık üçlünün parçası)
- ➖ hareketli + tetiklenen
- ➖ tetiklenen + kırılan

## 11. Platform × Tehlike (10)
- ✅ her platform (hareketli, tetiklenen, kırılan, tek yönlü, bounce pad) + asit / ölümcül nesne (10)

## 12. Platform × Diğer (15)
- ✅ hareketli / tetiklenen / kırılan / tek yönlü / bounce pad + anahtar-kapı (5)
- ✅ hareketli / tetiklenen / kırılan / tek yönlü / bounce pad + yere basmadan ödül (5)
- ✅ hareketli / tetiklenen / kırılan / tek yönlü + slingshot yenileyici (4)
- ➖ bounce pad + slingshot yenileyici

## 13. Tehlike × Tehlike (1)
- ✅ asit + ölümcül nesne

## 14. Tehlike × Diğer (6)
- ✅ asit / ölümcül nesne + slingshot yenileyici / anahtar-kapı / yere basmadan ödül (6)

## 15. Diğer × Diğer (3)
- ✅ slingshot yenileyici + anahtar-kapı
- ✅ slingshot yenileyici + yere basmadan ödül
- ✅ anahtar-kapı + yere basmadan ödül

## Toplam
✅ 119 · ➖ 28 · ❌ 6 = 153
