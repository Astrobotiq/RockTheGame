using System;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Karakterin yaşam döngüsünü yönetir, arayüz sözleşmesini yerine getirir ve ölüm anında dış sistemleri bilgilendirir.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IKillable
    {
        public event Action OnDeath;

        public void Kill()
        {
            gameObject.SetActive(false);
            OnDeath?.Invoke();
        }
    }
}