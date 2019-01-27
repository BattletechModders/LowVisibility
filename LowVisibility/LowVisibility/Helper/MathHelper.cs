
using System;

namespace LowVisibility.Helper {
    public static class MathHelper {

        // Count the number of hexes (rounded up) in a range
        public static int CountHexes(float range, bool roundUp=true) {
            int hexes = 0;
            int count = 0;
            while (count <= range) {
                count += 30;
                hexes++;
            }

            if (hexes != 0 && !Equals(hexes * 30.0, range) && !roundUp) { 
                hexes = hexes - 1; 
            }
            //Console.WriteLine($"For range:{range} roundUp:{roundUp} = hexes:{hexes}");

            return hexes;
        }

        public static int CountSteps(float range, int step, bool roundUp=true) {
            int hexes = MathHelper.CountHexes(range, false);
            int steps = 0;
            int count = 0;
            while (count <= hexes) {
                count+= step;
                steps++;
            }

            if (steps != 0 && !roundUp) { steps = steps - 1; }

            Console.WriteLine($"For range:{range} hexes:{hexes} step:{step} = steps:{steps}");
            return steps;
        }

        public static int DecayingModifier(int start, int end, int step, float range) {
            int steps = CountSteps(range, step, false);
            int delta = Math.Sign(start) == Math.Sign(end) ?
                Math.Abs(Math.Abs(start) - Math.Abs(end)) : 
                Math.Abs(start) + Math.Abs(end);

            int stepMod = start < end ? steps : steps * -1;
            int mod = start + stepMod;

            if (Math.Abs(mod) > Math.Abs(end)) {
                mod = end;
            }

            return mod;
        }

    }

}
