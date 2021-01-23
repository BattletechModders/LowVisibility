using BattleTech;
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

        //[TestMethod]
        //public void TestSignature_WhenShutdown()
        //{
        //    Mech attacker = TestHelper.TestMech();
        //    Mech target = TestHelper.TestMech();
        //    target.IsShutDown = false;

        //    EWState attackerState = new EWState(attacker);
        //    EWState targetState = new EWState(target);

        //    Assert.AreEqual(0.5f, SensorLockHelper.Get);
        //}


    }
}
