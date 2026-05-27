using UnityEngine;

namespace New_Scripts.Player.Nodes.Rotation
{
    /// <summary>
    /// Bir pivot etrafındaki açısal ilerlemeyi frame-safe şekilde takip eden saf hesap sınıfı.
    /// Unity bağımlılığı yoktur; herhangi bir state içinde new'lenebilir.
    /// 
    /// Tasarım kararı: Açı delta'sı SignedAngle ile alınır, bu sayede hem saat yönü hem de
    /// ters yön dönüşler doğru birikir ve 360 geçişinde sarma (wrap) hatası olmaz.
    /// </summary>
    public class FullRotationTracker
    {
        private readonly float _targetDegrees;
        private float _accumulatedDegrees;
        private Vector2 _previousDirection;
        private bool _isInitialized;

        /// <param name="targetDegrees">Kaç derecelik birikim "tam dönüş" sayılacak. Genellikle 360f.</param>
        public FullRotationTracker(float targetDegrees = 360f)
        {
            _targetDegrees = targetDegrees;
        }

        /// <summary>
        /// Her FixedUpdate'te çağrılır. Dönüş tamamlandığında true döner (yalnızca bir kez).
        /// </summary>
        /// <param name="playerPos">Karakterin mevcut pozisyonu.</param>
        /// <param name="anchorPos">Döndüğü pivot noktası.</param>
        public bool Tick(Vector2 playerPos, Vector2 anchorPos)
        {
            Vector2 currentDirection = (playerPos - anchorPos).normalized;

            if (!_isInitialized)
            {
                _previousDirection = currentDirection;
                _isInitialized = true;
                return false;
            }

            float delta = Vector2.SignedAngle(_previousDirection, currentDirection);
            
            _accumulatedDegrees += delta; 
            _previousDirection = currentDirection;

            if (Mathf.Abs(_accumulatedDegrees) >= _targetDegrees)
            {
                _accumulatedDegrees = 0f;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tracker'ı başlangıç durumuna döndürür (örn: anchor değiştiğinde).
        /// </summary>
        public void Reset()
        {
            _accumulatedDegrees = 0f;
            _isInitialized = false;
        }

        /// <summary>0..1 arası ilerleme yüzdesi. UI veya VFX için kullanılabilir.</summary>
        public float Progress => Mathf.Clamp01(Mathf.Abs(_accumulatedDegrees) / _targetDegrees);
    }
}