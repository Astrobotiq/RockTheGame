using New_Scripts.Death;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class DeathBurstMathTests
    {
        [Test]
        public void Progress_HitstopBoyuncaSifir_SonraDogrusal()
        {
            Assert.AreEqual(0f, DeathBurstMath.Progress(0.03f, 0.06f, 0.7f), 1e-5f);
            Assert.AreEqual(0.5f, DeathBurstMath.Progress(0.38f, 0.06f, 0.7f), 1e-4f);
            Assert.AreEqual(1f, DeathBurstMath.Progress(5f, 0.06f, 0.7f), 1e-5f);
        }

        [Test]
        public void Position_HareketOncesiBaslangictaDurur()
        {
            Vector2 o = new Vector2(2f, 3f);
            Assert.AreEqual(o, DeathBurstMath.Position(o, new Vector2(5f, 5f), 3f, 4.5f, 0.12f, -0.1f));
        }

        [Test]
        public void Position_SurtunmeyleYolVOverDragaYaklasir()
        {
            // yerçekimi yok: toplam yol v0 / drag
            Vector2 p = DeathBurstMath.Position(Vector2.zero, new Vector2(6f, 0f), 3f, 0f, 0f, 10f);
            Assert.AreEqual(2f, p.x, 1e-3f);
            Assert.AreEqual(0f, p.y, 1e-5f);
        }

        [Test]
        public void Position_YercekimiGecikmedenSonraBaslar()
        {
            Vector2 before = DeathBurstMath.Position(Vector2.zero, Vector2.zero, 3f, 4.5f, 0.12f, 0.12f);
            Vector2 after = DeathBurstMath.Position(Vector2.zero, Vector2.zero, 3f, 4.5f, 0.12f, 0.32f);
            Assert.AreEqual(0f, before.y, 1e-5f);
            Assert.Less(after.y, 0f);
        }

        [Test]
        public void ParticleSpeed_UctakiMerkezdenHizli()
        {
            Assert.Greater(DeathBurstMath.ParticleSpeed(2f, 3f, 1f, 1f), DeathBurstMath.ParticleSpeed(2f, 3f, 0f, 1f));
        }

        [Test]
        public void ParticleVisible_BaslardaGorunur_SonundaKaybolur()
        {
            Assert.IsTrue(DeathBurstMath.ParticleVisible(0.1f, 0.7f));
            Assert.IsFalse(DeathBurstMath.ParticleVisible(1f, 0.7f));
            // kaybolma payından sonra sönme penceresi: yanıp söner, sonunda hiç görünmez
            Assert.IsFalse(DeathBurstMath.ParticleVisible(0.7f * 0.85f + 0.01f, 0.7f)); // sönme başladı, ilk evre: gizli
            Assert.IsTrue(DeathBurstMath.ParticleVisible(0.7f * 0.85f + 0.06f, 0.7f));  // yanıp sönmenin görünür evresi
        }

        [Test]
        public void BuildPixelCells_DoluPikselleriHucreler_BosuAtlar()
        {
            // 4x4, yalnızca (1,1)..(2,2) dolu (4 piksel), cellPx 2 -> tek hücre
            var px = new Color32[16];
            for (int y = 1; y <= 2; y++)
                for (int x = 1; x <= 2; x++) px[y * 4 + x] = new Color32(200, 100, 50, 255);
            var cells = DeathBurstMath.BuildPixelCells(px, 4, 4, 2, 25);
            Assert.AreEqual(1, cells.Count);
            Assert.AreEqual(2f, cells[0].Center.x, 1e-4f);
            Assert.AreEqual(2f, cells[0].Center.y, 1e-4f);
            Assert.AreEqual(200, cells[0].Color.r);
            Assert.AreEqual(255, cells[0].Color.a);
        }

        [Test]
        public void BuildPixelCells_CellPx1_HerPikselBirHucre_OrtalamaRenk()
        {
            var px = new Color32[4];
            px[0] = new Color32(255, 0, 0, 255);
            px[1] = new Color32(0, 0, 255, 255);
            Assert.AreEqual(2, DeathBurstMath.BuildPixelCells(px, 2, 2, 1, 25).Count);
            var one = DeathBurstMath.BuildPixelCells(px, 2, 2, 2, 25);
            Assert.AreEqual(1, one.Count);
            Assert.AreEqual(127, one[0].Color.r, 1);
            Assert.AreEqual(127, one[0].Color.b, 1);
        }

        [Test]
        public void BuildPixelCells_Bos_SifirHucre()
        {
            Assert.AreEqual(0, DeathBurstMath.BuildPixelCells(new Color32[9], 3, 3, 2, 25).Count);
            Assert.AreEqual(0, DeathBurstMath.BuildPixelCells(null, 3, 3, 2, 25).Count);
        }

        [Test]
        public void Snap_PikselIzgarasinaOturur()
        {
            Assert.AreEqual(0.125f, DeathBurstMath.Snap(0.13f, 16f), 1e-5f);
            Assert.AreEqual(0.13f, DeathBurstMath.Snap(0.13f, 0f), 1e-5f);
        }

        [Test]
        public void BlockScale_SonundaGizli_BaslardaTam()
        {
            Assert.AreEqual(1f, DeathBurstMath.BlockScale(0.1f, false, 0.9f));
            Assert.AreEqual(0f, DeathBurstMath.BlockScale(1f, false, 0.9f));
            Assert.AreEqual(0.5f, DeathBurstMath.BlockScale(0.95f, false, 0.9f));
        }

        [Test]
        public void DissolveScale_BaslardaTam_SonundaSifir()
        {
            Assert.AreEqual(1f, DeathBurstMath.DissolveScale(0f));
            Assert.AreEqual(0f, DeathBurstMath.DissolveScale(1f));
        }

        [Test]
        public void HeadLaunch_AciYonundeHiz_ArtiOyuncuHizi()
        {
            Vector2 v = DeathBurstMath.HeadLaunch(2f, 3f, 0.5f, Mathf.PI / 2f, new Vector2(1f, 0f));
            Assert.AreEqual(1f, v.x, 1e-4f);
            Assert.AreEqual(3f, v.y, 1e-4f);
        }

        [Test]
        public void HeadPush_OyuncudanKafayaDogru_HafifYukari_SinirliHiz()
        {
            Vector2 push = DeathBurstMath.HeadPush(new Vector2(1f, 0f), 3.5f, Vector2.zero, 0.3f, 7f);
            Assert.Greater(push.x, 0f);   // oyuncunun sağındaki kafa sağa
            Assert.Greater(push.y, 0f);   // hafif yukarı
            Assert.AreEqual(3.5f, push.magnitude, 1e-3f);
            Vector2 fast = DeathBurstMath.HeadPush(new Vector2(-1f, 0f), 3.5f, new Vector2(-100f, 0f), 1f, 7f);
            Assert.LessOrEqual(fast.magnitude, 7f + 1e-4f);
            Assert.Less(fast.x, 0f);
        }

        [Test]
        public void DashPush_DashYonundeVeHafifYukari_NormalItmedenHizli()
        {
            Vector2 normal = DeathBurstMath.HeadPush(new Vector2(1f, 0f), 3.5f, Vector2.zero, 0f, 7f);
            Vector2 dash = DeathBurstMath.DashPush(new Vector2(33f, 0f), new Vector2(1f, 0f), 10f, 25f, 14f);
            Assert.Greater(dash.magnitude, normal.magnitude);
            Assert.AreEqual(10f, dash.magnitude, 1e-3f);
            Assert.Greater(dash.x, 0f);
            // 25 derece yukarı: y/x = tan(25) ≈ 0.466
            Assert.AreEqual(Mathf.Tan(25f * Mathf.Deg2Rad), dash.y / dash.x, 0.02f);
        }

        [Test]
        public void DashPush_KafaOyuncununAltindaOlsaBile_EgimSabit()
        {
            // oyuncu merkezi kafadan yüksekte: ham vektör aşağı bakar ama eğim 25 derece kalmalı
            Vector2 low = DeathBurstMath.DashPush(new Vector2(43f, 0f), new Vector2(0.33f, -0.19f), 10f, 25f, 14f);
            Vector2 level = DeathBurstMath.DashPush(new Vector2(43f, 0f), new Vector2(1f, 0f), 10f, 25f, 14f);
            Assert.AreEqual(level.y, low.y, 1e-3f);
            Assert.AreEqual(Mathf.Tan(25f * Mathf.Deg2Rad), low.y / low.x, 0.02f);
        }

        [Test]
        public void DashPush_SolaDashSolaGider_DikeyDashEgimEklemez_SinirliHiz()
        {
            Vector2 left = DeathBurstMath.DashPush(new Vector2(-33f, 0f), new Vector2(-1f, 0f), 10f, 25f, 14f);
            Assert.Less(left.x, 0f);
            Assert.Greater(left.y, 0f);
            Vector2 down = DeathBurstMath.DashPush(new Vector2(0f, -33f), new Vector2(0f, -1f), 10f, 25f, 14f);
            Assert.AreEqual(0f, down.x, 1e-4f);
            Assert.Less(down.y, 0f);
            Assert.LessOrEqual(DeathBurstMath.DashPush(new Vector2(33f, 0f), Vector2.right, 99f, 25f, 14f).magnitude, 14f + 1e-4f);
        }

        [Test]
        public void HeadPush_AyniNokta_YukariItilir()
        {
            Vector2 push = DeathBurstMath.HeadPush(Vector2.zero, 3f, Vector2.zero, 0f, 7f);
            Assert.AreEqual(0f, push.x, 1e-4f);
            Assert.Greater(push.y, 0f);
        }

        [Test]
        public void AddCapped_YavasKafaSinirdaKalir_HizliDusenKirpilmaz()
        {
            Vector2 slow = DeathBurstMath.AddCapped(new Vector2(1f, 0f), new Vector2(10f, 0f), 7f);
            Assert.AreEqual(7f, slow.magnitude, 1e-4f);
            Vector2 falling = DeathBurstMath.AddCapped(new Vector2(0f, -20f), new Vector2(1f, 0f), 7f);
            Assert.Greater(falling.magnitude, 7f);
            Assert.LessOrEqual(falling.magnitude, 20f + 1e-4f);
        }

        [Test]
        public void StretchScale_DurganKafaUzamaz_DusenDikeyUzar()
        {
            Assert.AreEqual(Vector2.one, DeathBurstMath.StretchScale(Vector2.zero, 0.03f, 0.3f));
            Vector2 fall = DeathBurstMath.StretchScale(new Vector2(0f, -10f), 0.03f, 0.3f);
            Assert.Greater(fall.y, 1f);
            Assert.Less(fall.x, 1f);
            Vector2 huge = DeathBurstMath.StretchScale(new Vector2(0f, -999f), 0.03f, 0.3f);
            Assert.AreEqual(1.3f, huge.y, 1e-4f); // max ile sınırlı
        }

        [Test]
        public void SquashAmountAt_BaslangictaTamGenlik_YaylanarakSoner()
        {
            Assert.AreEqual(0.3f, DeathBurstMath.SquashAmountAt(0.3f, 0f, 14f, 9f), 1e-5f);
            Assert.AreEqual(0f, DeathBurstMath.SquashAmountAt(0.3f, -1f, 14f, 9f), 1e-5f);
            Assert.Less(Mathf.Abs(DeathBurstMath.SquashAmountAt(0.3f, 2f, 14f, 9f)), 1e-3f);
            // yarım periyoda yakın bir anda yön değiştirir (yaylanma)
            Assert.Less(DeathBurstMath.SquashAmountAt(0.3f, Mathf.PI / 14f * 1.1f, 14f, 1f), 0f);
        }

        [Test]
        public void SquashScale_CarpmaEkseniYassilir_DikEksenGenisler()
        {
            Vector2 v = DeathBurstMath.SquashScale(0.3f, true);
            Assert.Less(v.y, 1f);
            Assert.Greater(v.x, 1f);
            Vector2 h = DeathBurstMath.SquashScale(0.3f, false);
            Assert.Less(h.x, 1f);
            Assert.Greater(h.y, 1f);
        }

        [Test]
        public void CombineScale_Sikistirir()
        {
            Vector2 c = DeathBurstMath.CombineScale(new Vector2(2f, 0.1f), new Vector2(2f, 1f), 0.6f, 1.5f);
            Assert.AreEqual(1.5f, c.x, 1e-5f);
            Assert.AreEqual(0.6f, c.y, 1e-5f);
        }

        [Test]
        public void QuantizeScale_TamPikselOlculer()
        {
            Vector2 q = DeathBurstMath.QuantizeScale(new Vector2(1.13f, 0.87f), 11);
            Assert.AreEqual(Mathf.Round(1.13f * 11f) / 11f, q.x, 1e-5f);
            Assert.AreEqual(0f, (q.x * 11f) - Mathf.Round(q.x * 11f), 1e-4f);
            Assert.AreEqual(new Vector2(1.13f, 0.87f), DeathBurstMath.QuantizeScale(new Vector2(1.13f, 0.87f), 0));
        }

        [Test]
        public void SlotCount_EnAzBir_GenislikleArtar()
        {
            Assert.AreEqual(1, DeathBurstMath.SlotCount(0.5f, 0.7f, 0.45f));
            Assert.AreEqual(2, DeathBurstMath.SlotCount(1.15f, 0.7f, 0.45f));
            Assert.AreEqual(6, DeathBurstMath.SlotCount(3f, 0.7f, 0.45f));
        }

        [Test]
        public void SlotNear_OlumNoktasinaEnYakinYuva_SinirdaKisilir()
        {
            // havuz x: 10..13, kafa 0.7, aralık 0.45 -> yuva merkezleri 10.35, 10.8, ...
            int n = DeathBurstMath.SlotCount(3f, 0.7f, 0.45f);
            Assert.AreEqual(0, DeathBurstMath.SlotNear(5f, 10f, 0.7f, 0.45f, n));
            Assert.AreEqual(n - 1, DeathBurstMath.SlotNear(99f, 10f, 0.7f, 0.45f, n));
            int mid = DeathBurstMath.SlotNear(11.7f, 10f, 0.7f, 0.45f, n);
            Assert.AreEqual(DeathBurstMath.SlotX(mid, 10f, 0.7f, 0.45f), 11.7f, 0.25f);
        }

        [Test]
        public void SlotX_SolKenardanKafaYariCapiIceride()
        {
            Assert.AreEqual(10.35f, DeathBurstMath.SlotX(0, 10f, 0.7f, 0.45f), 1e-5f);
            Assert.AreEqual(10.8f, DeathBurstMath.SlotX(1, 10f, 0.7f, 0.45f), 1e-5f);
        }

        [Test]
        public void PickSlotNear_TercihtenDisaDogruDoldurur()
        {
            var used = new bool[5];
            var order = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 5; i++)
            {
                int s = DeathBurstMath.PickSlotNear(used, 1);
                order.Add(s);
                used[s] = true;
            }
            Assert.AreEqual(1, order[0]);
            Assert.AreEqual(2, order[1]); // önce sağ komşu
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, order);
            Assert.AreEqual(-1, DeathBurstMath.PickSlotNear(used, 1));
        }

        [Test]
        public void RestY_DibeInmez_YuzeyinAltindaKalir()
        {
            float y = DeathBurstMath.RestY(10f, 4f, 0.65f);
            Assert.AreEqual(7.4f, y, 1e-4f);
            Assert.Less(y, 10f);
            Assert.Greater(y, 10f - 4f);
        }

        [Test]
        public void HeadTint_DerindeDahaYesil_OturmaylaBuyur()
        {
            Assert.Greater(DeathBurstMath.HeadTint(0.65f, 0.3f, 0.8f, 5f, 0.6f), DeathBurstMath.HeadTint(0.35f, 0.3f, 0.8f, 5f, 0.6f));
            Assert.AreEqual(0f, DeathBurstMath.HeadTint(0.5f, 0.3f, 0.8f, 0f, 0.6f), 1e-5f);
        }

        [Test]
        public void EaseToward_BaslangictanHedefeYaklasir()
        {
            Assert.AreEqual(5f, DeathBurstMath.EaseToward(5f, -2f, 3f, 0f), 1e-5f);
            Assert.AreEqual(-2f, DeathBurstMath.EaseToward(5f, -2f, 3f, 10f), 1e-3f);
        }
    }
}
