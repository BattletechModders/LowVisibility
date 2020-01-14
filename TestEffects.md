# Low Visibility Test Effects
This document maintains a library of test effects that are useful when developing.



## Advanced Sensors

Advanced sensors increase the detail level on the target.

```json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_ADV_SEN_6",
				"Name" : "Advanced Sensors - 6",
				"Details" : "Advanced sensors - gives more detailed information on detected ones.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_ADVANCED_SENSORS",
				"operation": "Int_Add",
				"modValue": "6",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
    ],
}

```





## ECM Emitter

An ECM emitter applies ECM Shield to allies, has an ECM Carrier value, and applies ECM Jamming to enemies. There are two auras, each with two effects, and a component-level status effect:

* Gear_LV_Test_ECM_Shield has an effect that applies the 

```json
"Auras": [
    {
      "Id": "Gear_LV_Test_ECM_Shield",
      "Name": "ECM",
      "ReticleColor": "#00f2ff",
      "Range": 240,
      "RemoveOnSensorLock": false,
      "State": "Persistent",
      "ApplySelf": false,
      "AllyStealthAffection": "None",
      "EnemyStealthAffection": "None",
      "IsPositiveToAlly": true,
      "IsNegativeToEnemy": false,
      "ownerVFX": [ ],
      "targetVFX": [ ],
      "removeOwnerVFX": [ ],
      "removeTargetVFX": [ ],
      "ownerSFX": [ ],
      "targetSFX": [ ],
      "removeOwnerSFX": [ ],
      "removeTargetSFX": [ ],
      "statusEffects": [
            {
                "durationData" : {
                    "duration": -1,
                    "stackLimit": -1
                },
                "targetingData" : {
                    "effectTriggerType" : "Passive",
                    "specialRules" : "NotSet",
                    "effectTargetType" : "AlliesWithinRange",
                    "range" : 0.0,
                    "forcePathRebuild" : false,
                    "forceVisRebuild" : false,
                    "showInTargetPreview" : false,
                    "showInStatusPanel" : false
                },
                "effectType" : "StatisticEffect",
                "Description" :
                {
                    "Id" : "LV_ECM_SHIELD",
                    "Name" : "ECM Shield Protection",
                    "Details" : "An ECM shield that makes it harder for enemies to detect you with sensors, and resolve attacks.",
                    "Icon" : "uixSvgIcon_status_ECM-missileDef"
                },
                "statisticData" : 
                {
                    "statName" : "LV_ECM_SHIELD",
                    "operation": "Int_Add",
                    "modValue": "1",
                    "modType": "System.Int32"
                },
                "nature" : "Buff"
            },
            {
                "durationData" : {
                    "duration": -1,
                    "stackLimit": -1
                },
                "targetingData" : {
                    "effectTriggerType" : "Passive",
                    "specialRules" : "NotSet",
                    "effectTargetType" : "AlliesWithinRange",
                    "range" : 0.0,
                    "forcePathRebuild" : false,
                    "forceVisRebuild" : false,
                    "showInTargetPreview" : false,
                    "showInStatusPanel" : false
                },
                "effectType" : "StatisticEffect",
                "Description" :
                {
                    "Id" : "LV_ECM_SHIELD_EMITTER_COUNT",
                    "Name" : "ECM Shield Emitter Count",
                    "Details" : "Counter for the number of ECM emitters currently affecting an actor with shielding.",
                    "Icon" : "uixSvgIcon_status_ECM-missileDef"
                },
                "statisticData" : 
                {
                    "statName" : "LV_ECM_SHIELD_EMITTER_COUNT",
                    "operation": "Int_Add",
                    "modValue": "1",
                    "modType": "System.Int32"
                },
                "nature" : "Buff"
            }
		]
    },
    {
      "Id": "Gear_LV_Test_ECM_Jammer",
      "Name": "JAMMER",
      "ReticleColor": "#0066ff",
      "Range": 240,
	  "HideOnNotSelected": false,
      "RemoveOnSensorLock": false,
      "State": "Persistent",
      "ApplySelf": false,
      "AllyStealthAffection": "None",
      "EnemyStealthAffection": "None",
      "IsPositiveToAlly": false,
      "IsNegativeToEnemy": true,
      "ownerVFX": [ ],
      "targetVFX": [ ],
      "removeOwnerVFX": [ ],
      "removeTargetVFX": [ ],
      "ownerSFX": [ "AudioEventList_ecm_ecm_enter", "AudioEventList_ui_ui_ecm_start" ],
      "targetSFX": [ "AudioEventList_ecm_ecm_enter" ],
      "removeOwnerSFX": [ "AudioEventList_ecm_ecm_exit", "AudioEventList_ui_ui_ecm_stop" ],
      "removeTargetSFX": [ "AudioEventList_ecm_ecm_exit" ],
      "statusEffects": [
        {
            "durationData" : {
                "duration": -1,
                "stackLimit": -1,
                "uniqueEffectIdStackLimit": 1
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "specialRules" : "NotSet",
                "effectTargetType" : "EnemiesWithinRange",
                "range" : 0.0,
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : false
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
                "Id" : "LV_ECM_JAMMED",
                "Name" : "ECM Jammer",
                "Details" : "Jamming ECM, reduces sensor checks for enemies in field",
                "Icon" : "uixSvgIcon_action_sensorlock"
            },
            "statisticData" : 
            {
                "statName" : "LV_ECM_JAMMED",
                "operation": "Int_Add",
                "modValue": "1",
                "modType": "System.Int32"
            },
            "nature" : "Debuff"
        },
        {
                "durationData" : {
                    "duration": -1,
                    "stackLimit": -1
                },
                "targetingData" : {
                    "effectTriggerType" : "Passive",
                    "specialRules" : "NotSet",
                    "effectTargetType" : "EnemiesWithinRange",
                    "range" : 0.0,
                    "forcePathRebuild" : false,
                    "forceVisRebuild" : false,
                    "showInTargetPreview" : false,
                    "showInStatusPanel" : false
                },
                "effectType" : "StatisticEffect",
                "Description" :
                {
                    "Id" : "LV_ECM_JAM_EMITTER_COUNT",
                    "Name" : "ECM Jamming Emitter Count",
                    "Details" : "Counter for the number of ECM emitters currently affecting an actor with jamming.",
                    "Icon" : "uixSvgIcon_status_ECM-missileDef"
                },
                "statisticData" : 
                {
                    "statName" : "LV_ECM_JAM_EMITTER_COUNT",
                    "operation": "Int_Add",
                    "modValue": "1",
                    "modType": "System.Int32"
                },
                "nature" : "Debuff"
            }
      ]
    }
],
"statusEffects": [
    {
        "durationData" : {
            "duration": -1,
            "stackLimit": -1
        },
        "targetingData" : {
            "effectTriggerType" : "Passive",
            "specialRules" : "NotSet",
            "effectTargetType" : "Creator",
            "range" : 0.0,
            "forcePathRebuild" : false,
            "forceVisRebuild" : true,
            "showInTargetPreview" : false,
            "showInStatusPanel" : false
        },
        "effectType" : "StatisticEffect",
        "Description" :
        {
            "Id" : "LV_ECM_Carrier_Test_2",
            "Name" : "ECM Carrier",
            "Details" : "You emit an ECM field that makes you easier to detect with sensors, but harder to hit.",
            "Icon" : "uixSvgIcon_status_ECM-missileDef"
        },
        "statisticData" : 
        {
            "statName" : "LV_ECM_CARRIER",
            "operation": "Set",
            "modValue": "2",
            "modType": "System.Int32"
        },
        "nature" : "Buff"
    }
]
```



