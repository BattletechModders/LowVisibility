using LowVisibility;
using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

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
        public void TestTargetSignature_ECMShield1()
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
        public void TestTargetSignature_ECMShield3()
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
        public void TestTargetSignature_ECMShield4()
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
        public void TestTargetSignature_ECMShield4_Probe2()
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
        public void TestTargetSignature_ECMShield4_Pinged2()
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
        public void TestTargetSignature_ECMShield3_Probe2_Pinged2()
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

        [TestMethod]
        public void TestTargetSignature_ECMShield3_Narc5()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 3);
            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.NarcEffect, "0.5_7_-2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // ECM shield = 3 * .1 => 0.3
            // Narc mod = 0.5 - ECMShield (0.3) => 0.2 
            // Sigs are multiplicative, so: (1.0 - 0.3) * (1.0 + 0.2) = 0.84
            Assert.AreEqual(0.84f, SensorLockHelper.GetTargetSignature(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestTargetSignature_Tag8()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // Tag = 0.8
            // Sigs are multiplicative, so (1.0 + 0.8) = 1.8 
            Assert.AreEqual(1.8f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

        [TestMethod]
        public void TestTargetSignature_Tag8_Mimetic6()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            // <maxCharges>_<visibilityModPerCharge>_<attackModPerCharge>_<hexesUntilDecay>
            target.StatCollection.Set(ModStats.MimeticEffect, "3_0.2_1_2");

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // Mimetic = 3 * 0.2 = 0.6 => 1.0 - 0.6 = 0.4 reduction
            // Tag = 0.8 - (1.0 - 0.6) = 0.2
            // Sigs are multiplicative, so (1.0 + 0.2) = 1.2
            Assert.AreEqual(1.2f, SensorLockHelper.GetTargetSignature(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestTargetSignature_Tag8_Mimetic6_ECMShield3()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            // <maxCharges>_<visibilityModPerCharge>_<attackModPerCharge>_<hexesUntilDecay>
            target.StatCollection.Set(ModStats.MimeticEffect, "3_0.2_1_2");

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 3);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // Mimetic = 3 * 0.2 = 0.6 => 1.0 - 0.6 = 0.4 reduction
            // Tag = 0.8 - (1.0 - 0.4) = 0.2
            // ECMShield = 0.3
            // Sigs are multiplicative, so (1.0 + 0.2) * (1.0 - 0.3) = 0.84
            Assert.AreEqual(0.84f, SensorLockHelper.GetTargetSignature(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestTargetSignature_Tag8_Mimetic6_ECMShield3_Probe2()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            // <maxCharges>_<visibilityModPerCharge>_<attackModPerCharge>_<hexesUntilDecay>
            target.StatCollection.Set(ModStats.MimeticEffect, "3_0.2_1_2");

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            target.StatCollection.Set(ModStats.ECMShield, 3);

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // Mimetic = (3charge - 2 probe) * 0.2 = 0.2 => 1.0 - 0.2 => 0.8 reduction
            // Tag = 0.8 - (1.0 - 0.8) = 0.6
            // ECMShield = 0.3 - 0.2 = 0.1
            // Sigs are multiplicative, so (1.0 + 0.6) * (1.0 - 0.1) = 1.44
            Assert.AreEqual(1.44f, SensorLockHelper.GetTargetSignature(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestTargetSignature_Tag8_Mimetic6_ECMShield3_Probe2_Pinged2()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            // <maxCharges>_<visibilityModPerCharge>_<attackModPerCharge>_<hexesUntilDecay>
            target.StatCollection.Set(ModStats.MimeticEffect, "3_0.2_1_2");

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            target.StatCollection.Set(ModStats.ECMShield, 3);

            attacker.StatCollection.Set(ModStats.ProbeCarrier, 2);

            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // Mimetic = (3charge - 2 probe - 2 ping) * 0.2 = 0 => 1.0 - 0 => 1.0 reduction
            // Tag = 0.8 - (1.0 - 1.0) = 0.8
            // ECMShield = 0.3 - 0.2 - 0.2 = 0
            // Sigs are multiplicative, so (1.0 + 0.8) * (1.0 - 0) = 1.8
            Assert.AreEqual(1.8f, SensorLockHelper.GetTargetSignature(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestTargetSignature_Tag8_ECMShield3()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            // ECM Shield
            target.StatCollection.Set(ModStats.ECMShield, 3);
            // <signatureMod>_<detailsMod>_<attackMod>
            target.StatCollection.Set(ModStats.TagEffect, "0.8_2_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            // ECM Shield = 3 * .1 => 0.3
            // Tag = 0.8
            // Sigs are multiplicative, so (1.0 - 0.3) * (1.0 + 0.8) = 1.26 
            Assert.AreEqual(1.26f, SensorLockHelper.GetTargetSignature(target, attackerState));
        }

    }
}
