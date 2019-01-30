using LowVisibility.Helper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LVUnitTests {

    [TestFixture]
    public class DivisionVsIterationTest {

        [Test]
        public void HexCounter() {
            Assert.AreEqual(0, MathHelper.CountHexes(0f, true));
            Assert.AreEqual(0, MathHelper.CountHexes(0f, false));

            Assert.AreEqual(1, MathHelper.CountHexes(1f, true));
            Assert.AreEqual(0, MathHelper.CountHexes(1f, false));

            Assert.AreEqual(1, MathHelper.CountHexes(29f, true));
            Assert.AreEqual(0, MathHelper.CountHexes(29f, false));

            Assert.AreEqual(1, MathHelper.CountHexes(30f, true));
            Assert.AreEqual(1, MathHelper.CountHexes(30f, false));

            Assert.AreEqual(2, MathHelper.CountHexes(31f, true));
            Assert.AreEqual(1, MathHelper.CountHexes(31f, false));
        }

        [Test]
        public void DivVsIter() {
            List<float> ranges = RangesInMeters();
            List<int> divHexes = new List<int>();
            // Division
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (float range in ranges) {
                divHexes.Add(RangeToHexByDiv(range));
            }

            stopWatch.Stop();
            long divTime = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"Division time was:{divTime}ms");

            // Iteration
            List<int> iterHexes = new List<int>();
            stopWatch.Reset();
            stopWatch.Start();
            foreach (float range in ranges) {
                iterHexes.Add(MathHelper.CountHexes(range));
            }

            stopWatch.Stop();
            long iterTime = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"Iteration time was:{iterTime}ms");

            float[] rangeArr = ranges.ToArray();
            int[] divArray = divHexes.ToArray();
            int[] iterArray = iterHexes.ToArray();
            Console.WriteLine($"Testing array lengths");
            Assert.AreEqual(divArray.Length, iterArray.Length);

            Console.WriteLine($"Testing array values");
            for (int i = 0; i < divArray.Length; i++) {
                Console.WriteLine($"range:{rangeArr[i]} div:{divArray[i]} iter:{iterArray[i]}");
                Assert.AreEqual(divArray[i], iterArray[i]);
            }

            //Console.WriteLine($"Testing execution time");
            //Assert.AreEqual(divTime, iterTime);
        }

        private int RangeToHexByDiv(float range) {
            int hexes = 0;
            if (range % 30.0 == 0) {
                hexes = (int)Math.Floor(range / 30.0);
            } else {
                hexes = (int)Math.Floor(range / 30);
                hexes = hexes + 1;
            }
            return hexes;
        }

        private List<float> RangesInMeters() {
            List<float> ranges = new List<float>();
            Random rand = new Random();
            for (int i = 0; i < 1000; i++) {
                float range = rand.Next(1, 500);
                ranges.Add(range);
            }

            return ranges;
        }
    }
}
