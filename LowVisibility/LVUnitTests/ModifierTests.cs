using LowVisibility.Helper;
using NUnit.Framework;

namespace LVUnitTests {

    [TestFixture]
    public class DecayingAttackTests {


        [Test]
        public void ZoomMod() {
            // 0 to 3 over 3 steps

            // No range, no modifier
            int mod = MathHelper.DecayingModifier(0, -3, 3, 0.0f);
            Assert.AreEqual(0, mod);

            // Range just under step count of 3 * 30 = 89.9
            mod = MathHelper.DecayingModifier(0, -3, 3, 89.9f);
            Assert.AreEqual(0, mod);

            // Range at first step count
            mod = MathHelper.DecayingModifier(0, -3, 3, 90.0f);
            Assert.AreEqual(-1, mod);

            // Range at second step count
            mod = MathHelper.DecayingModifier(0, -3, 3, 180.0f);
            Assert.AreEqual(-2, mod);

            // Range at third step count
            mod = MathHelper.DecayingModifier(0, -3, 3, 270.0f);
            Assert.AreEqual(-3, mod);

            // Range at fourth step count
            mod = MathHelper.DecayingModifier(0, -3, 3, 360.0f);
            Assert.AreEqual(-3, mod);
        }

        [Test]
        public void BrawlerMod() {
            // -3 to +3 over 6 steps

            // No range, full modifier
            int mod = MathHelper.DecayingModifier(-3, 3, 2, 0.0f);
            Assert.AreEqual(-3, mod);

            // First step (2*1*30), worse mod
            mod = MathHelper.DecayingModifier(-3, 3, 2, 60.0f);
            Assert.AreEqual(-2, mod);

            // Third step (2*3*30), neutral
            mod = MathHelper.DecayingModifier(-3, 3, 2, 180.0f);
            Assert.AreEqual(0, mod);

            // Fourth step (2*4*30), positive
            mod = MathHelper.DecayingModifier(-3, 3, 2, 240.0f);
            Assert.AreEqual(1, mod);

            // Sixth step (2*6*30), positive
            mod = MathHelper.DecayingModifier(-3, 3, 2, 360.0f);
            Assert.AreEqual(3, mod);

            // Seventh step (2*7*30), capped
            mod = MathHelper.DecayingModifier(-3, 3, 2, 420.0f);
            Assert.AreEqual(3, mod);
        }

        [Test]
        public void SniperMod() {
            // +3 to -3 over 6 steps

            // No range, full modifier
            int mod = MathHelper.DecayingModifier(3, -3, 2, 0.0f);
            Assert.AreEqual(3, mod);

            // First step (2*1*30), worse mod
            mod = MathHelper.DecayingModifier(3, -3, 2, 60.0f);
            Assert.AreEqual(2, mod);

            // Third step (2*3*30), neutral
            mod = MathHelper.DecayingModifier(3, -3, 2, 180.0f);
            Assert.AreEqual(0, mod);

            // Fourth step (2*4*30), positive
            mod = MathHelper.DecayingModifier(3, -3, 2, 240.0f);
            Assert.AreEqual(-1, mod);

            // Sixth step (2*6*30), positive
            mod = MathHelper.DecayingModifier(3, -3, 2, 360.0f);
            Assert.AreEqual(-3, mod);

            // Seventh step (2*7*30), capped
            mod = MathHelper.DecayingModifier(3, -3, 2, 420.0f);
            Assert.AreEqual(-3, mod);

        }



    }
}
