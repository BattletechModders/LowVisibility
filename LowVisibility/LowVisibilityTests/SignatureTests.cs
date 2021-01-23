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
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1.0f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_WhenShutdown()
        {
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            Traverse isShutdownT = Traverse.Create(target).Field("_isShutDown");
            isShutdownT.SetValue(true);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.5f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_Stealth_20pct()
        {
            Mech attacker = TestHelper.TestMech();
            Mech target = TestHelper.TestMech();

            Traverse isShutdownT = Traverse.Create(target).Field("_isShutDown");
            isShutdownT.SetValue(true);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.5f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }


    }
}
