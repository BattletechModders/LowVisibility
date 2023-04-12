
using System.Collections.Generic;

namespace LowVisibility
{

    public class ModText
    {

        // Map effect keys
        public const string LT_MAP_LIGHT_BRIGHT = "MAP_LIGHT_BRIGHT";
        public const string LT_MAP_LIGHT_DIM = "MAP_LIGHT_DIM";
        public const string LT_MAP_LIGHT_DARK = "MAP_LIGHT_DARK";
        public const string LT_MAP_FOG_LIGHT = "MAP_FOG_LIGHT";
        public const string LT_MAP_FOG_HEAVY = "MAP_FOG_HEAVY";
        public const string LT_MAP_SNOW = "MAP_SNOW";
        public const string LT_MAP_RAIN = "MAP_RAIN";

        // Map effects
        public Dictionary<string, string> MapEffects = new Dictionary<string, string>()
        {
            { LT_MAP_LIGHT_BRIGHT, "Bright" },
            { LT_MAP_LIGHT_DIM, "Dim" },
            { LT_MAP_LIGHT_DARK, "Dark" },
            { LT_MAP_FOG_LIGHT, "Fog" },
            { LT_MAP_FOG_HEAVY, "Dense Fog" },
            { LT_MAP_SNOW, "Snow" },
            { LT_MAP_RAIN, "Rain" }
        };

        // Status Panel
        public const string LT_PANEL_SENSORS = "PANEL_SENSORS";
        public const string LT_PANEL_VISUALS = "PANEL_VISUALS";
        public const string LT_PANEL_DETAILS = "PANEL_DETAILS";
        public const string LT_PANEL_HEAT = "PANEL_HEAT_VISION";
        public const string LT_PANEL_ZOOM = "PANEL_ZOOM_VISION";

        // Sensor details keys
        public const string LT_DETAILS_NONE = "DETAILS_NONE";
        public const string LT_DETAILS_LOCATION_AND_TYPE = "LOCATION_AND_TYPE";
        public const string LT_DETAILS_ARMOR_AND_WEAPON_TYPE = "ARMOR_AND_WEAPON_TYPE";
        public const string LT_DETAILS_STRUCT_AND_WEAPON_ID = "STRUCT_AND_WEAPON_ID";
        public const string LT_DETAILS_ALL_INFO = "ALL_INFO";
        public const string LT_DETAILS_UNKNOWN = "DETAILS_UNKNOWN";

        public const string LT_UNIT_TYPE_MECH = "UNIT_TYPE_MECH";
        public const string LT_UNIT_TYPE_VEHICLE = "UNIT_TYPE_VEHICLE";
        public const string LT_UNIT_TYPE_TURRET = "UNIT_TYPE_TURRET";

        // Status Panel
        public Dictionary<string, string> StatusPanel = new Dictionary<string, string>()
        {
            { LT_PANEL_SENSORS, "<b>Sensors</b><size=90%> <color=#{0}>{1:#.00}m</color> Multi:<color=#{2}> x{3}</color> [{4}]\n" },
            { LT_PANEL_VISUALS, "<b>Visuals</b><size=90%> <color=#00FF00>{0:0}m</color> Scan:{1}m [{2}]\n" },
            { LT_PANEL_DETAILS, "  Total: <color=#{0}>{1:+0;-#}</color><size=90%> Roll: <color=#{2}>{3:+0;-#}</color> Tactics: <color=#00FF00>{4:+0;-#}</color> AdvSen: <color=#{5}>{6:+0;-#}</color>\n" },
            { LT_PANEL_HEAT, "<b>Thermals</b><size=90%> Mod: <color=#{0}>{1:+0;-#}</color> / {2} heat Range: {3}m\n" },
            { LT_PANEL_ZOOM, "<b>Zoom</b><size=90%> Mod: <color=#{0}>{1:+0;-#}</color> Cap: <color=#{2}>{3:+0;-#}</color> Range: {4}m\n" },

            // Sensor Details Level
            { LT_DETAILS_NONE, "No Info" },
            { LT_DETAILS_LOCATION_AND_TYPE, "Type" },
            { LT_DETAILS_ARMOR_AND_WEAPON_TYPE, "Armor + Weapon Type" },
            { LT_DETAILS_STRUCT_AND_WEAPON_ID, "Struct + Weapon" },
            { LT_DETAILS_ALL_INFO, "All Info" },
            { LT_DETAILS_UNKNOWN, "Unknown" },

            { LT_UNIT_TYPE_MECH, "MECH" },
            { LT_UNIT_TYPE_VEHICLE, "VEHICLE" },
            { LT_UNIT_TYPE_TURRET, "TURRET" },
        };

