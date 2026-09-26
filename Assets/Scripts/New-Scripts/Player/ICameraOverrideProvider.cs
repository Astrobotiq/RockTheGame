namespace New_Scripts.Player
{
    public interface ICameraOverrideProvider
    {
        int Priority { get; }
        bool IsActive { get; }
        CameraOverrideSettings Settings { get; }
    }
}
