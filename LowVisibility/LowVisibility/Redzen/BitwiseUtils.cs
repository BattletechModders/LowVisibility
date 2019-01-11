// Shamelessly stolen from https://github.com/colgreen/Redzen/blob/master/Redzen/BitwiseUtils.cs, 
//  thanks to the MIT License. See http://heliosphan.org/zigguratalgorithm/zigguratalgorithm.html for details.
namespace LowVisibility.Redzen {
    public static class BitwiseUtils {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(ulong x, int k) {
            // Note. RyuJIT will compile this to a single rotate CPU instruction (as of about .NET 4.6.1 and dotnet core 2.0).
            return (x << k) | (x >> (64 - k));
        }
    }
}
