using BattleTech;
using LowVisibility;
using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LowVisibilityTests
{
    [TestClass]
    public class StealthTests
    {
        [TestMethod]
        public void TestStealthAttackMod_Short()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_9_1_2_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Weapon weapon = TestHelper.BuildTestWeapon(0, 60, 120, 240, 480);

            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 30)); 
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 60));
            Assert.AreEqual(-1, targetState.StealthAttackMod(attackerState, weapon, 90));
            Assert.AreEqual(-1, targetState.StealthAttackMod(attackerState, weapon, 120));
            Assert.AreEqual(-2, targetState.StealthAttackMod(attackerState, weapon, 200));
            Assert.AreEqual(-2, targetState.StealthAttackMod(attackerState, weapon, 240));
            Assert.AreEqual(-3, targetState.StealthAttackMod(attackerState, weapon, 400));
            Assert.AreEqual(-3, targetState.StealthAttackMod(attackerState, weapon, 480));
        }

        

    }
}
