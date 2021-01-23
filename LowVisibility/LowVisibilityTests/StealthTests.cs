using BattleTech;
using LowVisibility;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LowVisibilityTests
{
    [TestClass]
    public class StealthTests
    {
        [TestMethod]
        public void TestStealthSignatureMod()
        {
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.2f, targetState.StealthSignatureMod(attackerState));
        }

        [TestMethod]
        public void TestStealthSignatureModWithProbeCarrier()
        {
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.15f, targetState.StealthSignatureMod(attackerState));
        }

        [TestMethod]
        public void TestStealthSignatureModWithPingedByProbe()
        {
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.20_2_1_2_3");

            target.StatCollection.Set(ModStats.PingedByProbe, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.15f, targetState.StealthSignatureMod(attackerState));
        }
    }
}