        // Attack Tooltip
        public const string LT_ATTACK_FIRING_BLIND = "ATTACK_FIRE_BLIND";
        public const string LT_ATTACK_NO_VISUALS = "ATTACK_NO_VISUALS";
        public const string LT_ATTACK_ZOOM_VISION = "ATTACK_ZOOM_VISION";
        public const string LT_ATTACK_HEAT_VISION = "ATTACK_HEAT_VISION";
        public const string LT_ATTACK_MIMETIC = "ATTACK_MIMETIC_ARMOR";
        public const string LT_ATTACK_NO_SENSORS = "ATTACK_NO_SENSORS";
        public const string LT_ATTACK_ECM_JAMMED = "ATTACK_ECM_JAMMED";
        public const string LT_ATTACK_ECM_SHIELD = "ATTACK_ECM_SHIELD";
        public const string LT_ATTACK_STEALTH = "ATTACK_STEALTH";
        public const string LT_ATTACK_NARCED = "ATTACK_NARCED";
        public const string LT_ATTACK_TAGGED = "ATTACK_TAGGED";

        public Dictionary<string, string> AttackModifiers = new Dictionary<string, string>()
        {
            // Attack Tooltip
            { LT_ATTACK_FIRING_BLIND, "FIRING BLIND" },
            { LT_ATTACK_NO_SENSORS, "NO SENSORS" },
            { LT_ATTACK_NO_VISUALS, "NO VISUALS" },
            { LT_ATTACK_ZOOM_VISION, "ZOOM VISION" },
            { LT_ATTACK_HEAT_VISION, "HEAT VISION" },
            { LT_ATTACK_MIMETIC, "MIMETIC ARMOR" },
            { LT_ATTACK_STEALTH, "STEALTH" },
            { LT_ATTACK_ECM_JAMMED, "ECM JAMMING" },
            { LT_ATTACK_ECM_SHIELD, "ECM SHIELD" },
            { LT_ATTACK_NARCED, "NARCED" },
            { LT_ATTACK_TAGGED, "TAGGED" }
        };

        // Floatie texts
        public const string LT_FLOATIE_ECM_SHIELD = "FLOATIE_ECM_SHIELDED";
        public const string LT_FLOATIE_ECM_JAMMED = "FLOATIE_ECM_JAMMED";

        public Dictionary<string, string> Floaties = new Dictionary<string, string>()
        {
            // Floatie text
            { LT_FLOATIE_ECM_JAMMED, "ECM JAMMING" },
            { LT_FLOATIE_ECM_SHIELD, "ECM SHIELDING" }
        };

        // Targeting computer 
        public const string LT_TARG_COMP_BALLISTIC = "TARG_COMP_BALLISTIC";
        public const string LT_TARG_COMP_ENERGY = "TARG_COMP_ENERGY";
        public const string LT_TARG_COMP_MISSILE = "TARG_COMP_MISSILE";
        public const string LT_TARG_COMP_PHYSICAL = "TARG_COMP_PHYSICAL";
        public const string LT_TARG_COMP_UNIDENTIFIED = "TARG_COMP_UNIDENTIFIED";

        public Dictionary<string, string> TargetingComputer = new Dictionary<string, string>()
        {
            // Targeting computer text
            { LT_TARG_COMP_BALLISTIC, "Ballistic" },
            { LT_TARG_COMP_ENERGY, "Energy" },
            { LT_TARG_COMP_MISSILE, "Missile" },
            { LT_TARG_COMP_PHYSICAL, "Physical" },
            { LT_TARG_COMP_UNIDENTIFIED, "Unidentified" }
        };

        // HUD ToolTips
        public const string LT_TT_TITLE_VISION_AND_SENSORS = "TOOLTIP_TITLE_VISION_AND_SENSORS";
        public const string LT_TT_TITLE_SENSORS_DISABLED = "TOOLTIP_TITLE_SENSORS_DISABLED";
        public const string LT_TT_TEXT_SENSORS_DISABLED = "TOOLTIP_TEXT_SENSORS_DISABLED";
        public const string LT_TT_TITLE_EW = "TOOLTIP_TITLE_EW";
        public const string LT_TT_TEXT_EW_ECM_SHIELD = "TOOLTIP_LABEL_EW_ECM_SHIELD";
        public const string LT_TT_TEXT_EW_ECM_JAMMING = "TOOLTIP_LABEL_EW_ECM_JAMMING";
        public const string LT_TT_TEXT_EW_PROBE_CARRIER = "TOOLTIP_LABEL_EW_PROBE_CARRIER";
        public const string LT_TT_TEXT_EW_MIMETIC = "TOOLTIP_LABEL_EW_MIMETIC";
        public const string LT_TT_TEXT_EW_STEALTH = "TOOLTIP_LABEL_EW_STEALTH";
        public const string LT_TT_TEXT_EW_NARC_EFFECT = "TOOLTIP_LABEL_EW_NARC_EFFECT";
        public const string LT_TT_TEXT_EW_PROBE_EFFECT = "TOOLTIP_LABEL_EW_PROBE_EFFECT";
        public const string LT_TT_TEXT_EW_TAG_EFFECT = "TOOLTIP_LABEL_EW_TAG_EFFECT";

