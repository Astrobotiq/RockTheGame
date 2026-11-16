namespace New_Scripts.Player
{
    /// <summary>
    /// Kose duzeltmesinde denenecek kaydirma adaylari (docs/systems/movement-core.md "Kose duzeltmesi").
    /// Celeste'teki gibi en yakin aday once denenir; Celeste piksel piksel 1..4 dener, burada mesafe Steps parcaya bolunur.
    /// </summary>
    public static class CornerCorrectionMath
    {
        public const int Steps = 4;
        public const int MaxCandidates = Steps * 2;

        /// <summary>
        /// Adaylari yakindan uzaga buffer'a yazar, sayisini dondurur. Her mesafede once preferredSign yonu,
        /// allowOpposite ise ardindan ters yon denenir. maxDistance &lt;= 0 ise aday yoktur.
        /// </summary>
        public static int FillOffsets(float maxDistance, int preferredSign, bool allowOpposite, float[] buffer)
        {
            if (maxDistance <= 0f) return 0;
            int sign = preferredSign >= 0 ? 1 : -1;
            int count = 0;
            for (int i = 1; i <= Steps; i++)
            {
                float distance = maxDistance * i / Steps;
                buffer[count++] = sign * distance;
                if (allowOpposite) buffer[count++] = -sign * distance;
            }
            return count;
        }
    }
}
