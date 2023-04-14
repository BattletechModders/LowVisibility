using IRBTModUtils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowVisibilityTests
{
    [TestClass]
    public static class TestGlobalInit
    {
        [AssemblyInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            LowVisibility.Mod.Log = new DeferringLogger(testContext.TestResultsDirectory,
                "lowvis_tests", "LVT", true, true);

            LowVisibility.Mod.Config = new LowVisibility.ModConfig();
        }

        [AssemblyCleanup]
        public static void TearDown()
        {

        }
    }
}
