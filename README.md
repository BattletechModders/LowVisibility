# Low Visibility
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that introduces a new layer of depth into the detection mechanics of the game. These changes are influenced by the double-blind rules from MaxTech, but have been heavily adapted to the video game mechanics.

This mod has been designed to work with [RogueTech](http://roguetech.org). In theory is should work without that mod, but your mileage may vary.

__WARNING: THIS MOD LIKELY BREAKS SKIRMISH MULTIPLAYER.__ It has not been tested, but all due to the way it's been implemented my guess is that it won't work. You have been warned!

## Target Detection

What you can detect is defined by your unit's _visual lock_ or _sensor lock_. __Visual Lock__ occurs when your unit can visually identify a target, while __Sensor Lock__ occurs when your unit can identify a target using electronics. A unit can have one, both or no lock to a target, depending on various factors (described below).

### Visual Locks
Visual Locks offer the least information, providing little more than the target chassis and it's location. At long ranges all you will be able to say is that a target is an Atlas, Catapult, or Missile Carrier but you won't be able to tell which variant it is, or what it's carrying. At close range (typically within 5 hexes or so) the pilot can approximate some values such as armor and weapon types, but these are best guesses and can't always be trusted.

Information from visual locks is shared between all friendly units. The player and allied units share visual locks, while neutral and enemy units only share visual locks between their 'faction'.

### Sensor Locks
_Sensor Locks_ offer more information at a further distance. Experienced pilots and advanced equipment can use their sensors to identify fine details of a target. Some examples include exact armor values, component locations and possibly even the pilot name.

Sensor locks only share the location and outline of a target with their allies. Each unit has to rely upon their own sensors for the detailed breakdown of weapons, armor and equipment on the target. Specialized equipment can bypass this restriction and allow allied units to share detailed information as well as general location data.

#### Sensor Lock Checks
_Sensor Locks_ aren't reliable; they depend on the pilot's ability to interpret results amidst a changing electronic battlefield. At the start of each round, every unit makes a _sensor range_ and _sensor detail_ check.

 The __sensor range__ check influences how far out the unit can detect targets. A good roll increases the range, while a poor roll reduces it. The check result acts as a multiplier to the __total__ sensor range of the model, after any component multipliers or additions are included.

 The __sensor detail__ check influences what information you are presented on the target when you select them. If the roll is failed, you're unable to determine any specifics of the target and have to shoot blindly at them. Successes will reveal information such as their actual weapon loadout and armor status.

 The results of your current check are displayed in a tooltip in the status bar of each player mech. Check the icons in the bottom right corner, over the armor paperdoll, for a detailed breakdown.

### EW Equipment
__ECM__ components generate interference in a bubble around the unit, which makes the _sensor check_ of enemy units within that bubble more difficult. Units within the range of a friendly ECM are harder to detect as well. Powerful ECM can completely shutdown a unit's sensors, forcing them to rely upon visual lock for targeting purposes.

__Stealth__ components makes the equipped unit harder to detect. They require an ECM component to operate, but disable the ECM bubble effects.

__Active Probe__ components improve the quality of the units' sensors, and can break through ECM and Stealth if they are powerful enough.

__Narc Beacon__ weapons attach a powerful transmitter to targets. For a short duration, they will emit a signal that friendly units can use to identify the target's location __at any range__. This signal is opposed by friendly ECM, and may be disabled if enough ECM is present to overcome it's signal.

__TAG__ weapons identify the location and details of the target for all friendly units that receive the signal. This effect persists until the unit moves away from the position it was identified. Friendly ECM has no impact on this signal.

## Implementation Details
This section contains describes how to customize the mod's behavior. The values below impact various mechanics used through the mod to control visibility and detection.

While not necessary, it's suggested that you are familiar with the information in the [Low Visibility Design Doc](DesignDoc.md).

### Environmental Modifiers for Visual Lock

_Visual Lock_ is heavily influenced by the environment of the map. Each map contains one or more _mood_ tags that influence the vision range on that map. When each map is loaded, a base vision range is calculated for every unit from these tags. Flags related to the ambient light the a base vision range, while flags related to obscurement provide a multiplier that reduces this range.

Base Vision Range | Light |  Tags
-- | -- | --
15 hexes (450m) | bright | mood_timeMorning, mood_timeNoon, mood_timeAfternoon, mood_timeDay
11 hexes (330m) | dim | mood_timeSunrise, mood_timeSunset, mood_timeTwilight
7 hexes (210m) | dark | mood_timeNight

Vision Multiplier | Tags
-- | --
x0.7 | mood_weatherRain, mood_weatherSnow
x0.5 | mood_fogLight,
x0.3 | mood_fogHeavy

A map with _dim light_ and _rain_ has a vision range of `11 hexes * 30.0m * 0.7 = 231m`. Any _SpottingVisibilityMultiplier_ or _SpottingVisibilityAbsolute_ modifiers on the unit increase this base range as normal.

### Detection

At the start of every combat round, every unit (player or AI) makes two __sensor checks__. Each check is a random value between -14 to +14, assigned as per a normal distribution (aka a bell curve). The distribution uses mu=-2 and a sigma=4 value, resulting in a wide curve that's centered at the -2 result.

![Sensor Check Distribution](check_distribution.png "Sensor Check Distribution")

Each check is further modified by the source unit's tactics skill, as per the table below. (Skills 11-13 are for [RogueTech](http://roguetech.org) elite units).

Skill |  1  |  2  |  3  |  4  |  5  |  6  |  7  |  8  |  9  |  10  | 11 | 12 | 13
-- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | --
Modifier                  | +0 | +1 | +1 | +2 | +2 | +3 | +3 | +4 | +4 | +5 | +6 | +7 | +8
+ Lvl 5 Ability | +0 | +1 | +1 | +2 | +3 | +4 | +4 | +5 | +5 | +6 | +7 | +8 | +9
+ Level 8 Ability | +0 | +1 | +1 | +2 | +4 | +5 | +5 | +6 | +6 | +7 | +8 | +9 | +10

#### Detection Range

The first check (the __range check__) is used to determine the unit's sensor range this turn. Each unit has a base sensor range determined by its type, as given in the table below. This range is increased by _SensorDistanceMultiplier_ and _SensorDistanceAbsolute_ values normally, allowing variation in unit sensor ranges.

| Type | Base Sensor Range |
| -- | -- |
| Mech | 12 hexes * 30m = __360m__ |
| Vehicle | 9 hexes * 30m = __270m__ |
| Turret | 15 hexes * 30m = __450m__ |
| Others | 6 hexes * 30m = __180m__ |

The range check result is divided by ten, then used as a multiplier against the unit's sensor range. A range check result of +3 yields a  sensor range multiplier of (1 + 3/10) = 1.3x. A negative range check of -2 would result in a multiplier of (1.0 - 2/10) = 0.8x.

##### First Turn Protection

On the very first turn of every combat, every unit (friendly, neutral, or foe) always fail their __range check__. This ensures players can move away from their deployment zone before the AI has a chance to attack them. This behavior can be disabled by setting `FirstTurnForceFailedChecks` to __false__ in `mod.json`.

#### Detection Info

The second check (the __info check__) determines how much target information the unit will receive this round. This check is applicable for optimal conditions - enemy ECM and other effects can reduce this value on a target by target basis. The range of check results is given below:

| Info Check | Detail Level | Details shown |
| --| -- | -- |
| < 0 | No Info | Failed sensor check, no information shown |
| 0 | Location | Target location (3d arrow), but unknown name |
| 1 | Type | As above, but type defined (mech/vehicle/turret) |
| 2 | Silhouette | As above, with Chassis as name (Atlas, Catapult) |
| 3 | Vector | As above, adding Evasion Pips |
| 4 or 5 | Surface Scan | As above, adding Armor & Structure percentages, paperdoll |
| 6 or 7 | Surface Analysis   | As above, adding Weapon Types (as colored ???) |
| 8 | Weapon Analysis | As above, with Weapon Names defined. Name is Chassis + Model (Atlas AS7-D, CPLT-C1) |
| 9 | Structure Analysis | As above, plus current heat & stability, summary info (tonnage, jump jets, etc). Armor & structure includes current and max values. Name is Chassis + Variant name (Atlas ASS-HAT Foo, Catapult CPLT-C1 Bar) |
| 10 | Deep Scan | As above plus component location, buffs and debuffs |
| 11 | Dental Records | As above plus pilot name, info |

### ECM Details

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

_Silhouette ID_ and _VisualID_ require the source unit to have __visibility__ to the target. _VisualID_ only occurs when the source is within 90m of the target, or the map visibility limit, whichever is smaller.

## Jamming Details
TODO: Clean this up

* ```lv-jammer_mX_rY``` creates an ECM bubble in a circle of Y hexes (\*30 meters in game) around the source unit. The Jammer imposes a penalty of X to any sensor checks by jammed units.
* ```lv-probe_mX``` is an active probe. It adds a bonus of X to sensor checks made by this unit.

Probes of an equal tier penetrate jammers, to a T1 probe will penetrate a T1 jammer. This means the jammer won't add it's penalty to the source unit.

The MaxTech rulebook provides guidelines for how much of an impact ECM has on sensors. Because LowVisibility increases the dice 'roll' by a factor of 3, these modifiers are correspondingly increased. The table below lists the recommended modifier values for these scenarios:

| Sensor | MaxTech (A./G.) | Angel ECM Mod | Guardian ECM Mod |
| -- | -- | -- | -- |
| Vehicle Sensor | 7 / 6 | -21 | -16 |
| Mech Sensor | 6 / 5 | -18 | -15 |
| Beagle | 5 / 4 | -15 | -12 |
| Bloodhound | 4 / 3 | -12 | -9 |
| Clan Active Probe | 3 / 2 | -9 | -6 |

Assuming the ECM values above are used, and _Mech Sensors_ form the baseline at -18/-15 modifiers, recommended values for the active probe modifiers are:

* Beagle: +3
* Bloodhound: +6
* Clan Active Probe: +9

Probe ranges are given as additional ranges from MaxTech, while ECM ranges come from the Master Rules. Those values are:

* Guardian ECM: 6 hexes
* Angel ECM: 6 hexes
* CEWS (ECM): 3 hexes
* BattleArmor ECM: 3 hexes
* Light Active Probe: +3 hexes
* Beagle: +4 hexes
* Bloodhound: +8 hexes
* Clan Active Probe: +7 hexes
* CEWS: +8 hexes

Pull this all together, recommended tags for this mod are:

| Component | Tag |
| -- | -- |
| Guardian ECM | lv-jammer_t2_r6_m15 |
| Angel ECM | lv-jammer_t3_r6_m18 |
| CEWS | lv-jammer-t4_r3_m21 |
| Beagle Active Probe | lv-probe-t1_r4_m3 |
| Clan Active Probe | lv-probe-t1_r7_m9 |
| Bloodhound Active Probe | lv-probe-t2_r8_m6 |
| CEWS | lv-probe-t3_r3_m7 |

## Stealth

Stealth systems reduce the chance of the unit being targeted with a visual or sensor lock.

Component | Effect
-- | --
 __Chameleon Light Polarization Shield__ | TODO
__Stealth Armor__ | TODO
__Null-Signature System__ | TODO
__Void-Signature System__ | TODO

```lv-stealth_mX``` - applies ECM to the target unit instead of generating a bubble. Won't jam enemies around the unit, but automatically defeats the sensor suit of equal or lower tier X.

```lv-stealth-range-mod_sA_mB_lC_eD``` - applies A as a modifier to any attack against the target at short range, B at medium range, C at long range, and D at extreme range.

```lv-stealth-move-mod_mX_sZ``` - applies X as a modifier, and reduces it by -1 for each Y hexes the unit moves. m3_s2 would be a +3 modifier if the unit doesn't move, +2 if it moves 1-2 hexes, +1 for 3-4 hexes, +0 otherwise.

## Narc Beacons

__WIP__
RFC - thoughts on Narc beacon; I'll give it a tag like
lv_narc_mX
. The modifier is compared against all friendly to the narc'd unit ECM . If the narc modifier is > friendly_ecm, it makes the unit visible to enemies no matter the range.

The lack of range is to simplify the coding (complexity and checks). Once you're narc'd it's unlikely you'd get so far away that you'd be out of sensor range anyways, and it can guard against really bad sensor range checks.

Narcs would only impact visibility, NOT sensor details. So you can see/target them, but details would still be hidden.

## TAG

__WIP__
For TAG, I'm thinking
lv_tag_mX
, where X is a pretty big number (8-9). For each hex the target moves, that modifier gets reduced by -1. When a details check is made against the target, the current TAG bonus is added to the enemy check.

I'm thinking the value of TAG is that it wouldn't be  impacted by ECM. That's not 'realistic' but it feels like a useful trait mechanically.

## Worklog

### WIP Features

- [] Buildings should always be visible and not subject to ECM - breaks AI without this!
  - __Preliminary testing seems to indicate this may be fixed__
  - Likely an issue that I'm dealing with AbstractActors everywhere, but can be an ICombatant
- [] Allied units sometimes showing as blips instead of always full vision.
- [] Saves occur on post-mission/pre-mission saves; should skip
- [] Eliminate visual scan - vision lock is a limited amount of info
- [] Add tactics bonus for L5 & L8, just like SBI.
- [] Evasion pips display in T.HUD but not on the model
- [] C3 slave should require a C3 master to share sensors. CEWS Nova should share with units that have CEWS Nova. (ask MXMach/LA for details)
- [] FrostRaptor: @LadyAlekto so in a lance where c3m/c3s present... each adds +X to each other's detail / range checks?
  [4:17 PM] FrostRaptor: Same for c3i - each present in lance gives +X to lance members with it?
  [4:17 PM] LadyAlekto: yeah
- [] scrambler_m0 tag; scrambles sensor checks at any range. Allows LA to build the 'IFF jammer' she wants. Sorta like stealth, but w/o the ECM requirement.
- [] Sensor range circle not updating onActorSelected; gives you a false sense of where you can see
- [] If you have sensor lock from a position, but not LOS, doesn't display the lines showing that you can shoot from there. How to fix?
- [] On shutdown, no stealth / ecm / etc applies
- [] If possible, make SensorLock boost sensorrange by 2x for the remainder of the round.
- [] Validate functionality works with saves - career, campaign, skirmish
- [] Hide pilot details when not DentalRecords
- [] BUG - Debuff icons don't update when the sensor lock check is made, they only update after movement. Force an update somehow?
- [] BUG - Tactics skill should influence chassis name, blip type (CombatNameHelper, LineOfSightPatches)
- [] BUG - Weapons summary shown when beyond sensors range
- [] BUG - Status buffs shown on low-end checks. Should be on 8+.
- [] BUG - Units disappear from view in some cases. Doesn't appear related to the previous behavior, but is related.
- [] Component damage should eliminate ECM, AP, Stealth bonuses
- [] ```lv_shared_spotter``` tag on pilots to share LOS
- [] Implement ```lv-mimetic_m``` which represents reduces visibility if you don't move
- [] Implement Narc Effect - check status on target mech, if Effect.EffectData.tagData.tagList contains ```lv_effect_narc_rY_dZ```, narc Continues to emit for durationZ, Y is radius within which anybody can benefit from the Narc boost.
- [] Implement Tag effects; ```lv-effect-tag_m?```. Tag differs from narc in that it's only during LOS? Others wants it tied to TAG effects and be for 1-2 activations.
- [] Implement rings for vision lock range, ECM range, similar to what you have with sensor range (blue/white ring around unit)
- [] Implement stealth multi-target prohibition
- [] Reimplement sensor shadows?
- [] No Lock penalties are multipliers for range penalties; 0.5 for visual, 1.0 for sensor. So at short range you get a -1 for sensors, -2 at medium, etc. Reflects that it's harder to shoot someone without a lock the further out you get.
- [] Add a ```lv-max-info``` tag that caps the level of info that can be returned for a given unit. This would support certain units like infantry that have been asked for.
- [] Add a ```lv-sensor-roll-mod_m``` tag that provides a modifier to the sensor check (positive or negative)
- [] Modify called shot critical, % when making a shot w/o lock_
- [] injuries reduce sensor check

### Possible Additions

- [] Sensors should have a range within which they work; otherwise they are just a bonus to the roll. Making them have a specific range, and limiting vision details to probes, might be a way to go?
- [] Add a ```lv-jammer-boost_mX``` and ```lv-probe-boost_mX``` that provide a flat +M to any attached jammer, probe. This allows them to build the 'IFF Jammer' they talked about.
- [] Sensor info / penalty driven by range bands? You get more info at short range than long?
- [] Hide/obfuscate some ranged attack tooltip information at low sensor levels (evasion, stealth), etc? If we do this, some folks won't understand why something is -2 or -3 until they get a better reading.
- [] Add ability for a pilot to get a bad reading / critical failure. Tie to tactics as a roll, so poor pilots have it happen more often.  In failure, show wrong name/tonnage/information to confuse the player. May need some hidden marker indicating that this is a false lead - possibly a temporarily value that can be removed once we introduce the mod.
- [] Should stealth have a visibility modifier that changes as you move move? I.e. 0.1 visibility if you don't move, 0.5 if you do, etc. (Think Chameleon shield - should make it harder to see the less you move)

- [] Experiment with AllowRearArcSpotting:false in CombatGameConstants.json

- [] SensorLock becomes passive ability that doubles your tactics, boosts sensor detect level by +1 (for 'what you know'). Eliminate it as a menu item, ensure forceVisRebuild never gets called.

- [] Add a 'lv-dentist' tag that always provides the highest rating of info

- [] Add a 'lv-ninja' tag that always hides all the information of a unit.

- [] Add a 'lv-vision-lowlight_rX' for low-light vision; increases visual range during a non-daylight mood

- [] Add a 'lv-sensor-heat_rX_hY' for heat vision; increases detection of units with high heat ratings. For every Y heat, add +1 to the sensor check for this unit.

### To Document
- [x] First turn auto-fail; everybody fails their check on the first turn. Show 'powering up' sensors or something like that?
- [x] Add a minimum for sensor range, visual range. You can't go below that. Maybe 6/3?
- [x] BUG - When you overheat a mech, it disappears from vision
- [x] Add multiple ECM penalty to sensor check
- [x] VisionLock and VisualID ranges should be modified by equipment.
