namespace New_Scripts.Player.Nodes.Rotation
{
    /// <summary>
    /// Bir node etrafında tam 360 derece tamamlandığında tetiklenecek efekti tanımlayan arayüz.
    /// Yeni efekt türleri eklemek için bu arayüzü implemente edin; mevcut koda dokunmayın.
    /// </summary>
    public interface IFullRotationEffect
    {
        void OnFullRotationCompleted();
    }
}