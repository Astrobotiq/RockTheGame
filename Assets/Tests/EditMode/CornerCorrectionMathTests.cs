using New_Scripts.Player;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Kose duzeltmesi aday sirasi (docs/systems/movement-core.md "Kose duzeltmesi").
    /// </summary>
    public class CornerCorrectionMathTests
    {
        private readonly float[] _buffer = new float[CornerCorrectionMath.MaxCandidates];

        [Test]
        public void FillOffsets_OneDirection_NearestFirst()
        {
            int count = CornerCorrectionMath.FillOffsets(0.5f, 1, false, _buffer);
            Assert.AreEqual(4, count);
            Assert.AreEqual(0.125f, _buffer[0], 1e-6f);
            Assert.AreEqual(0.25f, _buffer[1], 1e-6f);
            Assert.AreEqual(0.375f, _buffer[2], 1e-6f);
            Assert.AreEqual(0.5f, _buffer[3], 1e-6f);
        }

        [Test]
        public void FillOffsets_NegativeSign_GoesLeft()
        {
            CornerCorrectionMath.FillOffsets(0.5f, -1, false, _buffer);
            Assert.AreEqual(-0.125f, _buffer[0], 1e-6f);
            Assert.AreEqual(-0.5f, _buffer[3], 1e-6f);
        }

        [Test]
        public void FillOffsets_BothDirections_InterleavesPreferredFirst()
        {
            int count = CornerCorrectionMath.FillOffsets(0.5f, -1, true, _buffer);
            Assert.AreEqual(8, count);
            Assert.AreEqual(-0.125f, _buffer[0], 1e-6f);
            Assert.AreEqual(0.125f, _buffer[1], 1e-6f);
            Assert.AreEqual(-0.25f, _buffer[2], 1e-6f);
            Assert.AreEqual(0.5f, _buffer[7], 1e-6f);
        }

        [Test]
        public void FillOffsets_ZeroDistance_NoCandidates()
        {
            Assert.AreEqual(0, CornerCorrectionMath.FillOffsets(0f, 1, true, _buffer));
        }
    }
}