        public Dictionary<string, string> Tooltips = new Dictionary<string, string>()
        {
            // HUD Tooltips
            { LT_TT_TITLE_VISION_AND_SENSORS, "VISION AND SENSORS" },
            { LT_TT_TITLE_SENSORS_DISABLED, "SENSORS OFFLINE" },
            { LT_TT_TEXT_SENSORS_DISABLED, "Sensors offline during the first round of the battle." },
            
            // Electronic Warfare options
            { LT_TT_TITLE_EW, "ELECTRONIC WARFARE" },
            { LT_TT_TEXT_EW_ECM_SHIELD, "<size=90%><b>ECM SHIELD</b>: <color=#{0}>{1:+0;-#}</color> to detect or attack.\n" },
            { LT_TT_TEXT_EW_ECM_JAMMING, "<size=90%><b>ECM JAMMING</b>: <color=#{0}>{1:+0;-#} sensor details.</color>\n" },
            { LT_TT_TEXT_EW_PROBE_CARRIER, "<size=90%><b>ACTIVE PROBE</b>: <color=#{0}>{1:+0;-#}</color> to enemy ECM, Stealth, Mimetic\n" },
            { LT_TT_TEXT_EW_NARC_EFFECT, "<size=90%><b>NARCED</b>: <color=#{0}>{1:+0;-#}</color> to attack and detect.\n" },
            { LT_TT_TEXT_EW_PROBE_EFFECT, "<size=90%><b>PROBE PINGED</b>: <color=#{0}>{1:+0;-#}</color> to detect.\n" },
            { LT_TT_TEXT_EW_TAG_EFFECT, "<size=90%><b>TAGGED</b>: <color=#{0}>{1:+0;-#}</color> attack and detection modifier.\n" },
            { LT_TT_TEXT_EW_MIMETIC, "<size=90%><b>MIMETIC</b>: <color=#{0}>{1:+0;-#}</color> attack modifier.\n" },
            { LT_TT_TEXT_EW_STEALTH, "<size=90%><b>STEALTH</b>: med. <color=#{0}>{1:+0;-#}</color> / long <color=#{0}>{2:+0;-#}</color> / ext. <color=#{0}>{3:+0;-#}</color> attack modifier\n" }
        };

        // CAC TargetInfo strings
        public const string LT_CAC_SIDEPANEL_TITLE = "SIDEPANEL_TITLE";
        public const string LT_CAC_SIDEPANEL_MOVE_MECH = "SIDEPANEL_MOVE_MECH";
        public const string LT_CAC_SIDEPANEL_MOVE_VEHICLE = "SIDEPANEL_MOVE_VEHICLE";
        public const string LT_CAC_SIDEPANEL_DIST = "SIDEPANEL_DISTANCE";
        public const string LT_CAC_SIDEPANEL_HEAT = "SIDEPANEL_HEAT";
        public const string LT_CAC_SIDEPANEL_STAB = "SIDEPANEL_DAMAGE";
        public const string LT_CAC_SIDEPANEL_WEIGHT = "SIDEPANEL_WEIGHT";

        public Dictionary<string, string> CACSidePanel = new Dictionary<string, string>()
        {
            // CAC Sidepanel strings
            { LT_CAC_SIDEPANEL_TITLE, "  {0} {1}\n" },
            { LT_CAC_SIDEPANEL_MOVE_MECH, "  Walk: {0}m  Run: {1}m  Jump: {2}m\n" },
            { LT_CAC_SIDEPANEL_MOVE_VEHICLE, "  Cruise: {0}m  Flank: {1}m\n" },
            { LT_CAC_SIDEPANEL_DIST, "  Distance: {0}m\n" },
            { LT_CAC_SIDEPANEL_HEAT, "  Heat: {0} of {1}\n" },
            { LT_CAC_SIDEPANEL_STAB, "  Instability: {0} of {1}\n" },
            { LT_CAC_SIDEPANEL_WEIGHT, "{0} tons" }
        };

    }
}
