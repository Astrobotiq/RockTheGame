using UnityEngine;

namespace New_Scripts.Player
{
    public interface ICameraOverrideProvider
    {
        int Priority { get; }
        bool IsActive { get; }
        CameraOverrideSettings Settings { get; }

        /// <summary>FrameArea çerçevesinin yerel olduğu uzay (genelde sağlayıcının kendi transform'u).</summary>
        Transform FrameSpace { get; }
    }

    /// <summary>
    /// Aktifliğini oyuncunun konumundan türeten sağlayıcı (bölgeler). CameraController her karede çağırır;
    /// kayıt OnEnable/OnDisable'dadır, oda geçişi ya da ışınlama kaydı silmez.
    /// </summary>
    public interface IPositionCameraOverride : ICameraOverrideProvider
    {
        void UpdateActive(Vector2 playerPosition);
    }
}
