using BattleTech.Rendering.Mood;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibility.Helper
{
    public static class MoodHelper
    {
        public static bool IsFoggy(MoodController controller)
        {
            return IsLightFog(controller) || IsHeavyFog(controller);
        }

        public static bool IsLightFog(MoodController controller)
        {
            return MoodHasTag("mood_fogLight", controller);
        }


        public static bool IsHeavyFog(MoodController controller)
        {
            return MoodHasTag("mood_fogHeavy", controller);
        }

        private static bool MoodHasTag(string tag, MoodController controller)
        {
            MoodSettings moodSettings = controller?.CurrentMood;
            TagSet moodTags = moodSettings?.moodTags;
            foreach (string moodTag in moodTags)
            {
                if (moodTag.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

    }
}
