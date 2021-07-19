using BattleTech;
using Harmony;
using LowVisibility;
using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LowVisibilityTests
{
    [TestClass]
    public class SignatureTests
    {

        [TestMethod]
        public void TestTargetSignature_NoModifiers()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1.0f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_WhenShutdown()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            Traverse isShutdownT = Traverse.Create(target).Field("_isShutDown");
            isShutdownT.SetValue(true);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.5f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_1pt()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 1);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);
            
            Assert.AreEqual(0.90f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_3pt()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 3);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.70f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_4pt()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 4);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.60f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_4pt_AttackerProbe2()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 4);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.80f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_4pt_PingedByProbe2()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 4);
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.80f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_ECMShield_3pt_AttackerProbe2_PingedByProbe2()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 3);
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }


    }
}
