using LowVisibility;
using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowVisibilityTests
{
    [TestClass]
    public class StealthSignatureTests
    {
        [TestMethod]
        public void TestStealthSignatureMod()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(-0.2f, targetState.StealthSignatureMod(attackerState));
        }

        [TestMethod]
        public void TestStealthSignatureModWithProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(-0.15f, targetState.StealthSignatureMod(attackerState));
        }

        [TestMethod]
        public void TestStealthSignatureModWithPingedByProbe()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            target.StatCollection.Set(ModStats.PingedByProbe, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(-0.15f, targetState.StealthSignatureMod(attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_Stealth_Minus_20pct()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            EWState attackerState = new EWState(attacker);

            Assert.AreEqual(0.8f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_Stealth_Minus_20pct_Pinged()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            // Reduce by 0.05 x 3
            target.StatCollection.Set(ModStats.PingedByProbe, 3);

            EWState attackerState = new EWState(attacker);

            Assert.AreEqual(0.95f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        public void TestTargetSignature_Stealth_Minus_20pct_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            // Reduce by 0.05 x 2
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            EWState attackerState = new EWState(attacker);

            Assert.AreEqual(0.9f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        public void TestTargetSignature_Stealth_Minus_20pct_Pinged_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            // Reduce by 0.05 x 3
            target.StatCollection.Set(ModStats.PingedByProbe, 3);

            // Reduce by 0.05 x 2
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            EWState attackerState = new EWState(attacker);

            Assert.AreEqual(0.8f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_Stealth_Plus20pct()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "-0.20_2_1_2_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1.2f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }
    }
}
