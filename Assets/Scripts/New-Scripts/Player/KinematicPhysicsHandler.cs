using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class KinematicPhysicsHandler : MonoBehaviour, IPassenger
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float skinWidth = 0.02f;
        [SerializeField] private float groundedDistance = 0.05f;
        [SerializeField] private float boxShrinkOffset = 0.1f;
        [SerializeField] private float movementThreshold = 0.001f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingLeftWall { get; private set; }
        public bool IsTouchingRightWall { get; private set; }
        public bool IsTouchingCeiling { get; private set; }

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

        private Vector2 _platformDelta;

        // --- TEŞHİS DEĞİŞKENLERİ ---
        private Vector2 _debugPenetrationFix;
        private bool _debugIsPushedHorizontally;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
        }

        public void MoveWithPlatform(Vector2 deltaPosition)
        {
            _platformDelta += deltaPosition;
        }

        public Vector2 Move(Vector2 deltaMovement)
        {
            // FIX: Platform deltasını BURADA EKMİYORUZ. Karakter, platformun fiziksel
            // olarak bulunduğu eski "hayalet" noktadan hesaplamalara başlıyor.
            Vector2 position = _body.position;

            // 1. Overlap Çözümü (Eski konumda iç içe girme varsa temizle)
            ResolvePenetrations(ref position);

            // 2. Yatay Hareket
            deltaMovement = ResolveHorizontalCollisions(position, deltaMovement);
            position.x += deltaMovement.x;

            // 3. Dikey Hareket (Kusursuz yüzey teması burada gerçekleşir)
            deltaMovement = ResolveVerticalCollisions(position, deltaMovement);
            position.y += deltaMovement.y;

            // 4. Sensörleri tam bu uyumlu konumdayken güncelle
            UpdateSensors(position);

            // FIX ASIL BURASI: Tüm fizik ve sensör işleri bittikten SONRA, 
            // platformun bizi ittiği mesafeyi ekleyip karakteri öyle ışınlıyoruz.
            position += _platformDelta;
            _platformDelta = Vector2.zero;

            _body.MovePosition(position);

            return deltaMovement / Time.fixedDeltaTime;
        }
        

        private void ResolvePenetrations(ref Vector2 position)
        {
            int overlapCount = Physics2D.OverlapBoxNonAlloc(position + _boxCollider.offset, _boxCollider.bounds.size,
                0f, _overlapBuffer, groundLayerMask);
            for (int i = 0; i < overlapCount; i++)
            {
                Collider2D overlap = _overlapBuffer[i];
                if (overlap == _boxCollider || overlap.isTrigger) continue;

                ColliderDistance2D distance = Physics2D.Distance(_boxCollider, overlap);
                if (distance.isOverlapped)
                {
                    position += distance.normal * distance.distance;
                }
            }
        }

        private Vector2 ResolveHorizontalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.x) < movementThreshold) return movement;

            float directionX = Mathf.Sign(movement.x);
            float distance = Mathf.Abs(movement.x) + skinWidth;
            Vector2 boxSize = _boxCollider.bounds.size;
            boxSize.y -= boxShrinkOffset;

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f,
                new Vector2(directionX, 0f), _hitBuffer, distance, groundLayerMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider.isTrigger) continue;
                if (_hitBuffer[i].distance < minDistance)
                {
                    minDistance = _hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit) movement.x = (minDistance - skinWidth) * directionX;
            return movement;
        }

        private Vector2 ResolveVerticalCollisions(Vector2 position, Vector2 movement)
        {
            if (Mathf.Abs(movement.y) < movementThreshold) return movement;

            float directionY = Mathf.Sign(movement.y);
            float distance = Mathf.Abs(movement.y) + skinWidth;
            Vector2 boxSize = _boxCollider.bounds.size;
            boxSize.x -= boxShrinkOffset;

            int hitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, boxSize, 0f,
                new Vector2(0f, directionY), _hitBuffer, distance, groundLayerMask);

            float minDistance = float.MaxValue;
            bool validHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].collider.isTrigger) continue;
                if (_hitBuffer[i].distance < minDistance)
                {
                    minDistance = _hitBuffer[i].distance;
                    validHit = true;
                }
            }

            if (validHit) movement.y = (minDistance - skinWidth) * directionY;
            return movement;
        }
        
        private void UpdateSensors(Vector2 position)
        {
            Vector2 horizontalBoxSize = _boxCollider.bounds.size;
            horizontalBoxSize.y -= boxShrinkOffset;

            int leftHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f,
                Vector2.left, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingLeftWall = HasValidSensorHit(leftHitCount);

            int rightHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, horizontalBoxSize, 0f,
                Vector2.right, _hitBuffer, skinWidth * 2f, groundLayerMask);
            IsTouchingRightWall = HasValidSensorHit(rightHitCount);

            Vector2 verticalBoxSize = _boxCollider.bounds.size;
            verticalBoxSize.x -= boxShrinkOffset;

            int groundHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f,
                Vector2.down, _hitBuffer, groundedDistance + skinWidth, groundLayerMask);
            IsGrounded = HasValidSensorHit(groundHitCount);

            int ceilingHitCount = Physics2D.BoxCastNonAlloc(position + _boxCollider.offset, verticalBoxSize, 0f,
                Vector2.up, _hitBuffer, groundedDistance + skinWidth, groundLayerMask);
            IsTouchingCeiling = HasValidSensorHit(ceilingHitCount);
        }


        private bool HasValidSensorHit(int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!_hitBuffer[i].collider.isTrigger) return true;
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _body == null) return;

            Vector2 pos = _body.position;

            // Penetrasyon düzeltmesi yatay eksene kaydıysa KIRMIZI ve kalın çizgiyle göster
            if (_debugIsPushedHorizontally)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos, pos + _debugPenetrationFix * 100f); // İtilme yönünü abartarak çiziyoruz

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontStyle = FontStyle.Bold;
                UnityEditor.Handles.Label(pos + Vector2.up, "HATA: YATAY İTİLME (KÖŞE SIKIŞMASI)", style);
            }
            // Sadece normal dikey gömülme varsa SARI ile ufak göster
            else if (Mathf.Abs(_debugPenetrationFix.y) > 0.0001f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, pos + _debugPenetrationFix * 10f);
            }
        }
#endif
    }
}