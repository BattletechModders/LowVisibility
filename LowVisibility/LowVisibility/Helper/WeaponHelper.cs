namespace LowVisibility.Helper
{
    public class WeaponHelper
    {

        // We assume range-based effects are encoded as a float[] with 5 spaces;
        //  < minimum range, < short range, < medium range, < long range, < maxRange
        // return of -1 is an error
        public static int GetRangeIndex(Weapon weapon, float distance)
        {
            int rangeIdx = -1;

            if (distance < weapon.MinRange) { rangeIdx = 0; }
            if (distance < weapon.ShortRange) { rangeIdx = 1; }
            if (distance < weapon.MediumRange) { rangeIdx = 2; }
            if (distance < weapon.LongRange) { rangeIdx = 3; }
            if (distance < weapon.MaxRange) { rangeIdx = 4; }

            return rangeIdx;
        }
    }
}
