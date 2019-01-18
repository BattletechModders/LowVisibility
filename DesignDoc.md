# Low Visibility Design Notes

## Vanilla Behavior
Before talking about the mechanics of this mod, it's helpful to understand the vanilla behavior. In vanilla [HBS BattleTech](http://battletechgame.com/), the detection model allows for opposing units to be in one of four states:
  * Undetected when outside of sensor range
  * Detected as a blob (the down-pointing arrow)
  * Detected as a specific type (the mech, turret or vehicle silhouette)
  * Visual detection with full details displayed

The range of visual detection is determined by a base spotting distance, which defaults to 300 (set in SimGameConstants). This value is modified by the spotting unit's `SpottingVisibilityMultiplier` and `SpottingVisibilityAbsolute` values. This value is then modified by the targets's `SpottingVisibilityMultiplier` and `SpottingVisibilityAbsolute` values. If this modified spotting distance is less than the distance to the target, the spotter is considered to have _LineOfSight_ to the target. Once one spotter on a team has _LineOfSight_ to a target, all other team members share that visibility.

If a target is outside of direct _LineOfSight_, then sensors are checked. The detecting unit's `SensorDistanceMultiplier` is added to any terrain `DesignMask.sensorRangeMultiplier`, while `SensorDistanceAbsolute` is added to the result. The target's `SensorSignatureModifier` modifies this value, and the final result is compared against a sensor detection range of 500 (also set in SimGameConstants). If the target is within range, it's identified as a sensor blip. The type of blip is influenced by the tactics level of the spotter. In these cases the _LineOfSight_ value is given as Blip or Blob.

The __Sensor Lock__ ability provides _LineOfSight_ to the target model, even if it's only within sensor range.

Most HBS logic requires a _LineOfSight_ for units to act upon. They refer to this as a _VisibilityLevel_. Units with _VisibilityLevel.None_ are effectively treated as if they don't exist.

## Concept
Notes: We don't follow the tech manual, we follow MaxTech. So Angel doesn't defeat bloodhound, it just makes it harder for those probes to find units. There is no 'completely blocking' - we're simulating that at a soft level with information hiding.

### Discarded Ideas

- [x] Consider: _Possible Info_ elements are randomly selected each round / actor (simulate one question note). __Want to eliminate randomness and focus on core mechanics__
- [x] Consider: Chance for VisualID to fail based upon a random roll __Want to eliminate randomness and focus on core mechanics__
- [x] SensorLock.SensorsID should randomly provide one piece of information about the target (armor, weapons, heat, ...?) __Want to eliminate randomness and focus on core mechanics__
- [x] Pilot tactics should provide a better guess of weapon types for _VisualID_. __Want to eliminate randomness and focus on core mechanics__
- [x] Add https://github.com/jeromerg/NGitVersion to build version number automatically. __Not nearly has useful as it looked, b/c we are on .NET 3.5__
    ​		

#### AbstractActor stats
AbstractActor relevant statistics:

```
this.StatCollection.AddStatistic<float>("ToHitIndirectModifier", 0f);
this.StatCollection.AddStatistic<float>("AccuracyModifier", 0f);
this.StatCollection.AddStatistic<float>("CalledShotBonusMultiplier", 1f);
this.StatCollection.AddStatistic<float>("ToHitThisActor", 0f);
this.StatCollection.AddStatistic<float>("ToHitThisActorDirectFire", 0f);
this.StatCollection.AddStatistic<bool>("PrecisionStrike", false);
this.StatCollection.AddStatistic<int>("MaxEvasivePips", 4);

AbstractActor:
​		public int EvasivePipsCurrent { get; set; }
​		public float DistMovedThisRound { get; set; }		
```

#### Book Info

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
* Stealth Armor
  * cannot be a secondary target
  * adds flat +1 at medium range, +2 at long range
  * ECM does not function, but 'Mech suffers effects as if in the radius of an enemy ECM suite
  * Requires ECM
* Null-Signature System (TacticalOperations: p336)
  * Cannot be detected by BAP, only Bloodhound, CEWS
  * Doesn't require ECM
  * Any critical shuts down the system
  * adds flat +1 at medium range, +2 at long range
  * Can stack with Chameleon
* Void Signature System (TacticalOperations: P349)
  * Can only be detected by a Bloodhound, CEWS - hidden from BAP, below
  * Requires an ECM unit
  * Any critical shuts down the system, as does losing the ECM
  * 0 movement = +3 penalty to hit
  * 1-2 hexes = +2 penalty to hit
  * 3-5 hexes = +1 penalty to hit
  * 6+ hexes = no penalty to hit
* Chameleon Light Polarization Shield (TacticalOperations: p300)
  * medium range +1 to hit, long range +2 to hit
  * Reduces visibility based upon range as well?
