using System.Collections.Generic;
using UnityEngine;
using New_Scripts.Player; // IPassenger arayüzünün olduğu yer

namespace New_Scripts.Platform
{
    /// <summary>
    /// Fiziksel platform hareketini ve üzerindeki yolcuları (Passenger) taşımayı yöneten kontrolcü.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DefaultExecutionOrder(-100)] // ÇOK KRİTİK: Karakterden ÖNCE çalışması garanti altına alındı
    public class PlatformController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private MovementStrategy movementStrategy;

        [Header("Passenger System")]
        [Tooltip("Sadece karakterin (Player) bulunduğu Layer'ı seçmelisin.")]
        [SerializeField] private LayerMask passengerLayer;
        [SerializeField] private Vector2 passengerBoxSize = new Vector2(1f, 0.1f);
        [SerializeField] private Vector2 passengerBoxOffset = new Vector2(0f, 0.5f);

        private Rigidbody2D _rigidbody2D;
        private Vector2 _previousPosition;
        private readonly Collider2D[] _passengerBuffer = new Collider2D[16];

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _previousPosition = _rigidbody2D.position;
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = movementStrategy.GetPositionAtTime(Time.time);
            Vector2 deltaPosition = newPosition - _previousPosition;

            // 1. Önce üzerimizdeki yolcuları bul
            HashSet<IPassenger> passengers = GetPassengers();

            // 2. Kendi Rigidbody'mizi taşı
            _rigidbody2D.MovePosition(newPosition);
            _previousPosition = newPosition;

            // 3. Tüm yolcuları sürükle (Kusursuz yapışma sağlar)
            foreach (var passenger in passengers)
            {
                passenger.MoveWithPlatform(deltaPosition);
            }
        }

        private HashSet<IPassenger> GetPassengers()
        {
            HashSet<IPassenger> passengers = new HashSet<IPassenger>();

            // Platformun hemen üstüne bir algılama kutusu atıyoruz
            Vector2 checkPosition = _rigidbody2D.position + passengerBoxOffset;
            int count = Physics2D.OverlapBoxNonAlloc(checkPosition, passengerBoxSize, 0f, _passengerBuffer, passengerLayer);

            for (int i = 0; i < count; i++)
            {
                if (_passengerBuffer[i].TryGetComponent(out IPassenger passenger))
                {
                    passengers.Add(passenger);
                }
            }

            return passengers;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
            
            // Editördeyken yolcu arama kutusunu yeşil renkle çizer. 
            // Bu kutuyu platformun tam yüzeyinin biraz üstüne hizalayacak şekilde offset/size ayarlarını yapmalısın.
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawWireCube(_rigidbody2D.position + passengerBoxOffset, passengerBoxSize);
        }
#endif
    }
}