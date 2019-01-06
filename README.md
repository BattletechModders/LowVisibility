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

Visual line of sight in this mod is heavily influenced by the environment of the map. Each map contains one or more _mood_ tags that are mapped to visibility ranges. Instead of the TODO:FIXME value from SimGameConstants, every unit uses this visibility range when determining how far away it can spot a target. Flags related to the light level set a base visibility level, while flags related to obscurement provide a multiplier to the base visibility range.

Light Level | Base Visibility | Tags
-- | -- | --
bright light | 60 * 30m | `mood_timeMorning, mood_timeNoon, mood_timeAfternoon, mood_timeDay`
dim light | 16 * 30m | `mood_timeSunrise, mood_timeSunset, mood_timeTwilight`
darkness | 6 * 30m | `mood_timeNight`

Obscurement | Visibility Multiplier | Tags
-- | -- | --
Minor | x0.5 | `mood_fogLight, mood_weatherRain, mood_weatherSnow`
Major | x0.2 | `mood_fogHeavy`

A unit on a map with _dim light_ and _minor obscurement_ would have a map vision range limit of `16 * 30m = 480m * 0.5 = 240m`. Even if the unit's SpottingVisibilityMultiplier or SpottingVisibilityAbsolute modifiers increase it's base range beyond this value, the unit would be limited to visually detecting targets no further away that 240m.

### Detection

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

| ID Level        | Type      | Name                | Weapons                       | Components | Evasion Pips | Armor / Structure               | Heat           | Stability      | Buffs/Debuffs |
| --------------- | --------- | ------------------- | ----------------------------- | ---------- | ------------ | ------------------------------- | -------------- | -------------- | ------------- |
| Silhouette ID   | Full View | Chassis only        | Hidden                        | Hidden     | Hidden       | Percentage Only                 | Hidden         | Hidden         | Hidden        |
| Visual ID       | Full View | Chassis Only        | Type only                     | Hidden     | Shown        | Percentage Only                 | Shown          | Shown          | Hidden        |
| Sensor ID       | Blip      | Chassis and Model   | Types Always / Names Randomly | Hidden     | Shown ?      | Percentage and Max Value        | Randomly Shown | Randomly Shown | Hidden        |
| Active Probe ID | Blip      | Chassis and Variant | Shown                         | Shown      | Shown        | Percentage, Max, Current Values | Shown          | Shown          | Shown         |
| No ID           | Hidden    | Hidden              | Hidden                        | Hidden     | Hidden       | Hidden                          | Hidden         | Hidden         | Hidden        |

_Silhouette ID_ and _VisualID_ require the source unit to have __visibility__ to the target. _VisualID_ only occurs when the source is within 90m of the target, or the map visibility limit, whichever is smaller.

_Sensor ID_ and _Active Probe ID_ require the source to have __detection__ to the target.


## WIP Features

- [] BUG - Why can you see the blip beyond sensors range?
- [] BUG - Weapons summary shown when beyond sensors range
- [] Visibility for players is unit specific, unless models have ```share_sensor_lock``` tags
- [] Visibility for enemies is unit specific, unless models have ```share_sensor_lock``` tags
- [] Chance for VisualID to fail based upon a random roll
- [] _Possible Info_ elements are randomly selected each round / actor (simulate one question note)
- [] ```lv_shared_spotter``` tag on pilots to share LOS
- []```lv_shares_vision``` tag on components to share LOS
- [] Implement Stealth, NSS, Void System visibility reduction
- [] Implement Stealth, NSS, Void System evasion by movement semantics
- [] Implement Narc Effect - check status on target mech, if Effect.EffectData.tagData.tagList contains ```lv_narc_effect```, show the target even if it's outside sensors/vision range. Apply no penalty?
- [] Validate functionality works with saves - career, campaign, skirmish
- [] Make shared vision toggleable, if possible?
- [] Distinction between visual lock and sensor lock; if you have visual lock, you can see the target's silhouette. If you have sensor lock, your electronics can target them. You need both to have normal targeting modifiers.
- [] If you have visual + sensor lock, you share your vision with allies. If you have sensor lock, and have the ```lv_share_sensor_lock``` tag, you share your sensor lock with allies.
- [] Consider: Sensor info / penalty driven by range bands? You get more info at short range than long?



### ECM Bubbles

ECM Equipment = ecm_t0
Guardian ECM = ecm_t1
Angel ECM = ecm_t2
CEWS = ecm_t3

Active Probe = activeprobe_t1
Bloodhound Probe = activeprobe_t2
CEWS = activeprobe_t3



Notes: We don't follow the tech manual, we follow MaxTech. So Angel doesn't defeat bloodhound, it just makes it harder for those probes to find units. There is no 'completely blocking' - we're simulating that at a soft level with information hiding.


### Appendix

Information from various source books used in the creation of this mod is included here for reference purposes.

* MaxTech gives visual range as
	- 1800m for daylight
	- 450 for twilight
	- 300 for rain or smoke
	- 150 for darkness
* Converted to meters, MaxTech sensor ranges are:
  * Bloodhound Active Probe: 480 / 960 / 1440
  * Clan Active Probe: 450 / 900 / 1350
  * Beagle Active Probe: 360 / 720 / 1080
  * Mech Sensor range: 240 / 480 / 720
  * Vehicle Range: 180 / 360 / 540
* ECM Equipment:
  * ECM/Guardian ECM provides a 6 hex bubble
    * Defeats Artemis IV/V, C3/C3i, Narc locks through the bubble (Tactical Rules)
  * Angel ECM - as Guardian ECM, but blocks BAP and streak
  * Power Armor ECM (Gear_PA_ECM) - As Guardian ECM
* Active Probes:
  * Light Active Probe - 3 hexes detect range
  * Beagle Active - 4 hexes detect range
  * Active Probe - 5 hexes detect range
  * Bloodhound - 8 hexes detect range. Beats Guardian ECMs.
* Combo Equipment:
  * Prototype ECM (Raven):
    * Active Probe, 3 hexes
    * ECM suite, 3 hexes
  * Watchdog System (Gear_Watchdog_EWS)
    * Standard Clan ECM Suite
    * Standard Clan Active Probe
  * Nova CEWS (Gear_Nova_CEWS) - No ECM can block CEWS except another CEWS. AP beats all other ECMs
    * Active Probe / 3 hexes
    * ECM / 3 hexes
* StealthArmor - target cannot be a secondary target, adds flat +1/+2 based on range (medium/long)
* Void Signature System - Cannot be targeted by electronic probes
  * 0 movement = +3 penalty to hit
  * 1-2 hexes = +2 penalty to hit
  * 3-5 hexes = +1 penalty to hit
  * 6+ hexes = no penalty to hit
* Chameleon Light Polarization Shield:
  * medium range +1 to hit, long range +2 to hit
* Null Signature Shield
  - Needs details
