using LowVisibility.Helper;
using LowVisibility.Object;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowVisibilityTests
{
    [TestClass]
    public class VisibilityTests
    {

        [TestMethod]
        public void TestVisibility_NoEffects()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(1.0f, VisualLockHelper.GetTargetVisibility(target, attackerState));
        }

        [TestMethod]
        public void TestVisibility_Shutdown()
        {
            Mech attacker = TestHelper.BuildTestMech();
            Mech target = TestHelper.BuildTestMech();

            attacker.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", 1f);
            attacker.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0f);

            target.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", 1f);
            target.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0f);

            Traverse isShutdownT = Traverse.Create(target).Field("_isShutDown");
            isShutdownT.SetValue(true);

            EWState attackerState = new EWState(attacker);
            EWState targetState = new EWState(target);

            Assert.AreEqual(0.5f, VisualLockHelper.GetTargetVisibility(target, attackerState));
        }

    }
}
