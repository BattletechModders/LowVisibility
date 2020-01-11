# Low Visibility Test Effects
This document maintains a library of test effects that are useful when developing.



## ECM Emitter

An ECM emitter applies ECM Shield to allies, has an ECM Carrier value, and applies ECM Jamming to enemies. There are two auras, each with two effects, and a component-level status effect:

* Gear_LV_Test_ECM_Shield has an effect that applies the 

```
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
                "stackLimit": 1,
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
                "Id" : "LV_ECM_JAMMED_1",
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





## ECM Shield

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
				"Id" : "LV_ECM_Carrier_Test_2",
				"Name" : "ECM Carrier",
				"Details" : "ECM Carrier - you emit an ECM field. You are easier to detect with sensors, but harder to hit. Friendly units in range are harder to hit and detect with sensors.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_ECM_CARRIER",
				"operation": "Int_Add",
				"modValue": "2",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
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
				"Id" : "LV_ECM_SHIELD",
				"Name" : "ECM Shield",
				"Details" : "ECM Shielding - you are protected by another unit's ECM effect. You are harder to hit and detect with sensors.",
				"Icon" : "uixSvgIcon_status_ECM-missileDef"
            },
            "statisticData" : 
            {
				"statName" : "LV_ECM_SHIELD",
				"operation": "Int_Add",
				"modValue": "2",
				"modType": "System.Int32"
            },
            "nature" : "Buff"
        },
    ],
}

```

