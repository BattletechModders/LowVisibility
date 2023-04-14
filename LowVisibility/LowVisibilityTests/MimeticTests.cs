using LowVisibility;
using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace LowVisibilityTests
{
    [TestClass]
    public class MimeticTests
    {

        [TestMethod]
        public void TestMimetic_Visibility_NoDecay()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.6f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_NoDecay_Pinged()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.8f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_NoDecay_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 3);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.9f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_NoDecay_Pinged_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 3);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_DecayOneStep()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(90f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.7f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_DecayOneStep_OnBorder()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(60f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.7f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        [TestMethod]
        public void TestMimetic_Visibility_DecayThreeStep()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(180f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.9f, VisualLockHelper.GetTargetVisibility(target, attackerState), 0.001);
        }

        // Attack modifier results
        [TestMethod]
        public void TestMimetic_AttackMod_NoDecay()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(4, targetState.MimeticAttackMod(attackerState));
        }

        [TestMethod]
        public void TestMimetic_AttackMod_NoDecay_Pinged()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(2, targetState.MimeticAttackMod(attackerState));
        }

        [TestMethod]
        public void TestMimetic_AttackMod_NoDecay_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 1);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(3, targetState.MimeticAttackMod(attackerState));
        }

        [TestMethod]
        public void TestMimetic_AttackMod_NoDecay_Pinged_ProbeCarrier()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(0f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);
            attacker.StatCollection.Set(ModStats.ProbeCarrier, 3);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");
            target.StatCollection.Set(ModStats.PingedByProbe, 2);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0, targetState.MimeticAttackMod(attackerState));
        }

        [TestMethod]
        public void TestMimetic_AttackMod_DecayOneStep()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(90f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_1_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(3, targetState.MimeticAttackMod(attackerState));
        }

        [TestMethod]
        public void TestMimetic_AttackMod_DecayOneStep_FractionalAtkMod()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            target.CurrentPosition = new Vector3(90f, 0, 0);
            Traverse previousPositionT = Traverse.Create(target).Field("previousPosition");
            previousPositionT.SetValue(new Vector3(0f, 0f, 0f));

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);
            target.StatCollection.Set(ModStats.MimeticEffect, "4_0.10_0.5_2");

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(2, targetState.MimeticAttackMod(attackerState));
        }
    }
}
