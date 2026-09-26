namespace New_Scripts.Death
{
    /// <summary>
    /// Oyuncu öldüğünde varsayılan durumuna sıfırlanması gereken bileşenler için arayüz.
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// Bileşeni varsayılan başlangıç değerlerine sıfırlar.
        /// </summary>
        void ResetToDefault();
    }
}
