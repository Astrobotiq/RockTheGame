namespace New_Scripts.Death
{
    /// <summary>
    /// Sahnede anında öldürülebilir veya yok edilebilir tüm nesnelerin (Oyuncu, düşman, kırılabilir obje) uygulaması gereken arayüz.
    /// </summary>
    public interface IKillable
    {
        void Kill();
    }
}