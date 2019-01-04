# Low Visibility
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that adds a modified version of the double-blind visibility rules from the MaxTech sourcebook. A brief list of features includes:
  * Instead of a flat sensor range, units make a check each turn to determine their effect sensor range. On a good roll, the unit will increase their sensor range to x1.5 or x2.5 their base range. On poor roll, the unit will be only able to detect units visually. A high tactics skill improves this roll.
  * Unit details are hidden and only revealed with better quality sensors. Visual identification will only give you the mech chassis, while basic sensors will tell you the weapons and evasion pips. Active probes are required to see where components are located, current heat and stability values.
  * ECM equipment emits a bubble that prevents sensors from operating properly. Enemy and neutral units within the bubble suffer significant penalties to their sensor checks which can cause them to be unable to detect opponents.
  * Active probes defeat ECM equipment, and provide comprehensive details on enemy targets.
  * The map environment influences the visibility range of units. Rain, fog and snow significantly reduce visual identification range, making sensors more important.

This mod was specifically designed to work with [RogueTech](http://roguetech.org). Running standalone should work, but has not been tested.

## Vanilla Behavior
Before talking about what this mod does, it's helpful to understand the vanilla behavior. In vanilla [HBS BattleTech](http://battletechgame.com/), the detection model allows for opposing units to be in one of four states:
  * Undetected when outside of sensor range
  * Detected as a blob (the down-pointing arrow)
  * Detected as a specific type (the mech, turret or vehicle silhouette)
  * Visual detection with full details displayed

The range of visual detection is determined by a base spotting distance, which defaults to 300 (set in SimGameConstants). This value is modified by the spotting unit's `SpottingVisibilityMultiplier` and `SpottingVisibilityAbsolute` values. This value is then modified by the targets's `SpottingVisibilityMultiplier` and `SpottingVisibilityAbsolute` values. If this modified spotting distance is less than the distance to the target, the spotter is considered to have _LineOfSight_ to the target. Once one spotter on a team has _LineOfSight_ to a target, all other team members share that visibility.

If a target is outside of direct _LineOfSight_, then sensors are checked. The detecting unit's `SensorDistanceMultiplier` is added to any terrain `DesignMask.sensorRangeMultiplier`, while `SensorDistanceAbsolute` is added to the result. The target's `SensorSignatureModifier` modifies this value, and the final result is compared against a sensor detection range of 500 (also set in SimGameConstants). If the target is within range, it's identified as a sensor blip. The type of blip is influenced by the tactics level of the spotter.

The __Sensor Lock__ ability provides _LineOfSight_ to the target model, even if it's only within sensor range.

## Mod Behavior

This mod re-uses the HBS concepts of __visibility__ and __detection__, but applies them in new ways. __Visibility__ is when the source unit has visual line of sight to a target, while __detection__ occurs when the source can identify the target on sensors. 

### Visibility

### Sensor Check

At the start of every combat round, every unit (player or AI) makes a sensor check. This check is a random roll between 0 to 36, but is modified by the source unit's tactics skill, as per the table below. (Skills 11-13 are for [RogueTech](http://roguetech.org) elite units).

| Skill                | 1    | 2    | 3    | 4    | 5    | 6    | 7    | 8    | 9    | 10   | 11   | 12   | 13   |
| -------------------- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- |
| Modifier             | +0   | +1   | +1   | +2   | +2   | +3   | +3   | +4   | +4   | +5   | +6   | +7   | +8   |

The result of this check determines the effective range of the unit that round:

  * A check of 0-12 is a __Failure__
  * A check of 13-19 is __Short Range__
  * A check of 20-26 is __Medium Range__
  * A check of 27+ is __Long Range__

On a failure, the unit can only detect targets it has visibility to (see below). On a success, the unit's sensors range for that round is set to a __base value__ defined by the spotter's unit type:

Type | Short | Medium | Long
-- | -- | -- | --
Mech | 300 | 450 | 550
Vehicle | 270 | 350 | 450
Turret | 240 | 480 | 720
Others | 150 | 250 | 350

This base value replaces the base sensor distance value from SimGameConstants for that model, but otherwise sensor detection ranges occur normally.

#### ECM Jamming Modifier

When the source unit begins it's activation within an ECM bubble, its sensors will be __Jammed__ by the enemy ECM. Units in this state will have a debuff icon in the HUD stating they are _Jammed_ but the source will not be identified. Units will also display a floating notification when they begin their phase or end their movement within an ECM bubble. _Jammed_ units reduce their sensor check result by the ECM modifier of the jamming unit.  This modifier is variable, but typically will be in the -12 to -24 range. Some common values are:

ECM Component | Modifier
-- | --
IS Guardian ECM | -15
Clan Guardian ECM | -?
Angel ECM | -?
TODO | TODO

A significant deviation from the _MaxTech_ rules is that multiple ECM bubbles DO NOT apply their full strength to jammed units. Instead, for each additional ECM jamming a source a flat -5 modifier is applied. This modifier is configurable in the settings section of the `LowVisibility/mod.json` file.

#### Active Probe Modifier

If a unit is _Jammed_ and has an __Active Probe__, it applies a positive modifier to the sensor check typically in the range of +1 to +8.

Active Probe Component | Modifier
-- | --
Beagle Active Probe | +?
Bloodhound Active Probe | +?

### Identification Level

In _Low Visibility_ details about enemy units are often hidden unless you have a good sensors check or are equipped with _Active Probes_. This mod defines five __ID Levels__ which reflect a progressively more detailed knowledge of a target:

ID Level | Description
-- | --
No ID | The target cannot be seen or selected
Silhouette ID | The target's type is shown (mech, vehicle, turret) and any chassis name will be visible (Atlas, Catapult). The unit's evasion pips will be shown. No other details are known.
Visual ID | The target's type is shown (mech, vehicle, turret), the chassis name, evasion pips, and weapon count and colors are shown. The target's armor and structure outline and bars are shown, but values are not displayed.
Sensor ID | The target's type is shown (mech, vehicle, turret), the chassis name, evasion pips, weapon names are shown. The target's armor and structure outline and bars are shown. Max values for armor and structure are shown, but current values are not.
Active Probe ID | All details about the target are shown, including component locations, buff, and debuff values.

_Silhouette ID_ and _VisualID_ require the source unit to have __visibility__ to the target. _VisualID_ only occurs when the source is within 90m of the target, or the map visibility limit, whichever is smaller.

_Sensor ID_ and _Active Probe ID_ require the source to have __detection__ to the target.


## Features

Stealth, NSS, Void System, Nova CEWS, WatchDog Suit.

- Visibility will tell you it's a Catapult, but not a CPLT-C1
- APs reveal all details about a model so long as the model is within their AOE
- Standard sensors give you a progression of answers (could re-use what I have for KYF)
- Visual inspection only within 3 hexes, can only show external details

- Visibility will by default only give you chassis name
- Visual identification (3 hexes) will only give you chassis name, weapons
- Standard sensors gives you variant name, weapons, armor/struct max values
- Active probes give you variant name, current armor/struct values, weapons, components
- What about heat/stab bars?
- What about statuses?

### Spotting vs Visibility
Loreum ipsum

### Sensors vs Signature
Loreum ipsum

### ECM Bubbles
Light Active Probe - 3 hexes
Beagle Active - 4 hexes
Active Probe - 5 hexes
Bloodhound - 8 hexes
    Beats ECM/Guardian

ECM/Guardian ECM (Gear_Guardian_ECM, Gear_Guardian_ECM_CLAN)
    - 6 hex bubble
    - defeats artemis IV/V, C3/C3i, Narc
- Angel ECM
    - as above, blocks BAP and streak


ECM Equipment = ecm_t0
Guardian ECM = ecm_t1
Angel ECM = ecm_t2
CEWS = ecm_t3

Active Probe = activeprobe_t1
Bloodhound Probe = activeprobe_t2
CEWS = activeprobe_t3

### Active Probes

Loreum ipsum

### Visibility Influenced by Environment

"mood_timeMorning",
"mood_timeNoon",
"mood_timeAfternoon",
"mood_timeDay",

"mood_timeSunrise",
"mood_timeSunset",
"mood_timeTwilight",

"mood_timeNight",

"mood_weatherClear",
"mood_weatherRain",
"mood_weatherSnow",
"mood_weatherWindy",
"mood_weatherCloudy",

		"mood_fogHeavy",
"mood_fogLight",
"mood_fogClear",

Daylight + No Conditions: 60 * 30 = 1800
+Fog,Rain,Smoke = 10 * 30 = 300

Twilight + No Conditions 15 * 30 = 450
+Fog,Rain,Smoke = 3 * 30 = 90

Darkness + No Conditions: 5 * 30 = 150
+Fog,Rain,Smoke = 1 * 30 = 300


Nova CEWS (Gear_Nova_CEWS)
    - Active Probe / 3 hexes
    - ECM / 3 hexes
    - No ECM can block CEWS except another CEWS

Watchdog System (Gear_Watchdog_EWS)
    - Standard Clan ECM Suite
    - Standard Clan Active Probe

Power Armor ECM (Gear_PA_ECM)
    - Same as Guardian ECM

ECM Equipment (Gear_ECM)
    - Active Probe, 3 hexes
    - ECM suite, 3 hexes

Unknown:
    - Gear EWS
    - Pirate ECM
    - Aftermarket EWS
    - Coventry Mark 85
    - Coventry Mark 95

StealthArmor - cannot be a secondary target, garners flat +1/+2 based on range

Void Signature System - Cannot be targeted by electronic probes
    0 movement = +3 penalty to hit
    1-2 hexes = +2 penalty to hit
    3-5 hexes = +1 penalty to hit
    6+ hexes = no penalty to hit

Chameleon Light Polarization Shield:
    medium range +1 to hit, long range +2 to this

Null Signature Shield
    (Same as Stealth armor, but description is odd)


MaxTech Rules:

Visual Range:  
	- 1800m for daylight
	- 450 for twilight
	- 300 for rain or smoke
	- 150 for darkness

Bloodhound Active Probe: 480 / 960 / 1440
Clan Active Probe: 450 / 900 / 1350
Beagle Active Probe: 360 / 720 / 1080
Mech Sensor range: 240 / 480 / 720
Vehicle Range: 180 / 360 / 540
