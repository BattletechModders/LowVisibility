# Low Visibility
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that introduces a new layer of depth into the detection mechanics of the game. These changes are influenced by the double-blind rules from MaxTech, but have been heavily adapted to the video game mechanics.

This mod has been designed to work with [RogueTech](http://roguetech.org). In theory is should work without that mod, but your mileage may vary.

__WARNING: THIS MOD LIKELY BREAKS SKIRMISH MULTIPLAYER.__ It has not been tested, but all due to the way it's been implemented my guess is that it won't work. You have been warned!

*REQUIRES* [IRBTModUtils](https://github.com/IceRaptor/IRBTModUtils) to compile.

CAE is highly recommended. Many effects assume they aren't active at the same time (stealth/ecm, vision modes, etc.) You can enforce yourself through convention, or through CAE.

## Summary

This mod is comprehensive, but a short summary of changes in the mod include:

* Sensor blips can now be targeted and attacked, allowing long range builds to be more viable.
* ECM bubbles provide protection for allies and reduce enemy sensors.
* Enemy details are hidden and are only revealed to strong sensors and/or pilots with high Tactics.
* The environment can reduce visibility due to snow, rain, night, etc. This makes sensors critical to success on those maps.
* Stealth can hide enemy mechs (and your own!) allowing you to close range safely.
* Memetic armor reduces your ability to be targeted, but decreases if you move

## Target States

Every model in the game is in one of the three states. A target is either:

* **Visible** - your team can directly observing the target.
* **Detected** - your team tracks the target using sensors.
* **Unknown** - your team does not know the position of the target at all.

All allied units contribute towards determining if a model is visible or detected. If one unit can see a target, it passes that information to all other units on that team. However, each unit **individually** calculates whether it has a *Line of Sight* to the target, and what level of *Identification* it has for the target.

A unit has a *line of sight* to a target if it can directly observe the target. Line of sight can be obstructed by terrain such as hills, or reduced by weather conditions such as rain or fog. A line of sight can be *obstructed*, which means it partially passes through a terrain feature.

Even if a target is visible or detected, you may know its location but all other details can be hidden. **Identification** of the target's details comes from either visual inspection or sensor analysis:

* **Visual ID** - if you are close enough, your pilots can determine some basic information such as the chassis type, rough armor/structure values, and type of weapon. You have to be within 150 meters for visual identification.
* **Sensor ID** - with a good electronic warfare check your pilots can determine all of a target's details, including current armor values, weapon names, and component locations.

### Pilot Skill

Even the most advanced equipment depends upon the pilot... `TODO: CONTINUE ME`

### Equipment

This mod adds several types of equipment that generate electronic warfare effects. A short summary of their effects are:

* __ECM__ components generate a scrambling bubble that protects the carrier and friendly units within its area. This makes provides an attack penalty to attacks against friendly units, increases the difficulty of sensor identification against friendly units, and applies a penalty to sensor identification for any enemy unit within the area of effect.
* __Stealth__ components makes the equipped unit harder to detect by absorbing sensor emissions. They make the carrier harder to detect, increase the difficulty of sensor identification, and applies an attack penalty to any attacker.
* __Mimetic__ components make the unit harder to visually identify through a chameleonic effect. The carrier will be less visible and attacks will suffer a penalty. These effects are lost if the carrier moves during their turn.
* __Probe__ components are advanced sensors that make it easier to detect enemy units. They provide a bonus to sensor identification and reduce ECM, Stealth, and Mimetic effects on targets.
* __Narc__ weapons fire a transmitter the broadcasts data about the target across the entire map. This effect ignores Stealth and Mimetic effects on the target.
* __TAG__ weapons fire a specialized beam that locates and identifies a target. This effect is lost when the target moves. This effect ignores ECM or Stealth on the target.

## Details

This section describes the various mechanics that used throughout *LowVisibility*. Players may find this information overwhelming, as its written as a reference for mod authors.

While not necessary, it's suggested that you are familiar with the information in the [Low Visibility Design Doc](DesignDoc.md).

Any variable name in `code syntax` is a configuration value. It can be changed by modifying the named value in **LowVisibility/mod.json**.

### Electronic Warfare Checks
At the start of each round, every unit makes an **electronic warfare check**. A good check result represents the pilot making the best use of their equipment, while a poor one reflects them being preoccupied with other things.

Each check is a random value between -14 to +14, assigned from a normal distribution (aka a bell curve). The distribution uses mu=-2 and a sigma=4 value, resulting in a wide curve that's centered at the -2 result.

![Sensor Check Distribution](check_distribution.png "Sensor Check Distribution")

Each check is further modified by the source unit's tactics skill, as per the table below. (Skills 11-13 are for [RogueTech](http://roguetech.org) elite units).

| Skill |  1  |  2  |  3  |  4  |  5  |  6  |  7  |  8  |  9  |  10  | 11 | 12 | 13 |
| -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| Modifier | +0 | +1 | +1 | +2 | +2 | +3 | +3 | +4 | +4 | +5 | +6 | +7 | +8 |
| w/ Lvl 5 Ability | +0 | +1 | +1 | +2 | +3 | +4 | +4 | +5 | +5 | +6 | +7 | +8 | +9 |
| w/ Lvl 8 Ability | +0 | +1 | +1 | +2 | +4 | +5 | +5 | +6 | +6 | +7 | +8 | +9 | +10 |

The current check result is displayed in a tooltip in the status bar of each player mech. The check result contributes to various calculations in the mod, including:

* As a bonus to visual identification
* As a bonus to sensor identification

### Spotting and Sensors

Every unit in the game has four related values that control whether it can be seen and targeted:

* **Spotting Range** determines how far (in meters) the unit can visually locate a target. You can only draw a *line of sight* to a target closer than your spotting range.
* **Visibility** is a multiplier from the target that modifies the source's *spotting range*.
* **Sensor Range** determines how far (in meters) the unit can locate targets using sensors.
* **Signature** is a multiplier from the target that modifies the source's *sensor range*.

For both cases the math is straightforward. If a source has a 500 meter range, and the target has visibility/signature of 0.5, the target can be detected at 250 meters or closer. If the target's visibility/signature is 1.5, it can be detected at 750 meters or closer.

Terrain can modify visibility and signature values. Forests apply a 0.8  modifier to signature, while water applies a 1.2.

### Spotting

In *LowVisibility* unit's *base spotting range* is shorter than vanilla , as sensor locks allow targets to be identified and attacked. It can also be influenced by weather effects (see below). No matter the circumstances, a unit's spotting range cannot drop below `VisionRangeMinimum`. This value is expressed as 30 meter hexes, and defaults to 2 hexes (60 meters).

A MechWarrior can make some guesses about the target when they are very close. Units at `VisualIDRange` or will identify basic details about the target even if they have no sensor lock. Details will be limited to approximate armor and structure amounts, general weapon types, and the like. The default value is 5 hexes (180 meters).

TODO: Add EW check to effect to allow stronger pilots to know more

#### Environmental Effects

Each map contains one or more _mood_ tags, some of which apply a limit to all units spotting distance. Conceptually tags related to the ambient light level set the base vision range, while tags related to obscurement provide a multiplier that reduces this base range.

Any modifiers to a units _SpottingVisibilityMultiplier_ or _SpottingVisibilityAbsolute_ statistic increase the calculated base range.

Base Vision Range | Light |  Tags
-- | -- | --
15 hexes (450m) | bright | mood_timeMorning, mood_timeNoon, mood_timeAfternoon, mood_timeDay
11 hexes (330m) | dim | mood_timeSunrise, mood_timeSunset, mood_timeTwilight
7 hexes (210m) | dark | mood_timeNight

Vision Multiplier | Tags
-- | --
x0.7 | mood_weatherRain, mood_weatherSnow
x0.5 | mood_fogLight
x0.3 | mood_fogHeavy

> A map with _dim light_ and _rain_ has a vision range of `11 hexes * 30.0m * 0.7 = 231m`.

#### Sensor Range

The first check (the __range check__) is used to determine the unit's sensor range this turn. Each unit has a base sensor range determined by its type, as given in the table below. This range is increased by _SensorDistanceMultiplier_ and _SensorDistanceAbsolute_ values normally, allowing variation in unit sensor ranges.

| Type | Base Sensor Range |
| -- | -- |
| Mech | 12 hexes * 30m = __360m__ |
| Vehicle | 9 hexes * 30m = __270m__ |
| Turret | 15 hexes * 30m = __450m__ |
| Others | 6 hexes * 30m = __180m__ |

The range check result is divided by ten, then used as a multiplier against the unit's sensor range. A range check result of +3 yields a sensor range multiplier of (1 + 3/10) = 1.3x. A negative range check of -2 would result in a multiplier of (1.0 - 2/10) = 0.8x.

No matter the circumstances, sensors range cannot drop below a number of hexes equal to _SensorRangeMinimum_. This value defaults to 6 hexes (240 meters).

##### Target Signature

A unit's sensors define the range at which they can see a standard target in normal conditions. Equipment and environment can result in a target that's easier or harder to see, which is expressed through their __signature__ rating. This rating is typically hidden, though components in the Mechlab will indicate any increases or reductions in signature.

Target signatures act as a multiplier to the sensor range. If a unit has a sensor range check that resulted in sensors range 400:

  * A hard to detect target with signature 0.5 would only be detectable at 200m or closer.
  * A standard target with signature 1.0 would be detectable at 400m or closer.
  * An easy to detect target with signature 1.2 would be detectable at 480m.



#### Sensor Info

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

### First Turn Protection

On the very first turn of every combat, every unit (friendly, neutral, or foe) always fail their __range check__. This ensures players can move away from their deployment zone before the AI has a chance to attack them. This behavior can be disabled by setting `FirstTurnForceFailedChecks` to __false__ in `mod.json`.

### ECM

ECM components generate an aura around the carrier unit. Any friendly unit within the bubble receives an **ECM Shield** effect. Enemy units within the bubble receive an **ECM Jamming** effect. If there are multiple ECM sources a target uses the strongest modifier, +1 for each additional emitter. You can change the modifier for multiplier emitters can be modified by changing `MultipleJammerPenalty`.

For each point of **ECM Shield** on a target, attackers gain a +1 attack penalty and -1 sensors identification check. The target gains a 0.05 increase to their signature, making it easier for them to be located.

For each point of **ECM Jamming** on a target, the target suffers a -1 penalty to any sensors identification check they make.

If a source with **ECM Jamming** is attempting to identify a target protected by **ECM Shield**, both modifiers apply.

To enable ECM on a component, define the following effects on the componentDef:

```

```

### Active Probes

Some components apply a modifier to the unit's _Sensors Range Check_ and _Sensors Info Check_. These generally add a bonus that increases sensors range, and improves resolution of target details.

* Components with a `lv-probe_mX` apply the highest value of X as a modifier to the unit's sensor checks. Only the best value is applied, and negative values are ignored.
* Components with a `lv-probe-boost_mX` tag sum their X values, and apply the result as a modifier to sensor checks. Negative values for X are allowed.

The modifiers from the `lv-probe_mX` and `lv-probe-boost_mX` tags are additive. If a unit has both, their sensors are improved by the sum of each modifier.

> Example: A unit has components with tags lv-probe_m2, lv-probe_m3, lv-probe-boost_m2, lv-probe-boost_m. The sensor check modifier is 3 (from lv-probe_m3, which causes lv-probe_m2 to be ignored), + 2 (lv-probe-boost_m2) + 1 (lv-probe-boost_m1) = 6. This unit would add +6 to any sensor range and info checks it makes.

To build __Active Probes__, you should use the `lv-probe_mX` modifier to ensure only the strongest probe is used. `lv-probe-boost_mX` can be used to build equipment that reduces sensors, reflecting poor equipment.

These modifiers apply to both range and info. If you only want longer ranged sensors, use the `SensorRangeMultiplier` and `SensorDistanceAbsolute` on the component instead.

### Stealth

Stealth components reduce an opponent's ability to sensor lock the protected unit. Stealth only applied to the unit directly, and has no effect when the enemy attempts to detect other friendly units. Stealth generally applies a penalty to the opponent's check, which reduces the _Sensors Info Check_. Stealth does not impact an opponent's _Sensors Range Check_.

* Components with a `lv-stealth_mX` apply the highest value of X to the unit's stealth modifier. Only the best value is applied, and negative values are ignored.
* Components with a `lv-scrambler_mX` tag sum their X values, and apply the result to the unit's stealth modifier. Negative values for X are allowed.

The modifiers from the `lv-stealth_mX` and `lv-scrambler_mX` tags are additive. If a unit has both, their sensors are improved by the sum of each modifier.

> Example: A unit has components with tags lv-stealth_m2, lv-scrambler_m-2, lv-scrambler_m-3. The stealth modifier is 2 (from lv-stealth_m2), - 2 (lv-scrambler_m-2) - 3 (lv-scrambler_m-3) = -3. This unit would add +3 to any sensor range and info checks against it, because it's stealth modifier is negative.

__Design Note:__  Stealth closely approximates the sensor and signature spectrum HBS already has in the game. _LowVisibility's_ stealth was created to be less binary than signature reductions. Signature modifiers also hide a target, by reducing the range at which the target can be detected. Sensor modifiers can increase the range, allowing a push and pull between them that mimics TT stealth. However, Stealth reduces the info level (not the range), which allows high-sensor builds to still have a chance to detect them for targeting purposes, without knowing their details.

## Effects

The sections below discuss electronic warfare effects allowed by  _LowVisibility_.

### Narc Effect

Narc beacons launch a small transmitter that attaches to the target and broadcasts their location. _LowVisibility_ provides an effect tag that mimics this effect by providing a strong bonus to sensor detect checks for targets that have been narc'd.

Any effect that attaches the `lv-narc-effect_mX` tag will apply X as a modifier to sensor checks against the unit under the effect. If this modifier is greater than the affected unit's ECM protection the unit will be marked as a sensor blip regardless of sensor range. Units within sensor range will apply the difference as a modifier to any _sensor info checks_.

> Example: A unit has a `lv-narc-effect_m6` applied to it from a Narc beacon. The unit has ECM protection of 4, so the Narc's effect becomes 6 -4 = 2. The unit's will be visible as a blip at any range, and ay sensor info checks against the unit gain a +2.

An example of attaching this tag to an effect is below:

```
"statusEffects": [{
    ...
    "tagData" : {
    "tagList" : [ "lv-narc-effect_m8" ]
  },
}]
```

### TAG Effect

TAG emitters are special sensors that provide deep information on a target so long as they can be targeted with a laser-like beam. _LowVisibility_ mimics this effect by providing a _sensor info modifier_ that decays as the target moves. TAG effects are NOT impacted by ECM protection, which provides a way for players to fight against opponents with strong ECM.

Any effect that incorporates the `lv-tag-effect` provides a _sensor check_ equal to the duration of the effect. Any friendly unit applies this bonus to their _sensor info check_. This effect is intended to be placed on a status effect with durationData that uses `ticksOnMovements: true`. `ticksOnMovements` causes the effect duration to be reduced by one for each hex the unit moves, which provides the decay effect we expect.

> Example: A unit has an effect with the `lv-tag-effect` applied to it, with an effect duration of 10. Sensor info checks against the target would gain a bonus of +10. If the target moves 4 hexes, the duration would reduce to 6, which also reduces the sensor info check bonus to +6. Once the target moved another 6 hexes the bonus would completely decay.

An example effect that uses this tag is below:

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
    "Name" : "TAG'd - Visibility",
    "Details" : "This will be much easier to sensor lock until it moves.",
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

## Attack Modifiers
Once a target has been detected it can be attacked normally, though attacks may suffer penalties based upon how strong of a lock they have to the target.

If an attacker only has _sensor lock_ to the target, they suffer a __Sensors Only__ attack penalty of +2 (_SensorsOnlyPenalty_).

If an attacker only has _visual lock_ to the target, they suffer a __Vision Only__ attack penalty based upon their distance to the target. For every 3 hexes away (_VisionOnlyRangeStep_) the attacker suffers a +1 attack penalty (_VisionOnlyPenalty_), regardless of the weapon used.

These penalties described can be adjusted by editing `LowVisibility/mod.json`.

### Zoom Vision

BattleTech has a long standing tradition of zoom vision being a standard feature on cockpits. To support this, components with the `lv-vismode-zoom_mX_sY` tag apply an attack bonus that decays over distance. Each point of X applies as a -1 bonus to the attack roll. For each Y hexes between the attacker and the target, the bonus is reduced by one, until no bonus is provided.

> Example: A unit has a component with tag `lv-vismode-zoom_m3_s8`. For any **ranged** attack between 0-8 hexes, the attacker applies a -3 attack bonus. For attacks between 9-16 hexes, the attacker applies a -2 bonus. At 17-24 hexes the bonus is -1, and at 25+ hexes there is no bonus.

This bonus only applies ranged attacks. This bonus does not stack with other vision bonuses. An attacker with multiple vismode components applies the highest bonus to an attack, plus +1 for each addition vismode that provides a bonus.


### Heat Vision

Like zoom vision, detecting an opponent through thermal vision has been a stable of BattleTech games back to MW2. Components with the `lv-vismode-heat_mX_dY` mimic this effect by applying an attack bonus that increases the as the target heats up. The attacker gains a -X bonus to their attack for each Y points of heat the target currently has. This bonus cannot exceed _HeatVisionMaxBonus_, defined in `LowVisibility/mod.json`.

> Example: A unit has a component with tag `lv-vismode-heat_m1_d20`. For any **ranged** attack where the target has 20 heat or less, the attack gains no bonus. If the target has 20-40 heat the attack has a -1 bonus, for 41-60 heat it has a -2 bonus, and so on.

This bonus only applies ranged attacks. This bonus does not stack with other vision bonuses. An attacker with multiple vismode components applies the highest bonus to an attack, plus +1 for each addition vismode that provides a bonus.

### Stealth Attack Modifiers

In addition to making sensor detection difficult, stealth can make it hard to attack a target at range. To reflect that _LowVisibility_ exposes a few tags that add special case modifiers to the attack roll.

* Components with the `lv-stealth-range-mod_sA_mB_lC_eD` apply a penalty to an attacker's weapons based upon their range to the target. A penalty of A is applied against targets at short range, B at medium range, C at long range, and D at extreme range.
* Components with the `lv-stealth-move-mod_mX_sZ` tag apply a penalty that is strongest when the target doesn't move. X is the base penalty that applies to any attack against the target. This penalty decays by 1 for each Z hexes the target moves, until it the penalty is completely eliminated.

> Example One: A tag of `lv-stealth-range-mod_s1_m2_l3_e4` would apply a +1 penalty at short range, +2 at medium range, +3 at long range and +4 at extreme range. This range is applied per weapon, so the attacker may experience different penalties for different weapons they fire.

> Example Two: A tag of `lv-stealth-move-mod_m3_s2` applies a +3 penalty to the attacker if the target does not move. If the target moves 1 or 2 hexes, this penalty would be reduced (by -1) to +2. If the target moves 3-4 hexes, the penalty is reduced to +1, and if the unit moves 5 hexes or more the penalty is completely removed.

## WIP

### 1.6 Bugs
* Stealth textures don't always load
* Prefix not showing for mimetic
* Vehicles need an oblate spheroid for stealth effect, not prolate.
* Better VFX definition for Stealth (instead of black bubble)
* ECM/Stealth/Mimetic applies a flat hexes reduction to signature/visibility, instead of a multiplicative one?
* Show EYE icon on enemies and friendlys for stealth, etc
* BUG: Visual penalties shouldn't be applied if firing indirectly

### WIP Features

- [ ] BUG: ticket-0304 in Discord - SensorDistanceAbsolute multipliers may not be adding to the total sensor distance value.
- [ ] BUG: Report of vehicle blips showing mech paperdoll instead of vehicle. Photo proof, so see if it can be reproduced.
- [ ] BUG: AIM calculations aren't accurate. Likely caused by LV going after AIM in the patch order?
- [ ] BUG: Offensive push shows damaged areas even with a crap information roll. LA suggestion: restrict offensive push to a minimum info roll.
- [ ] BUG: Debuff icons don't update when the sensor lock check is made, they only update after movement. Force an update somehow?
- [ ] BUG: Armor/structure exact values are being shown, not just as ??
- [ ] FEATURE: Show 'Cannon' / 'Missile' / 'Support' instead of 'Unidentified'
- [ ] FEATURE: Prevent called shot against blips
- [ ] FEATURE: Per LA, nerf multi-targeting but add  an item tag that helps/hurts. One tag that adds a penalty to each target. A second 'multitracker' that grants bonus to this (reduce penalty or bonus). Third, no multitargeting stealth w/o a multitracker. Maybe make the latter that you need a positive attack modifier from FCS/etc to multi-shoot against stealth? Have to think more.
- [ ] FEATURE: Should tactics limit equipment modifiers? I.e. tactics +2 means you can't make use of a +4 ECM gear. If you tactics equals the gear, maybe get a bonus?
- [ ] FEATURE: Wounds detracts from your EW check each round
- [ ] FEATURE: Show signature, visibility modifiers for target on the tooltips. Show same for player mechs.
- [ ] FEATURE: ImplementNoSensorLockCriticalMultiPenalty = 0.0f; NoVisualLockCriticalMultiPenalty = 0.0f; (Modify called shot critical, % when making a shot w/o lock)
- [ ] FEATURE: Implement stealth multi-target prohibition
- [ ] FEATURE: Reduce critical % chance when sensors only/visual only (VisionOnlyCriticalPenalty/SensorsOnlyCriticalPenalty)

### Possible Additions

- [] Add ability for a pilot to get a bad reading / critical failure. Tie to tactics as a roll, so poor pilots have it happen more often.  In failure, show wrong name/tonnage/information to confuse the player. May need some hidden marker indicating that this is a false lead - possibly a temporarily value that can be removed once we introduce the mod.
- [] Experiment with AllowRearArcSpotting:false in CombatGameConstants.json
- [] SensorLock becomes passive ability that doubles your tactics, boosts sensor detect level by +1 (for 'what you know'). Eliminate it as a menu item, ensure forceVisRebuild never gets called.
- [] Add a 'lv-vision-lowlight_rX' for low-light vision; increases visual range during a non-daylight mood

### To Document
- [x] BUG: SensorRange not respecting SensorRangeMinimum.
- [x] BUG: Narc, Tag not cleaned up on combat end. Causes some NPEs.
- [x] BUG: VisibilityRange not respecting VisionRangeminiumum
- [x] BUG - When you overheat a mech, it disappears from vision
- [x] VisionLock and VisualID ranges should be modified by equipment.
- [x] Fix issues with VisualID - make it apply if close enough
- [x] Hide pilot details when not DentalRecords
- [x] BUG: vismodes should only apply to ranged attacks
- [x] BUG: Stability & heat bars showing on vehicles & turrets for some reason.
- [x] BUG: Ranges should be normalized to a hex instead of partial hex (Math.Ceil to hex definition)
- [x] BUG: Enemies not following the sensor blackout on turn one; there are cases where they get to attack anyways.
- [x] Document - FiredWeaponsSignatureEffect from CombatGameConstants.json (causes blipping effect)
- [x] BUG: Have to select a unit after save for GameRepresentations to be rendered.
- [x] BUG: If you have sensor lock from a position, but not LOS, doesn't display the lines showing that you can shoot from there. How to fix? Looks to be related to AIM.
- [ ] BUG: Sensor range circle not updating onActorSelected; gives you a false sense of where you can see
