using System;
using UnityEngine;

/// <summary>
/// Sistemler arası bağımlılık yaratmadan Transform verisi taşıyan yayın/abonelik (pub/sub) kanalıdır.
/// </summary>
[CreateAssetMenu(menuName = "Events/Transform Event Channel")]
public class TransformEventChannelSO : ScriptableObject
{
    public event Action<Transform> OnEventRaised;

    public void RaiseEvent(Transform value)
    {
        OnEventRaised?.Invoke(value);
    }
}