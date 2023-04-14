using LowVisibility;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowVisibilityTests
{
    [TestClass]
    public class StealthTests
    {
        [TestMethod]
        public void TestStealthAttackMod_Bonuses()
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
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 90));
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 120));
            Assert.AreEqual(2, targetState.StealthAttackMod(attackerState, weapon, 200));
            Assert.AreEqual(2, targetState.StealthAttackMod(attackerState, weapon, 240));
            Assert.AreEqual(3, targetState.StealthAttackMod(attackerState, weapon, 400));
            Assert.AreEqual(3, targetState.StealthAttackMod(attackerState, weapon, 480));
        }

        [TestMethod]
        public void TestStealthAttackMod_Bonuses_Pinged()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_9_1_2_3");

            // Reduce by 3
            target.StatCollection.Set(ModStats.PingedByProbe, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Weapon weapon = TestHelper.BuildTestWeapon(0, 60, 120, 240, 480);

            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 30));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 60));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 90));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 120));
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 200));
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 240));
            Assert.AreEqual(2, targetState.StealthAttackMod(attackerState, weapon, 400));
            Assert.AreEqual(2, targetState.StealthAttackMod(attackerState, weapon, 480));
        }

        [TestMethod]
        public void TestStealthAttackMod_Bonuses_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_9_1_2_3");

            // Reduce by 2
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Weapon weapon = TestHelper.BuildTestWeapon(0, 60, 120, 240, 480);

            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 30));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 60));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 90));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 120));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 200));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 240));
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 400));
            Assert.AreEqual(1, targetState.StealthAttackMod(attackerState, weapon, 480));
        }

        [TestMethod]
        public void TestStealthAttackMod_Bonuses_Pinged_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_9_1_2_3");

            // Reduce by 3
            target.StatCollection.Set(ModStats.PingedByProbe, 3);

            // Reduce by 2
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Weapon weapon = TestHelper.BuildTestWeapon(0, 60, 120, 240, 480);

            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 30));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 60));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 90));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 120));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 200));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 240));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 400));
            Assert.AreEqual(0, targetState.StealthAttackMod(attackerState, weapon, 480));
        }

        [TestMethod]
        public void TestStealthAttackMod_Negatives()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_9_-1_-2_-3");

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

        [TestMethod]
        public void TestStealthDetailsMod_Positive()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_2_3_3_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(2, targetState.StealthDetailsMod());
        }

        [TestMethod]
        public void TestStealthDetailsMod_Negative()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // Stealth - <signature_modifier>_<details_modifier>_<mediumAttackMod>_<longAttackmod>_<extremeAttackMod>
            target.StatCollection.Set(ModStats.StealthEffect, "0.99_-2_3_3_3");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(-2, targetState.StealthDetailsMod());
        }

    }
}
