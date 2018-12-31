# Low Visibility
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game 

### Features


FrostRaptor: What are some relevant tags/test units I should look at?
[11:39 AM] LadyAlekto: ratprs, guerilla, dagger/rapier

items would be anything with the ecm/advecm category or stealth
[11:41 AM] Khan Damien King: Stealth, NSS, Void System, Nova CEWS, WatchDog Sui

Stealth, NSS, Void System, Nova CEWS, WatchDog Suit.

[11:41 AM] FrostRaptor: So if you have ecm, you can't be scanned for what? Components only? Advanced_ecm means you can't be  scanned for  weapons/armor values?
[11:42 AM] FrostRaptor: Stealth would be what, reduce your visibility range if you don't move?
[11:43 AM] FrostRaptor: I ask b/c there's player information (the targeting HUD) but also visibility
[11:43 AM] LadyAlekto: id say, base ecm preventing standard readouts and the higher the level the harder it is to retain the info
[11:43 AM] LadyAlekto: id need some brain juice to have a good concept though

[11:55 AM] jo: gunnery check vs tactics with bonuses based on respective equipment
[11:56 AM] jo: interesting effect would be
[11:56 AM] jo: to keep someone cloaked "red" even if they are in distance if they roll well enough
[11:56 AM] jo: or have good enough stuff
[11:56 AM] jo: basically untargetable bad guys if you are a scrub

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
    - Priate ECM
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
    medium range +1 to hit, long range +2 to thi

Null Signature Shield
    (Same as Stealth armor, but description is odd)

BaseSpotterDistance: 350
BaseSensorDistance: 500

SpotterRange is BaseSpotterDistance * allSpotterMultipliers + AllSpotterAbsolutes
SensorRange is BaseSensorDistance * SensorRangeMultipliers + SensorRangeAbsolute
TargetVisibility is 1f + TargetVisMultipliers + TargetVisibilityAbsolute

AdjustedSpotterRange = spotterRange - targetVisibility

MaxTech Rules:

Visual Range:  Daylight: 1800m / 450 for twilight / 300 for rain or smoke / 150 for darkness

Bloodhound Active Probe: 480 / 960 / 1440
Clan Active Probe: 450 / 900 / 1350
Beagle Active Probe: 360 / 720 / 1080
Mech Sensor range: 240 / 480 / 720
Vehicle Range: 180 / 360 / 540


- Visibility will tell you it's a Catapult, but not a CPLT-C1
- APs reveal all details about a model so long as the model is within their AOE
- Standard sensors give you a progression of answers (could re-use what I have for KYF)
- Visual inspection only within 3 hexes, can only show external details
- 