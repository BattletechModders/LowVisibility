using IRBTModUtils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        [AssemblyCleanup]
        public static void TearDown()
        {

        }
    }
}
