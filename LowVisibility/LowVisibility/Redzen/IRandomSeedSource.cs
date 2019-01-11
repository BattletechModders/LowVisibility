// Shamelessly stolen from https://github.com/colgreen/Redzen/, 
//  thanks to the MIT License. See http://heliosphan.org/zigguratalgorithm/zigguratalgorithm.html for details.
namespace LowVisibility.Redzen {
    /// <summary>
    /// A source of seed values for use by pseudo-random number generators (PRNGs).
    /// </summary>
    public interface IRandomSeedSource {
        /// <summary>
        /// Get a new seed value.
        /// </summary>
        ulong GetSeed();
    }
}