## Mimetic Carrier

Mimetic carriers are harder to detect visually, as well as attack.

```json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_MIMETIC_3",
				"Name" : "Mimetic",
				"Details" : "Mimetic system - while active you are harder to detect visually and attack.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_MIMETIC",
				"operation": "Int_Add",
				"modValue": "3",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
    ],
}

```



## Probe Carrier

Probe carriers have powerful sensors that make it easier to detect other units, and provides more information on those units.

```
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_PROBE_CARRIER_3",
				"Name" : "Probe Carrier - 3",
				"Details" : "Probe carrier - makes enemy units easier to detect, and provides more information on them.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_PROBE_CARRIER",
				"operation": "Int_Add",
				"modValue": "3",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
    ],
}

```





## Stealth Carrier

Stealth carriers are harder to detect with sensors, as well as attack.

```json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_Stealth_4",
				"Name" : "Stealth",
				"Details" : "Stealth system - while active you are harder to detect with a sensors and  attack.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_STEALTH",
				"operation": "Int_Add",
				"modValue": "4",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
    ],
}

```



# Vision Attack Effects

These effects provide attack bonuses based upon detection range.

## Heat Vision

This effect provides an attack bonus based upon the target's heat.

```json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_HEAT_VISION",
				"Name" : "Heat Vision",
				"Details" : "Provides attack bonuses based upon the target's heat.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_HEAT_VISION",
				"operation": "Set",
				"modValue": "true",
				"modType": "System.String"
            },
            "nature" : "Buff"
        },
    ],
}

```





## Zoom Vision

This effect provides an attack bonus based upon distance to the target.

```
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_NIGHT_VISION",
				"Name" : "Night Vision",
				"Details" : "Provides extended vision range at night.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_NIGHT_VISION",
				"operation": "Set",
				"modValue": "true",
				"modType": "System.Boolean"
            },
            "nature" : "Buff"
        },
    ],
}

```





# Vision Effects

These effects provide certain visual changes to the battlefield.

## Low Light Vision

Units with low light vision have extended vision range at night, but it will be shown with a green effect.

```json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_NIGHT_VISION",
				"Name" : "Night Vision",
				"Details" : "Provides extended vision range at night.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_NIGHT_VISION",
				"operation": "Set",
				"modValue": "true",
				"modType": "System.Boolean"
            },
            "nature" : "Buff"
        },
    ],
}

```





## Vision Sharing

Units with vision sharing will combine their visual detection range, allowing them to see units within their range.

````json
{
    "statusEffects" : [
            {
            "durationData" : {
                "duration" : -1,
                "ticksOnActivations" : false,
                "useActivationsOfTarget" : false,
                "ticksOnEndOfRound" : false,
                "ticksOnMovements" : false,
                "stackLimit" : 1,
                "clearedWhenAttacked" : false
            },
            "targetingData" : {
                "effectTriggerType" : "Passive",
                "effectTargetType" : "Creator",
                "forcePathRebuild" : false,
                "forceVisRebuild" : false,
                "showInTargetPreview" : false,
                "showInStatusPanel" : true
            },
            "effectType" : "StatisticEffect",
            "Description" :
            {
				"Id" : "LV_SHARES_VISION",
				"Name" : "Vision Sharing",
				"Details" : "Shares vision with other units in the lance, allowing them to see what you see.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_SHARES_VISION",
				"operation": "Set",
				"modValue": "true",
				"modType": "System.Boolean"
            },
            "nature" : "Buff"
        },
    ],
}

````



# Active Effects

Some effects should not be applied via auras or passive effects, but rather from active effects.



## Probe Ping Effect

Loreum ipsum

## Narc Effect

Loreum ipsum

## Tag Effect

Loreum ipsum