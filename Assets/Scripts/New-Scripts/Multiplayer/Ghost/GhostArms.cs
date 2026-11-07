using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayalet kollarının kopyalanacağı kaynak: oyuncunun kol pivotları ve kol sprite'ları (ArmController).</summary>
    public sealed class GhostArmSource
    {
        public Transform LeftPivot;
        public SpriteRenderer LeftSprite;
        public Transform RightPivot;
        public SpriteRenderer RightSprite;
    }

    /// <summary>
    /// Hayaletin kolları: yalnız o taraf bir yere tutunmuşken görünür, tutunduğu noktaya döner; ip kolun ucundan çıkar.
    /// Oyuncudaki ArmController girdi yönüne bakar; rakibin girdisi ağdan gelmediği için hayalet kancanın kendisine bakar.
    /// </summary>
    internal sealed class GhostArms
    {
        private sealed class Arm
        {
            public Transform Pivot;
            public SpriteRenderer Sprite;
            /// <summary>Pivottan kol ucuna dünya uzunluğu (Square sprite 1 birim genişlikte varsayılır).</summary>
            public float Length;
        }

        private Arm _left;
        private Arm _right;

        public static GhostArms Create(GhostArmSource source, Transform ghostRoot, Color tint, int sortingOrderOffset)
        {
            return new GhostArms
            {
                _left = CopyArm(source.LeftPivot, source.LeftSprite, ghostRoot, tint, sortingOrderOffset),
                _right = CopyArm(source.RightPivot, source.RightSprite, ghostRoot, tint, sortingOrderOffset)
            };
        }

        private static Arm CopyArm(Transform pivotSource, SpriteRenderer spriteSource, Transform ghostRoot, Color tint, int offset)
        {
            if (pivotSource == null || spriteSource == null) return null;

            var pivot = new GameObject(pivotSource.name).transform;
            pivot.SetParent(ghostRoot, false);
            pivot.localPosition = pivotSource.localPosition;
            pivot.localScale = pivotSource.localScale;

            var sprite = new GameObject(spriteSource.name).AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(pivot, false);
            sprite.transform.localPosition = spriteSource.transform.localPosition;
            sprite.transform.localRotation = spriteSource.transform.localRotation;
            sprite.transform.localScale = spriteSource.transform.localScale;
            sprite.sprite = spriteSource.sprite;
            sprite.sharedMaterial = spriteSource.sharedMaterial;
            sprite.sortingLayerID = spriteSource.sortingLayerID;
            sprite.sortingOrder = spriteSource.sortingOrder - offset;
            var c = spriteSource.color;
            sprite.color = new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, tint.a);
            sprite.enabled = false;

            float localTip = spriteSource.transform.localPosition.x + Mathf.Abs(spriteSource.transform.localScale.x) * 0.5f;
            return new Arm { Pivot = pivot, Sprite = sprite, Length = localTip * Mathf.Abs(pivotSource.localScale.x) };
        }

        public void Update(Vector2? leftAnchor, Vector2? rightAnchor, bool shown)
        {
            UpdateArm(_left, leftAnchor, shown);
            UpdateArm(_right, rightAnchor, shown);
        }

        private static void UpdateArm(Arm arm, Vector2? anchor, bool shown)
        {
            if (arm == null) return;
            bool visible = shown && anchor.HasValue;
            arm.Sprite.enabled = visible;
            if (!anchor.HasValue) return;

            Vector2 direction = anchor.Value - (Vector2)arm.Pivot.position;
            if (direction.sqrMagnitude > 1e-6f)
                arm.Pivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        /// <summary>İpin başlangıcı: o tarafın kolu varsa elin ucu, yoksa <paramref name="fallback"/>.</summary>
        public Vector2 RopeStart(bool left, Vector2? anchor, Vector2 fallback)
        {
            var arm = left ? _left : _right;
            if (arm == null || !anchor.HasValue) return fallback;
            Vector2 pivot = arm.Pivot.position;
            Vector2 direction = anchor.Value - pivot;
            if (direction.sqrMagnitude < 1e-6f) return pivot;
            return pivot + direction.normalized * Mathf.Min(arm.Length, direction.magnitude);
        }
    }
}
