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

##### Visual Identification

TODO: FIXME

_Silhouette ID_ and _VisualID_ require the source unit to have __visibility__ to the target. _VisualID_ only occurs when the source is within 90m of the target, or the map visibility limit, whichever is smaller.

### ECM Details

ECM components emit a bubble around the unit. After every movement occurs, all units are checked to see if they are within the ECM bubble of another unit.

If they are within an enemy ECM bubble, they gain an __ECM jamming__ modifier equal to the strength of the emitter. This reduces __both__ _Detection_ checks by the emitter strength.

If the unit is within a friendly ECM bubble it gains __ECM protection__. This adds the friendly emitter's strength as a negative modifier to any _Detection Info_ checks made against the target.

If there are multiple ECM emitters covering a target, the strongest modifier will be applied. Each additional emitter will add +1 strength to the strongest emitter's modifier. This value can be tweaked by changing the ``MultipleJammerPenalty`` in ``mod.json``.

ECM components must have the tag ```lv-jammer_mX_rY``` to be recognized as an ECM emitter. The X value is the modifier the emitter adds as protection or jamming to the target. The Y value is the size of the ECM bubble generated, in hexes. A tag of __lv-jammer_m4_r8__ would apply to any targets within 8 hexes, apply a modifier of -4 to jammed enemies, and add 4 points of protection to friendly units.

If an enemy unit within an ECM bubble is attempting to detect a friendly unit protected by the bubble, __both modifiers apply__. If there are two overlapping bubbles of __lv-jammer_m4_r8__ emitters, the enemy would have a total `-4 -1 = -5` penalty from being __jammed__, and a further `-4 -1 = -5` modifier due to the target having __protection__. Their checks would have a __-10__ modifier to detect the unit protected by both bubbles.

#### Active Probe Details

`lv-probe_mX` is an active probe. It adds a bonus of X to sensor checks made by this unit.


#### Scrambler Details

**WIP**

`lv-scrambler_mX` is an active type of stealth. It reduces the target's ability to lock onto the target;

Should this increase visibility?

#### Stealth Details

Stealth systems reduce the chance of the unit being detected with sensors. This makes them harder to find in the first place, but also makes them harder to attack as well. Stealth systems can have one or more of the following effects.

Components with the `lv-stealth_mX` tag reduce the _Detection Info_ check for a unit attempting to detect them.

Components with the `lv-stealth-range-mod_sA_mB_lC_eD` tag are more difficult to attack with ranged weapons. A is applied as a penalty to attacks against the target at short target. B is the penalty applied at medium range, C at long range, and D at extreme range.

Components with the `lv-stealth-move-mod_mX_sZ` tag are more difficult to attack if the unit is stationary. The value of X is a base penalty that applies to any attack against the target. This penalty is reduced by 1 for each Z hexes the target moves, until it the penalty is completely eliminated. A tag of __lv-stealth-move-mod_m3_s2__ would apply a +3 penalty if the unit did not move. If the unit moves 1 or 2 hexes, this penalty would be reduced to +2. If the unit moves 3-4 hexes, the penalty is reduced to +1, and if the unit moves 5 hexes or more the penalty is completely removed.

#### Narc Beacon Details

__WIP__

`lv-narc-effect_m`

. The modifier is compared against all friendly to the narc'd unit ECM . If the narc modifier is > friendly_ecm, it makes the unit visible to enemies no matter the range.

The lack of range is to simplify the coding (complexity and checks). Once you're narc'd it's unlikely you'd get so far away that you'd be out of sensor range anyways, and it can guard against really bad sensor range checks.

    "statusEffects": [{
        ...
        "tagData" : { 
    		"tagList" : [ "lv-narc-effect_m8" ] 
    	},
    }]
    
#### TAG Details

__WIP__
For TAG, I'm thinking
`lv-tag-effect`
, where X is a pretty big number (8-9). For each hex the target moves, that modifier gets reduced by -1. When a details check is made against the target, the current TAG bonus is added to the enemy check.

I'm thinking the value of TAG is that it wouldn't be  impacted by ECM. That's not 'realistic' but it feels like a useful trait mechanically.

```
"statusEffects": [{
"durationData":{
    "duration" : 10,
    "ticksOnActivations" : false,
    "useActivationsOfTarget" : true,
    "ticksOnEndOfRound" : false,
    "ticksOnMovements" : true,
    "stackLimit" : 1,
    "clearedWhenAttacked" : false
	},
"targetingData" : {
    "effectTriggerType" : "OnHit",
    "triggerLimit" : 0,
    "extendDurationOnTrigger" : 0,
    "specialRules" : "NotSet",
    "effectTargetType" : "NotSet",
    "range" : 0,
    "forcePathRebuild" : false,
    "forceVisRebuild" : false,
    "showInTargetPreview" : false,
    "showInStatusPanel" : true
    },
"effectType" : "TagEffect",
"Description" : {
    "Id" : "TAG-Effect-Vision",
    "Name" : "Target Acquired",
    "Details" : "This target was TAG'ed. It will be much easier to see until it moves.",
    "Icon" : "uixSvgIcon_artillery"
},
"nature" : "Debuff",
"statisticData" : null,
"tagData" : { 
	"tagList" : [ "lv-tag-effect" ] 
	},
"floatieData" : null,
"actorBurningData" : null,
"vfxData" : null,
"instantModData" : null,
"poorlyMaintainedEffectData" : null
}]
```





## Worklog

### WIP Features

- [] Buildings should always be visible and not subject to ECM - breaks AI without this!__
  - Likely an issue that I'm dealing with AbstractActors everywhere, but can be an ICombatant
- [] Saves occur on post-mission/pre-mission saves; should skip
- [] Fix issues with VisualID - make it apply if close enough
- [] Evasion pips display in T.HUD but not on the model
- [] Add `lv-vision-zoom_m` and `lv-vision-heat_m` modifiers; reduces direct fire penalty, but at different ranges
- [] Make sensor lock not end your turn (SensorLockSequence)
- [] C3 slave should require a C3 master to share sensors. CEWS Nova should share with units that have CEWS Nova. (ask MXMach/LA for details)
- [] FrostRaptor: @LadyAlekto so in a lance where c3m/c3s present... each adds +X to each other's detail / range checks?
  [4:17 PM] FrostRaptor: Same for c3i - each present in lance gives +X to lance members with it?
  [4:17 PM] LadyAlekto: yeah
- [] Sensor range circle not updating onActorSelected; gives you a false sense of where you can see
- [] If you have sensor lock from a position, but not LOS, doesn't display the lines showing that you can shoot from there. How to fix?
- [] On shutdown, no stealth / ecm / etc applies
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
