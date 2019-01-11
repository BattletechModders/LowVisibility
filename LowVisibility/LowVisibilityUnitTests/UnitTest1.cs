
using LowVisibility;
using LowVisibility.Redzen;
using NUnit.Framework;
using System;

namespace LowVisibilityUnitTests {
    [TestFixture]
    public class UnitTest1 {
        [Test]
        public void TestMethod1() {
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = 0;
            double stdDev = 3;
            double[] sampleBuf = new double[256];
            ZigguratGaussian.Sample(rng, mean, stdDev, sampleBuf);
            Console.WriteLine("Printing samples.");
            foreach (double sample in sampleBuf) {
                Assert.True(sample != 0);
                Console.WriteLine($"Sample is: {sample}");
            }
        }

        /*
         *         public static void Sample(IRandomSource rng, double mean, double stdDev, double[] buf) {
            for (int i = 0; i <= buf.Length; i++) {
                buf[i] = mean + (Sample(rng) * stdDev);
            }
        }*/
    }
}
